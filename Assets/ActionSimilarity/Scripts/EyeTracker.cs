using System;
using System.Collections.Generic;
using UnityEngine;

using UXF;
using ViveSR.anipal.Eye;

public class EyeTracker : UXF.Tracker
{
    // ======= Public / Inspector =======
    [Header("Scene refs")]
    public Session session;
    public Camera cam;
    [Tooltip("Right hand/controller transform (assign in Inspector)")]
    public Transform rightHandTf;
    [Tooltip("Fixation target transform (assign at trial start if dynamic)")]
    public Transform fixationTf;
    
    /// <summary>
    /// These two fields are updated by the SRanipalGazeRaySample_v2 script.
    /// Please ensure this script is attached to a GameObject called EyeTracker
    /// so that the SRanipalGazeRaySample_v2 script can find it!
    /// </summary>
    public EyeData_v2 eyeData;
    public Vector3 gazeDirectionCombined;


    [Header("Gaze → Physics")]
    [Tooltip("Max focus distance for gaze raycast")]
    public float maxFocusDistance = 100f;
    [Tooltip("Layers considered focusable by gaze raycast")]
    public LayerMask focusMask = ~0;

    [Header("Rates")]
    [Tooltip("Physics (focus hit) computation rate; eye metrics stay at 90 Hz")]
    public int physicsHz = 0; // keep eyes at 90 Hz, decimate only the raycast

    [Header("Debug")]
    public bool debug = false;

    // ======= UXF table descriptor (match your columns) =======
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

    // ======= Internal state =======
    private bool simulating;

    // SRanipal bulk buffer, filled once per frame
    private EyeData_v2 _eyeData;
    private int _lastFrameSequence;

    // Cached per-sample values (read by GetCurrentValues)
    private bool _hasGaze;
    private Vector3 _gazeDirWorld;
    private Vector2 _gazeAngle;

    private bool _hasViewGaze;
    private Vector3 _viewGazeViewport;

    private bool _hasFixationTriang;
    private Vector3 _fixationPointWorld;

    private float _eyeOpenL = float.NaN, _eyeOpenR = float.NaN;
    private float _pupilDiaL = float.NaN, _pupilDiaR = float.NaN;
    private Vector2 _pupilSensorL = new Vector2(float.NaN, float.NaN);
    private Vector2 _pupilSensorR = new Vector2(float.NaN, float.NaN);

    // Non-alloc physics
    private static readonly RaycastHit[] _oneHit = new RaycastHit[1];
    private float _nextPhysicsT;

    // ======= Unity lifecycle =======
    void Awake()
    {
        session = Session.instance;
        if (!cam) cam = Camera.main; // cache reference once

        // Simulate vs Player (mirror your original logic)
        var simulateObj = GameObject.Find("Simulate");
        var playerObj   = GameObject.Find("Player");
        if (simulateObj != null && simulateObj.activeInHierarchy)
        {
            if (playerObj != null && playerObj.activeInHierarchy)
                throw new UnityException("Must choose to activate only one between Simulate and Player!");
            simulating = true;
        }
        else if (playerObj != null && playerObj.activeInHierarchy) simulating = false;
        else Debug.LogError("Neither Simulate nor Player is active in hierarchy");
    }

    void OnEnable()
    {
        var fw = SRanipal_Eye_Framework.Instance;
        if (fw != null) fw.EnableEyeDataCallback = true; // make getters non-blocking
        _nextPhysicsT = Time.unscaledTime;
        _lastFrameSequence = 0;
    }

    void OnDisable()
    {
        var fw = SRanipal_Eye_Framework.Instance;
        if (fw != null) fw.EnableEyeDataCallback = false;
    }

    void Update()
    {
        if (simulating)
        {
            ClearSample();
            return;
        }

        // Ensure framework is up to avoid stalls
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING)
        {
            ClearSample();
            return;
        }

        // ===== ONE BULK COPY PER FRAME (cheap with callback enabled) =====
        SRanipal_Eye_API.GetEyeData_v2(ref _eyeData);

