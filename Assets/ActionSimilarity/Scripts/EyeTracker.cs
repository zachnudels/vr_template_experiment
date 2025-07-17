using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UXF;
using ViveSR.anipal.Eye;


//
// Script for supporting Eye Tracking from the Vive Pro Eye with UXF. Inherits from Tracker.
// Attach this script to an object and then reference it in your Session component (UXF_Rig) under tracked objects.
// This is the same method you would attach a PositionRotationTracker.
//


public class EyeTracker : Tracker
{
    public Session session;
    public Camera cam;

    public float maxFocusDistance = 100f;
    public LayerMask focusMask;
    public bool debug;
    private bool simulating;
    public ActionSimilarity.Trial trial;

    private readonly GazeIndex[] idxPriority = new GazeIndex[] { GazeIndex.COMBINE, GazeIndex.LEFT, GazeIndex.RIGHT };

    void Awake()
    {
        session = Session.instance;
        GameObject simulateObj = GameObject.Find("Simulate");
        GameObject playerObj = GameObject.Find("Player");

        if (simulateObj != null && simulateObj.activeInHierarchy)
        {
            if (playerObj != null && playerObj.activeInHierarchy)
            {
                throw new UnityException("Must choose to activate only one between Simulate and Player!");
            }
            GameObject cameraOffset = GameObject.Find("CameraOffset");
            if (cameraOffset != null)
            {
                simulating = true;
            }
            else
            {
                Debug.LogError("Camera Offset not found under Simulate");
            }
        }
        else if (playerObj != null && playerObj.activeInHierarchy)
        {
            GameObject vrCam = GameObject.Find("VRCamera");
            if (vrCam != null)
            {
                simulating = false;
            }
            else
            {
                Debug.LogError("VRCamera not found under Player");
            }
        }
        else
        {
            Debug.LogError("Neither Simulate nor Player is active in hierarchy");
        }
    }
    
    
    // public void 

    protected override UXFDataRow GetCurrentValues()
    {

        if (session == null)
        {
            session = Session.instance;
        }
        var row = new UXFDataRow();

        RecordSession(row);
        RecordGaze(row);
        RecordEyeData(row);
        RecordEyeFocus(row);
        RecordHead(row);
        RecordHand(row);

        return row;
    }

    void RecordSession(UXFDataRow row)
    {
        row.Add(("Trial", session.currentTrialNum));
        row.Add(("Block", session.currentBlockNum));
        row.Add(("TriggerCode", session.settings.GetInt("triggerCode")));
    }

    void RecordHand(UXFDataRow row)
    {
        Vector3 hand_position = GameObject.FindGameObjectsWithTag("rightHand")[0].GetComponent<Transform>().position;
        row.Add(("HandRPosX", hand_position.x));
        row.Add(("HandRPosY", hand_position.y));
        row.Add(("HandRPosZ", hand_position.z));
        
        Vector3 hand_direction = GameObject.FindGameObjectsWithTag("rightHand")[0].GetComponent<Transform>().forward;
        row.Add(("HandRDirX", hand_direction.x));
        row.Add(("HandRDirX", hand_direction.y));
        row.Add(("HandRDirX", hand_direction.z));    }

    void RecordHead(UXFDataRow row)
    {
        Transform cameraTransform = cam.transform;
        Vector3 cameraPos = cameraTransform.position;
        Vector3 cameraDir = cameraTransform.forward;
        
        row.Add(("HeadPosX", cameraDir.x));
        row.Add(("HeadPosY", cameraDir.y));
        row.Add(("HeadPosZ", cameraDir.z));
        row.Add(("HeadDirX", cameraPos.x));
        row.Add(("HeadDirY", cameraPos.y));
        row.Add(("HeadDirZ", cameraPos.z));

        Vector2 headAngle = GetVectorAngle(cam.transform.forward); 
        row.Add(("HeadAngleX", headAngle.x));
        row.Add(("HeadAngleY", headAngle.y));
    }

