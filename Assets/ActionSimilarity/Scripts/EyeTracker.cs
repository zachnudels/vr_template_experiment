using System;
using System.Collections.Generic;
using UnityEngine;
using ActionSimilarity;

using UXF;
using ViveSR.anipal.Eye;

/// <summary>
/// EyeTracker that does ZERO SRanipal calls. It reads two fields that must be
/// updated elsewhere (e.g., in SRanipalGazeRaySample):
///   - public EyeData_v2 eyeData
///   - public Vector3   gazeDirectionCombined
/// Set gazeIsWorldSpace depending on how you feed gazeDirectionCombined.
/// </summary>
public class EyeTracker : UXF.Tracker
{
    // [Header("Upstream eye feed (set by another component)")]
    // [Tooltip("Latest EyeData_v2 (should be kept fresh elsewhere, SRanipalGazeRaySample_v2)")]
    public EyeData_v2 eyeData { get; set; }

    // [Tooltip("Combined gaze direction vector supplied by another script: SRanipalGazeRaySample_v2")]
    public Vector3 gazeDirectionCombined = Vector3.zero;

    // [Tooltip("True if gazeDirectionCombined is already in world space; false if it's camera/local space")]
    public bool gazeIsWorldSpace = true;

    // [Header("Scene refs")]
    public Session session;
    public Camera cam;
    // [Tooltip("Right hand/controller transform (assign in Inspector)")]
    public Transform rightHandTf;
    // [Tooltip("Fixation target transform (assign at trial start if dynamic)")]
    public ActionSimilarity.Trial trial;

    private Transform fixationTf;

    // [Header("Gaze → Physics")]
    public float maxFocusDistance = 100f;
    public LayerMask focusMask = 0;

    // [Header("Debug")]
    public bool debug = false;

    // --- Cached per-row values (no allocations in hot path) ---
    bool   _hasGaze;
    Vector3 _gazeDirWorld;
    Vector2 _gazeAngle;

    bool   _hasViewGaze;
    Vector3 _viewGazeViewport;

    bool   _hasFixationTriang;
    Vector3 _fixationPointWorld;

    float _eyeOpenL = float.NaN, _eyeOpenR = float.NaN;
    float _pupilDiaL = float.NaN, _pupilDiaR = float.NaN;
    Vector2 _pupilSensorL = new Vector2(float.NaN, float.NaN);
    Vector2 _pupilSensorR = new Vector2(float.NaN, float.NaN);

    // physics non-alloc
    static readonly RaycastHit[] _oneHit = new RaycastHit[1];

    public override string MeasurementDescriptor => "eye_tracking";
    public override IEnumerable<string> CustomHeader => new string[] {
        "TriggerCode","Block","Trial",
        "GazeDirX","GazeDirY","GazeDirZ",
        "HeadPosX","HeadPosY","HeadPosZ",
        "HeadDirX","HeadDirY","HeadDirZ",
        "EyeOpenL","EyeOpenR","PupilDiaL","PupilDiaR",
        "PupilSensorLx","PupilSensorLy","PupilSensorRx","PupilSensorRy",
        "GazeAngleX","GazeAngleY","HeadAngleX","HeadAngleY",
        "ViewGazeX","ViewGazeY","ViewGazeZ",
        "ViewFixX","ViewFixY","ViewFixZ",
        "HandRPosX","HandRPosY","HandRPosZ",
        "HandRDirX","HandRDirY","HandRDirZ",
        "FixationPointX","FixationPointY","FixationPointZ",
    };

    void Awake()
    {
        session = Session.instance;
        if (!cam) cam = Camera.main; // cache once
        fixationTf = trial.fixationSettings.fixationSphere.transform;
    }