        // If no fresh frame, keep previous derived results
        if (_eyeData.frame_sequence == _lastFrameSequence)
            return;
        _lastFrameSequence = _eyeData.frame_sequence;

        // ---------- Derive metrics from _eyeData (no SRanipal helper calls) ----------
        var v = _eyeData.verbose_data;

        // Eye openness / pupil metrics
        _eyeOpenL = v.left.eye_openness;
        _eyeOpenR = v.right.eye_openness;
        _pupilDiaL = v.left.pupil_diameter_mm;
        _pupilDiaR = v.right.pupil_diameter_mm;
        _pupilSensorL = v.left.pupil_position_in_sensor_area;
        _pupilSensorR = v.right.pupil_position_in_sensor_area;

        // Build a single gaze ray (prefer combined if your SDK exposes it)
        Vector3 oLocal, dLocal;
        if (!TryGetCombinedFromVerbose(v, out oLocal, out dLocal))
        {
            FuseLRByOpenness(v, out oLocal, out dLocal); // robust fallback
        }

        // To world space (Vive: flip X, convert mm→m for origins)
        var t = cam.transform;
        Vector3 originW = t.TransformPoint(FlipX_mmToM(oLocal));
        _gazeDirWorld   = t.TransformDirection(FlipX(dLocal)).normalized;
        _gazeAngle      = GetVectorAngle(_gazeDirWorld);
        _hasGaze        = true;

        if (debug) Debug.DrawRay(originW, _gazeDirWorld * 2f, Color.red, 0f);

        // Optional: triangulate fixation from L/R rays (disabled by default for perf)
        _hasFixationTriang = false; // set true and compute if you really need it

        // Gaze "focus" via physics — decimate to reduce spikes
        if (Time.unscaledTime >= _nextPhysicsT)
        {
            _nextPhysicsT = Time.unscaledTime + 1f / Mathf.Max(1, physicsHz);
            int hits = Physics.RaycastNonAlloc(
                originW, _gazeDirWorld,
                _oneHit, maxFocusDistance, focusMask, QueryTriggerInteraction.Ignore);

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
        // else keep previous _viewGazeViewport to avoid extra raycasts
    }

    // ======= UXF hot path: NO SRANIPAL CALLS HERE =======
    protected override UXFDataRow GetCurrentValues()
    {
        if (session == null) session = Session.instance;

        var row = new UXFDataRow();

        // Session info + trigger (edge-reset)
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
        else
        {
            AddNaNs(row, "GazeDirX","GazeDirY","GazeDirZ","GazeAngleX","GazeAngleY");
        }

        // Head pose & angles
        var ht = cam.transform;
        var headPos = ht.position;
        var headDir = ht.forward;
        row.Add(("HeadPosX", headPos.x));
        row.Add(("HeadPosY", headPos.y));
        row.Add(("HeadPosZ", headPos.z));
        row.Add(("HeadDirX", headDir.x));
        row.Add(("HeadDirY", headDir.y));
        row.Add(("HeadDirZ", headDir.z));
        var headAngle = GetVectorAngle(headDir);
        row.Add(("HeadAngleX", headAngle.x));
        row.Add(("HeadAngleY", headAngle.y));

        // Eye openness / pupils / sensor pos
        row.Add(("EyeOpenL", _eyeOpenL));
        row.Add(("EyeOpenR", _eyeOpenR));
        row.Add(("PupilDiaL", _pupilDiaL));
        row.Add(("PupilDiaR", _pupilDiaR));
        row.Add(("PupilSensorLx", _pupilSensorL.x));
        row.Add(("PupilSensorLy", _pupilSensorL.y));
        row.Add(("PupilSensorRx", _pupilSensorR.x));
        row.Add(("PupilSensorRy", _pupilSensorR.y));

        // Viewport of fixation target (if provided)
        if (fixationTf)
        {
            var vf = cam.WorldToViewportPoint(fixationTf.position);
            row.Add(("ViewFixX", vf.x));
            row.Add(("ViewFixY", vf.y));
            row.Add(("ViewFixZ", vf.z));
        }
        else
        {
            AddNaNs(row, "ViewFixX","ViewFixY","ViewFixZ");
        }

        // Viewport of gaze hit
        if (_hasViewGaze)
        {
            row.Add(("ViewGazeX", _viewGazeViewport.x));
            row.Add(("ViewGazeY", _viewGazeViewport.y));
            row.Add(("ViewGazeZ", _viewGazeViewport.z));
        }
        else
        {
            AddNaNs(row, "ViewGazeX","ViewGazeY","ViewGazeZ");
        }

        // Right hand pose (cached Transform)
        if (rightHandTf)
        {
            var hp = rightHandTf.position;
            var hd = rightHandTf.forward;
            row.Add(("HandRPosX", hp.x));
            row.Add(("HandRPosY", hp.y));
            row.Add(("HandRPosZ", hp.z));
            row.Add(("HandRDirX", hd.x));
            row.Add(("HandRDirY", hd.y));
            row.Add(("HandRDirZ", hd.z));
        }
        else
        {
            AddNaNs(row, "HandRPosX","HandRPosY","HandRPosZ","HandRDirX","HandRDirY","HandRDirZ");
        }

        // Optional: triangulated fixation point (disabled in Update for perf)
        if (_hasFixationTriang)
        {
            row.Add(("FixationPointX", _fixationPointWorld.x));
            row.Add(("FixationPointY", _fixationPointWorld.y));
            row.Add(("FixationPointZ", _fixationPointWorld.z));
        }
        else
        {
            AddNaNs(row, "FixationPointX","FixationPointY","FixationPointZ");
        }

        return row;
    }