    void RecordGaze(UXFDataRow row)
    {

        string[] headers = {
            // "gaze_origin_x", 
            // "gaze_origin_y",
            // "gaze_origin_z",
            "GazeDirX",
            "GazeDirY",
            "GazeDirZ",
        };
        
        if (simulating)
        {
            AddNans(headers, row);
            return;
        }
        Vector3 gazeOriginCombinedLocal = new Vector3(float.NaN, float.NaN, float.NaN);
        Vector3 gazeDirectionCombinedLocal = new Vector3(float.NaN, float.NaN, float.NaN); ;

        bool gaze = false;
        foreach (var idx in idxPriority)
        {
            gaze = SRanipal_Eye_v2.GetGazeRay(idx, out gazeOriginCombinedLocal, out gazeDirectionCombinedLocal);
            if (gaze) break;
        }

        if (gaze)
        {
            Vector3 gazeOriginCombined = cam.transform.TransformPoint(gazeOriginCombinedLocal);
            Vector3 gazeDirectionCombined = cam.transform.TransformDirection(gazeDirectionCombinedLocal);

            if (debug)
            {
                Debug.DrawRay(gazeOriginCombined, gazeDirectionCombined, Color.red);
            }

            // row.Add(("gaze_origin_x", gazeOriginCombined.x));
            // row.Add(("gaze_origin_y", gazeOriginCombined.y));
            // row.Add(("gaze_origin_z", gazeOriginCombined.z));
            row.Add(("GazeDirX", gazeDirectionCombined.x));
            row.Add(("GazeDirY", gazeDirectionCombined.y));
            row.Add(("GazeDirZ", gazeDirectionCombined.z));

            Vector2 gazeAngle = GetVectorAngle(gazeDirectionCombined);
            row.Add(("GazeAngleX", gazeAngle.x));
            row.Add(("GazeAngleX", gazeAngle.y));
        }
        else
        {
            AddNans(headers, row);
        }
    }


    void RecordEyeData(UXFDataRow row)
    {
        if (simulating)
        {
            string[] headers =
            {
                "EyeOpenL",
                "EyeOpenR", 
                "PupilDiaL", 
                "PupilDiaR",
                "FixationPointX",
                "FixationPointY",
                "FixationPointZ",
                "PupilSensorLx",
                "PupilSensorLy",
                "PupilSensorRx",
                "PupilSensorRy",
            };
            AddNans(headers, row);
            return;
        }
        VerboseData vEyeData = new VerboseData();
        bool success = SRanipal_Eye_v2.GetVerboseData(out vEyeData);
        

        if (success)
        {
            RecordFixation(vEyeData, row);
            row.Add(("PupilSensorLx", vEyeData.left.pupil_position_in_sensor_area.x));
            row.Add(("PupilSensorLy", vEyeData.left.pupil_position_in_sensor_area.y));
            row.Add(("PupilSensorRx", vEyeData.right.pupil_position_in_sensor_area.x));
            row.Add(("PupilSensorRy", vEyeData.right.pupil_position_in_sensor_area.y));

            row.Add(("EyeOpenL", vEyeData.left.eye_openness));
            row.Add(("EyeOpenR", vEyeData.right.eye_openness));
            row.Add(("PupilDiaL", vEyeData.left.pupil_diameter_mm));
            row.Add(("PupilDiaR", vEyeData.right.pupil_diameter_mm));
        }
    }

    void RecordFixation(VerboseData vEyeData, UXFDataRow row)
    {
        Vector3 origin_L = vEyeData.left.gaze_origin_mm;
        Vector3 direction_L = vEyeData.left.gaze_direction_normalized;
        Vector3 origin_R = vEyeData.right.gaze_origin_mm;
        Vector3 direction_R = vEyeData.right.gaze_direction_normalized;

        direction_L.x *= -1;
        direction_R.x *= -1;

        direction_L = Camera.main.transform.TransformDirection(direction_L);
        direction_R = Camera.main.transform.TransformDirection(direction_R);

        origin_L.x *= -1f;
        origin_R.x *= -1f;
        origin_L *= 0.001f;
        origin_R *= 0.001f;

        origin_L = Camera.main.transform.TransformPoint(origin_L);
        origin_R = Camera.main.transform.TransformPoint(origin_R);
        if (LineLineIntersection(out var intersection, origin_L, direction_L, origin_R, direction_R))
        {
            row.Add(("FixationPointX", intersection.x));
            row.Add(("FixationPointY", intersection.y));
            row.Add(("FixationPointZ", intersection.z));
        }
    }