    // ------- Main logging path (no SRanipal calls) -------
    protected override UXFDataRow GetCurrentValues()
    {
        if (session == null) session = Session.instance;

        // Derive everything we need from the two inputs (eyeData + gazeDirectionCombined)
        DeriveFromUpstreamInputs();

        var row = new UXFDataRow();

        // Session / trigger
        row.Add(("Trial", session.currentTrialNum));
        row.Add(("Block", session.currentBlockNum));
        int trigger = session.settings.GetInt("triggerCode");
        row.Add(("TriggerCode", trigger));
        if (trigger != 0) session.settings.SetValue("triggerCode", 0);

        // Gaze direction & angles
        if (_hasGaze)
        {
            row.Add(("GazeDirX", _gazeDirWorld.x));
            row.Add(("GazeDirY", _gazeDirWorld.y));
            row.Add(("GazeDirZ", _gazeDirWorld.z));
            row.Add(("GazeAngleX", _gazeAngle.x));
            row.Add(("GazeAngleY", _gazeAngle.y));
        }
        else AddNaNs(row, "GazeDirX","GazeDirY","GazeDirZ","GazeAngleX","GazeAngleY");

        // Head pose & angles
        var ht = cam.transform;
        var headPos = ht.position; var headDir = ht.forward;
        row.Add(("HeadPosX", headPos.x)); row.Add(("HeadPosY", headPos.y)); row.Add(("HeadPosZ", headPos.z));
        row.Add(("HeadDirX", headDir.x)); row.Add(("HeadDirY", headDir.y)); row.Add(("HeadDirZ", headDir.z));
        var headAngle = GetVectorAngle(headDir);
        row.Add(("HeadAngleX", headAngle.x)); row.Add(("HeadAngleY", headAngle.y));

        // Eye openness / pupils / sensor pos
        row.Add(("EyeOpenL", _eyeOpenL)); row.Add(("EyeOpenR", _eyeOpenR));
        row.Add(("PupilDiaL", _pupilDiaL)); row.Add(("PupilDiaR", _pupilDiaR));
        row.Add(("PupilSensorLx", _pupilSensorL.x)); row.Add(("PupilSensorLy", _pupilSensorL.y));
        row.Add(("PupilSensorRx", _pupilSensorR.x)); row.Add(("PupilSensorRy", _pupilSensorR.y));

        // Viewport of fixation target (if provided)
        if (fixationTf)
        {
            var vf = cam.WorldToViewportPoint(fixationTf.position);
            row.Add(("ViewFixX", vf.x)); row.Add(("ViewFixY", vf.y)); row.Add(("ViewFixZ", vf.z));
        }
        else AddNaNs(row, "ViewFixX","ViewFixY","ViewFixZ");

        // Viewport of gaze hit
        if (_hasViewGaze)
        {
            row.Add(("ViewGazeX", _viewGazeViewport.x));
            row.Add(("ViewGazeY", _viewGazeViewport.y));
            row.Add(("ViewGazeZ", _viewGazeViewport.z));
        }
        else AddNaNs(row, "ViewGazeX","ViewGazeY","ViewGazeZ");

        // Right hand pose (cached Transform)
        if (rightHandTf)
        {
            var hp = rightHandTf.position; var hd = rightHandTf.forward;
            row.Add(("HandRPosX", hp.x)); row.Add(("HandRPosY", hp.y)); row.Add(("HandRPosZ", hp.z));
            row.Add(("HandRDirX", hd.x)); row.Add(("HandRDirY", hd.y)); row.Add(("HandRDirZ", hd.z));
        }
        else AddNaNs(row, "HandRPosX","HandRPosY","HandRPosZ","HandRDirX","HandRDirY","HandRDirZ");

        // Triangulated fixation from L/R (optional; computed below)
        if (_hasFixationTriang)
        {
            row.Add(("FixationPointX", _fixationPointWorld.x));
            row.Add(("FixationPointY", _fixationPointWorld.y));
            row.Add(("FixationPointZ", _fixationPointWorld.z));
        }
        else AddNaNs(row, "FixationPointX","FixationPointY","FixationPointZ");

        return row;
    }