    // ======= Helpers (no allocations) =======
    private void AddNaNs(UXFDataRow row, params string[] headers)
    {
        for (int i = 0; i < headers.Length; i++) row.Add((headers[i], float.NaN));
    }

    private void ClearSample()
    {
        _hasGaze = _hasViewGaze = _hasFixationTriang = false;
        _gazeDirWorld = Vector3.zero;
        _gazeAngle = new Vector2(float.NaN, float.NaN);
        _viewGazeViewport = new Vector3(float.NaN, float.NaN, float.NaN);
        _eyeOpenL = _eyeOpenR = _pupilDiaL = _pupilDiaR = float.NaN;
        _pupilSensorL = _pupilSensorR = new Vector2(float.NaN, float.NaN);
    }

    // Try to use a combined gaze from VerboseData if your SRanipal version exposes it robustly.
    // Return false to fall back to fusing L/R (recommended unless you've verified combined validity on your device).
    private bool TryGetCombinedFromVerbose(in VerboseData v, out Vector3 originLocal, out Vector3 dirLocal)
    {
        // Many SDK builds don't provide a reliable combined ray in VerboseData, so default to false.
        originLocal = default;
        dirLocal = default;
        return false;
    }

    // Fuse L/R rays by openness weights (works well in practice)
    private void FuseLRByOpenness(in VerboseData v, out Vector3 originLocal, out Vector3 dirLocal)
    {
        Vector3 oL = v.left.gaze_origin_mm;
        Vector3 dL = v.left.gaze_direction_normalized;
        Vector3 oR = v.right.gaze_origin_mm;
        Vector3 dR = v.right.gaze_direction_normalized;

        // Flip X to Unity convention and convert mm→m for origins
        dL = FlipX(dL); dR = FlipX(dR);
        oL = FlipX_mmToM(oL); oR = FlipX_mmToM(oR);

        float wL = Mathf.Clamp01(v.left.eye_openness);
        float wR = Mathf.Clamp01(v.right.eye_openness);
        float s  = wL + wR;

        if (s <= 1e-6f)
        {
            // Fall back to left
            originLocal = oL;
            dirLocal    = dL.normalized;
            return;
        }

        originLocal = (oL * wL + oR * wR) / s;
        dirLocal    = ((dL * wL + dR * wR) / s).normalized;
    }

    private static Vector3 FlipX(Vector3 v) { v.x = -v.x; return v; }
    private static Vector3 FlipX_mmToM(Vector3 v) { v.x = -v.x; return v * 0.001f; }

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

    // (Optional) If you ever re-enable triangulation, you can reuse your LineLineIntersection here.
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
}