    void RecordEyeFocus(UXFDataRow row)
    {

        string[] headers = { "focus_object_raw", "focus_point_x", "focus_point_y", "focus_point_z", "focus_distance" };
        if (simulating)
        {
            AddNans(headers, row);
            return;
        }
        Ray gazeRay = new Ray();
        FocusInfo focusInfo = new FocusInfo();
        bool focus = false;
        foreach (var idx in idxPriority)
        {
            focus = SRanipal_Eye_v2.Focus(idx, out gazeRay, out focusInfo, 0.1f, maxFocusDistance, focusMask);
            if (focus) break;
        }

        if (focus)
        {
            if (debug)
            {
                Debug.LogFormat("Focus object: {0}", focusInfo.transform.name);
            }
            
            Vector3 viewGaze = cam.WorldToViewportPoint(focusInfo.point);
            row.Add(("ViewGazeX", viewGaze.x));
            row.Add(("ViewGazeY", viewGaze.y));
            row.Add(("ViewGazeZ", viewGaze.z));


            //fixation cross
            //tm.text = "fixationcross";
            Vector3 screenPosFixation = cam.WorldToViewportPoint(trial.fixationSettings.fixationSphere.transform.position);
            row.Add(("ViewFixX", screenPosFixation.x));
            row.Add(("ViewFixY", screenPosFixation.y));
            row.Add(("ViewFixZ", screenPosFixation.z));

            row.Add(("focus_object_raw", focusInfo.transform.name));
            row.Add(("focus_point_x", focusInfo.point.x));
            row.Add(("focus_point_y", focusInfo.point.y));
            row.Add(("focus_point_z", focusInfo.point.z));
            row.Add(("focus_distance", focusInfo.distance));
        }
        else
        {
            AddNans(headers, row);
        }
    }

    protected override void SetupDescriptorAndHeader()
    {
        measurementDescriptor = "eye_tracking";
        customHeader = new string[]
        {
            
            // "tick",
            // "Tick",
            // "Time",
            "TriggerCode",
            "Block",
            "Trial",
            "GazeDirX",
            "GazeDirY",
            "GazeDirZ",
            "HeadPosX",
            "HeadPosY",
            "HeadPosZ",
            "HeadDirX",
            "HeadDirY",
            "HeadDirZ",
            "EyeOpenL",
            "EyeOpenR",
            "PupilDiaL",
            "PupilDiaR",
            "PupilSensorLx",
            "PupilSensorLy",
            "PupilSensorRx",
            "PupilSensorRy",
            "PositionLx",
            "PositionLy",
            "PositionRx",
            "PositionRy",
            "GazeAngleX",
            "GazeAngleY",
            "HeadAngleX",
            "HeadAngleY",
            "ViewGazeX",
            "ViewGazeY",
            "ViewGazeZ",
            "ViewFixX",
            "ViewFixY",
            "ViewFixZ",
            "HandRPosX",
            "HandRPosY",
            "HandRPosZ",
            "HandRDirX",
            "HandRDirY",
            "HandRDirZ",
            "FixationPointX",
            "FixationPointY",
            "FixationPointZ",
            // "trigger_code",
            // "trial",
            // "block",
            // "gaze_origin_x",
            // "gaze_origin_y",
            // "gaze_origin_z",
            // "gaze_direction_x",
            // "gaze_direction_y",
            // "gaze_direction_z",
            // "eye_openness_left",
            // "eye_openness_right",
            // "pupil_diameter_left",
            // "pupil_diameter_right",
            // "focus_object_raw",
            // "focus_point_x",
            // "focus_point_y",
            // "focus_point_z",
            // "focus_distance",
            // "head_pos_x",
            // "head_pos_y",
            // "head_pos_z",
            // "head_dir_x",
            // "head_dir_y",
            // "head_dir_z",
        };
    }

    private void AddNans(string[] headers, UXFDataRow row)
    {
        foreach (string header in headers)
        {
            row.Add((header, "NA"));
        }
    } 
    public static bool LineLineIntersection(out Vector3 intersection,
         Vector3 linePoint1, Vector3 lineDirection1,
         Vector3 linePoint2, Vector3 lineDirection2)
     {

         Vector3 lineVec3 = linePoint2 - linePoint1;
         Vector3 crossVec1and2 = Vector3.Cross(lineDirection1, lineDirection2);
         Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineDirection2);
         float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

         //is coplanar, and not parallel
         if (Mathf.Abs(planarFactor) < 0.05f //0.0001f
                 && crossVec1and2.sqrMagnitude > 0.0001f) //0.0001f
         {
             float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
             intersection = linePoint1 + (lineDirection1 * s);
             return true;
         }
         else
         {
             intersection = Vector3.zero;
             return false;
         }
     }
    
    public float GetAngleOnAxis(Vector3 dir1, Vector3 dir2, Vector3 axis)
    {
        Vector3 perpendicularDir1 = Vector3.Cross(axis, dir1);
        Vector3 perpendicularDir2 = Vector3.Cross(axis, dir2);
        return Vector3.SignedAngle(perpendicularDir1, perpendicularDir2, axis);
    }

    public Vector2 GetVectorAngle(Vector3 vec)
    {
        float xAngle = GetAngleOnAxis(Vector3.forward, vec, Vector3.up);
        float yAngle = GetAngleOnAxis(Vector3.forward, vec, Vector3.left);
        return new Vector2(xAngle, yAngle);
    }
}