    // --------- Derive metrics from upstream inputs (no SDK calls) ---------
    private void DeriveFromUpstreamInputs()
    {
        // Pull scalar eye metrics directly
        var v = eyeData.verbose_data;
        _eyeOpenL     = v.left.eye_openness;
        _eyeOpenR     = v.right.eye_openness;
        _pupilDiaL    = v.left.pupil_diameter_mm;
        _pupilDiaR    = v.right.pupil_diameter_mm;
        _pupilSensorL = v.left.pupil_position_in_sensor_area;
        _pupilSensorR = v.right.pupil_position_in_sensor_area;

        // Build a gaze origin from L/R (openness-weighted), in WORLD space
        Vector3 originWorld = ComputeGazeOriginWorld(v, cam);

        // Decide world-space gaze direction
        if (gazeDirectionCombined == Vector3.zero)
        {
            _hasGaze = false;
            _gazeDirWorld = Vector3.zero;
            _gazeAngle = new Vector2(float.NaN, float.NaN);
            _hasViewGaze = false;
            _viewGazeViewport = new Vector3(float.NaN, float.NaN, float.NaN);
        }
        else
        {
            _gazeDirWorld = gazeIsWorldSpace
                ? gazeDirectionCombined.normalized
                : cam.transform.TransformDirection(gazeDirectionCombined).normalized;

            _gazeAngle = GetVectorAngle(_gazeDirWorld);
            _hasGaze = true;

            if (debug) Debug.DrawRay(originWorld, _gazeDirWorld * 2f, Color.red, 0f);

            // Physics focus (single non-alloc raycast)
            int hits = Physics.RaycastNonAlloc(originWorld, _gazeDirWorld, _oneHit,
                                               maxFocusDistance, focusMask,
                                               QueryTriggerInteraction.Ignore);

            if (hits > 0)
            {
                _viewGazeViewport = cam.WorldToViewportPoint(_oneHit[0].point);
                _hasViewGaze = true;
            }
            else
            {
                _viewGazeViewport = new Vector3(float.NaN, float.NaN, float.NaN);
                _hasViewGaze = false;
            }
        }

        // Optional: triangulate fixation from L/R rays (cheap math, no IO)
        _hasFixationTriang = TryTriangulateFixationWorld(v, cam, out _fixationPointWorld);
    }

    private static Vector3 ComputeGazeOriginWorld(in VerboseData v, Camera c)
    {
        // Pull L/R origins in camera space (mm), flip X (Vive), convert to meters
        Vector3 oL = v.left.gaze_origin_mm;  oL.x *= -1f;  oL *= 0.001f;
        Vector3 oR = v.right.gaze_origin_mm; oR.x *= -1f;  oR *= 0.001f;

        float wL = Mathf.Clamp01(v.left.eye_openness);
        float wR = Mathf.Clamp01(v.right.eye_openness);
        float s  = wL + wR;

        Vector3 oCam = (s > 1e-6f) ? (oL * wL + oR * wR) / s : oL;
        return c.transform.TransformPoint(oCam);
    }

    private bool TryTriangulateFixationWorld(in VerboseData v, Camera c, out Vector3 intersectionW)
    {
        // Left ray (camera space) → world
        Vector3 oL = v.left.gaze_origin_mm;  oL.x *= -1f; oL *= 0.001f;
        Vector3 dL = v.left.gaze_direction_normalized; dL.x *= -1f;
        // Right ray
        Vector3 oR = v.right.gaze_origin_mm;  oR.x *= -1f; oR *= 0.001f;
        Vector3 dR = v.right.gaze_direction_normalized; dR.x *= -1f;

        var t = c.transform;
        Vector3 oLw = t.TransformPoint(oL);
        Vector3 oRw = t.TransformPoint(oR);
        Vector3 dLw = t.TransformDirection(dL);
        Vector3 dRw = t.TransformDirection(dR);

        return LineLineIntersection(out intersectionW, oLw, dLw, oRw, dRw);
    }

    // --------- Utilities ---------
    private void AddNaNs(UXFDataRow row, params string[] headers)
    {
        for (int i = 0; i < headers.Length; i++) row.Add((headers[i], float.NaN));
    }

    public static bool LineLineIntersection(out Vector3 intersection,
         Vector3 linePoint1, Vector3 lineDirection1,
         Vector3 linePoint2, Vector3 lineDirection2)
    {
        Vector3 v3 = linePoint2 - linePoint1;
        Vector3 c12 = Vector3.Cross(lineDirection1, lineDirection2);
        Vector3 c32 = Vector3.Cross(v3, lineDirection2);
        float planar = Vector3.Dot(v3, c12);

        if (Mathf.Abs(planar) < 0.05f && c12.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(c32, c12) / c12.sqrMagnitude;
            intersection = linePoint1 + (lineDirection1 * s);
            return true;
        }
        intersection = Vector3.zero;
        return false;
    }

    public float GetAngleOnAxis(Vector3 dir1, Vector3 dir2, Vector3 axis)
    {
        Vector3 p1 = Vector3.Cross(axis, dir1);
        Vector3 p2 = Vector3.Cross(axis, dir2);
        return Vector3.SignedAngle(p1, p2, axis);
    }

    public Vector2 GetVectorAngle(Vector3 vec)
    {
        float x = GetAngleOnAxis(Vector3.forward, vec, Vector3.up);
        float y = GetAngleOnAxis(Vector3.forward, vec, Vector3.left);
        return new Vector2(x, y);
    }
}
