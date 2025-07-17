//using System.Collections;
//using System.Collections.Generic;
//using System;
//using UnityEngine;
//using System.Linq;
//using System.Text;
//using System.IO;
//using UnityEngine.XR;
//using System.Diagnostics;


//public class Experimentscript : MonoBehaviour
//{
//    public ViveSR.anipal.Eye.EyeData eyeData = new ViveSR.anipal.Eye.EyeData();

//    // settings
//    public const int blocks = 10;
//    public const int trials = 20;
//    public const float fixationOffset = -0.05f; // -0.2f
//    public const float probeOffset = -0.1f; // -0.1f

//    //constants
//    public const float preparationTime = 0f;
//    public const float anticipationTime = 1f;
//    public const float presentationTime = 0.250f;
//    public const float retentionTime = 1f;
//    public const float cueTime = 0.250f;
//    public const float cueOffTime = 1.250f;
//    private Vector3 flowVel = new Vector3(0f,0f,0f);
    

//    // the data structures, literally.
//    //time series
//    public Vector3 gazeDirectionCombined;
//    public Vector3 centerEyePos;
//    Dictionary<string,int> tagCodes;
//    Dictionary<Vector3,int> positionCodes;
//    Dictionary<Color,int> colorCodes;
//    Dictionary<Vector3,int> posProbeCodes;
//    float time;
//    float presentationTimeStamp;
//    float gaze_angle_x, gaze_angle_y, head_angle_x, head_angle_y;

//    private List<Vector3> gaze_dir;
//    private List<Vector3> gaze_pos;
//    private List<Vector3> head_pos;
//    private List<Vector3> head_dir;

//    private List<Vector3> presentation_positions;
//    private List<Vector3> presentation_anglediffs;
//    List<float> eye_openness_Ls, eye_openness_Rs;
//    List<float> pupil_dia_Ls, pupil_dia_Rs;
//    List<Vector2> position_Ls, position_Rs;
//    List<Vector2> pupil_sensor_Ls, pupil_sensor_Rs;
//    List<Ray> ray_Ls, ray_Rs, ray_Cs;
//    List<ViveSR.anipal.Eye.FocusInfo> info_Ls, info_Rs, info_Cs;
//    List<Vector3> focusNormals;
//    List<float> focusDistances;
//    List<Vector3> focusPoints;
//    List<int> triggerCodes;
//    List<string> focusTags;
//    List<long> currentTimesMs;
//    List<int> currentBlocks;
//    List<int> currentTrials;
//    List<int> ticks;
//    List<float> gaze_angles_x, gaze_angles_y;
//    List<float> head_angles_x, head_angles_y;

//    List<Vector3> viewpointsFix,viewpointsGaze;
//    List<Vector3> viewpointsCube, viewpointsSphere, viewpointsStar; //viewpointsCube, viewpointsSphere, viewpointsCylinder,viewpointsTriangle,viewpointsDiamond,viewpointsStar;


//    List<Vector3> screenPosGazes;
//    List<Vector3> screenPosFixations;
//    List<Vector3> screenPosCubes;
//    List<Vector3> screenPosSpheres;
//    //List<Vector3> screenPosCylinders;
//    //List<Vector3> screenPosTriangles;
//    //List<Vector3> screenPosDiamonds;
//    List<Vector3> screenPosStars;

//     //HANDTRACKING
//    List<Vector3> hand_pos;

//    GameObject presCube, presSphere,presStar; //presCube, presSphere,presCylinder,presTriangle,presDiamond,presStar;

//    //other eye data
//    float eye_openness_L, eye_openness_R;
//    float pupil_dia_L, pupil_dia_R;
//    Vector2 position_L, position_R;
//    Vector2 pupil_sensor_L, pupil_sensor_R;
//    Ray ray_L, ray_R, ray_C;
//    ViveSR.anipal.Eye.FocusInfo info_L, info_R, info_C;
//    Vector3 focusNormal;
//    float focusDistance;
//    Vector3 focusPoint;
//    Vector3 cameraPos;
//    Vector3 cameraRot;
//    string focusTag;

//    // pseudo-random 6x50 conditions
//    public List<string> conditionSequence;

    
//    // external input
//    public GameObject txtobj;
//    public GameObject timer;
//    public Shader outline;

//    public bool practice = true;
//    public int practiceTrial = 0;
//    public bool testing = false;
//    public int participant_nr = 0;

//    // constants
//    public string[] conditions = new string[]{"s2", "d2"};
//    public string[] tagNames = new string[]{"triangle", "sphere","star"};//new string[]{"cube", "sphere", "cylinder", "triangle", "diamond", "star"};
//    string[] obj_properties = new string[]{"shape", "color", "loc", "probe"};

    
//    public const float answerTime = 5f;
//    private const int objectCount = 2;
//    private const int probeCount = 3;
//    private const float presenceChance = 100f;
//    private const float probeScaleFactor = 0.2f;
//    private int totalTrials = blocks * trials;
//    public int conditionCountBlock;
//    private string useController = "rightHand";
//    private Vector3 probeScale = new Vector3(0.1f, 0.1f, 0.1f);
//    private Vector3 probeScaleBig = new Vector3(0.12f, 0.12f, 0.12f);
//    Vector3 refPos = new Vector3(0.0f,1.25f,0.0f);
//    Vector3 refPosProbe = new Vector3(0.0f, 1.2f,0.0f);
//    Vector3 relHeight = new Vector3(0f,fixationOffset,0f);
//    Vector3 relHeightProbe = new Vector3(0f,probeOffset,0f);
    
//    // collection components
//    public GameObject cube;
//    public GameObject sphere;
//    //public GameObject cylinder;
//    //public GameObject triangle;
//    //public GameObject diamond;
//    public GameObject star;

//    public static Color red = Color.red;
//    public static Color green = Color.green;
//    public static Color blue = Color.blue;
//    public static Color gray = Color.gray;
//    public static Color magenta = Color.magenta;
//    public static Color cyan = Color.cyan;
//    public static Color yellow = Color.yellow;
//    public static Color black = Color.black;

//    // preset collections
//    public Color[] colors;
//    public GameObject[] shapes;
//    public Vector3[] positions;
//    public Vector3[] positions_2;
//    public Vector3[] positions_6;
//    public Vector3[] positions_flow;
//    public Vector3[] positions_flow_2;
//    public Vector3[] positions_flow_6;
//    public Vector3[] positions_probes;
//    public Vector3[] positions_probes_2;
//    public Vector3[] positions_probes_6;
//    public Vector3[] positions_all;
//    public Vector3[] positions_circle;
//    public Vector3[] positions_circle_flow;
//    public Vector3[] left_half;
//    public Vector3[] right_half;
//    public Vector3[] left_half_flow;
//    public Vector3[] right_half_flow;

//    public Dictionary<Color, string> colorNames = new Dictionary<Color, string>();

//    // temporary structures
//    private GameObject[] sample_shapes; // presentation objects
//    private Color[] sample_colors; //presentation colors
//    private GameObject[] sample_probes; //probe objects
//    private Color[] probes_colors; // probe colors
//    private int[,] presentationInfo; //this contains object info for presentation array: [index, color, shape]
//    private bool[,] ConfirmationArray;
//    private bool[,] ProbeConfirmationArray;


//    // flags and counters
//    //private bool paused = false;
//    public string stage = "none";
//    private int blockCount = 1;
//    private int trialCount = 1;
//    public GameObject selected;
//    public int timerCounter = 0;
//    private int ColorRandomNumber;
//    private int ShapeRandomNumber;
//    private bool isATarget;
//    public string currentCondition = "none"; // "none", "fixed", "flow"
//    bool conditionControl;
//    public int currentNumCondition = 0; // 0, 2, 4, 6
//    public Vector3[] currentPositions;
//    public Vector3[] currentProbePositions;
//    public GameObject currentSelectedObject;
//    float triggerValue;
//    public GameObject highlightedObject;
//    public GameObject targetObject;
//    private bool answered = false;
//    Color targetColor;
//    private bool correct = false;
//    public GameObject closest = null;
//    public GameObject testinstance;
//    public bool pressed = false;
//    public int pressCount = 0;
//    public int currentTrial = 0;
//    public int currentBlock = 0;
//    int triggerCode = 0;
//    string conditionName;
//    int addition_num = 0;
//    int currentTick;

//    // formatted trial data
//    int absoluteTrial = 0; // 1-500
//    //currentBlock
//    //currentTrial
//    int conditionCode = 0; // 1-6
//    //correct // 0|1
//    long responseTime; //ms
//    int targetShape = 0; // 1-6
//    int targetCol;
//    int targetLoc;
//    int targetLocProbe;
//    int responseShape;
//    int responseColor;
//    int responseLoc;
//    int responseLocProbe;

//    Vector3 screenPosGaze;
//    Vector3 screenPosFixation;
//    Vector3 screenPosCube;
//    Vector3 screenPosSphere;
//    //Vector3 screenPosCylinder;
//    //Vector3 screenPosTriangle;
//    //Vector3 screenPosDiamond;
//    Vector3 screenPosStar;
//    Vector3 hand_position;


//    // instances
//    public TextMesh tm;
//    private GameObject testObj;
//    private GameObject timeObj;
//    public GameObject targetInstance;
//    public GameObject responseInstance;
//    public GameObject targetProbe;
//    public GameObject responseProbe;

//    private int targetIndex;
//    List<int> takenPos;
//    int m;
    
//    List<int> shapeSeq;
//    List<int> colSeq;
//    List<int> circle_inds;
//    List<int> extra_list;
//    string shp_tag;
//    string trg_tag;

//    Vector3 camHeight;
//    Vector3 heightOffset;
    
//    Stopwatch watch;
//    long oldTime;
//    long newTime;
//    long currentTimeMs;
    

//    XRInputSubsystem subSys;

//    //OUTPUT FILE
//    //tab-separated
//    //

//    //Dictionary<string, List<string>> dataset = new Dictionary<string, List<string>>();
//    //private List<Vector3> head_positions

//    //controllers
//    List<UnityEngine.XR.InputDevice> controllerList = new List<UnityEngine.XR.InputDevice>();

//    // Start is called before the first frame update
//    void Start()
//    {
//        currentBlock = 1;
//        currentTick = 1;
//        currentTimeMs = 0L;
//        watch = Stopwatch.StartNew();
//        subSys = new XRInputSubsystem();
        
//        targetInstance = new GameObject();
//        responseInstance = new GameObject();
//        targetProbe = new GameObject();
//        responseProbe = new GameObject();

//        gaze_dir = new List<Vector3>();
//        gaze_pos = new List<Vector3>();
//        head_pos = new List<Vector3>();
//        head_dir = new List<Vector3>();
//        eye_openness_Ls = new List<float>();
//        eye_openness_Rs = new List<float>();
//        pupil_dia_Ls = new List<float>();
//        pupil_dia_Rs = new List<float>();
//        position_Ls = new List<Vector2>();
//        position_Rs = new List<Vector2>();
//        pupil_sensor_Ls = new List<Vector2>();
//        pupil_sensor_Rs = new List<Vector2>();
//        ray_Ls = new List<Ray>();
//        ray_Rs = new List<Ray>();
//        ray_Cs = new List<Ray>();
//        info_Ls = new List<ViveSR.anipal.Eye.FocusInfo>();
//        info_Rs = new List<ViveSR.anipal.Eye.FocusInfo>();
//        info_Cs = new List<ViveSR.anipal.Eye.FocusInfo>();
//        focusNormals = new List<Vector3>();
//        focusDistances = new List<float>();
//        focusPoints = new List<Vector3>();
//        triggerCodes = new List<int>();
//        focusTags = new List<string>();
//        currentTimesMs = new List<long>();
//        currentBlocks = new List<int>();
//        currentTrials = new List<int>();
//        ticks = new List<int>();
//        gaze_angles_x = new List<float>();
//        gaze_angles_y = new List<float>();
//        head_angles_x = new List<float>();
//        head_angles_y = new List<float>();
//        viewpointsFix = new List<Vector3>();
//        viewpointsGaze = new List<Vector3>();
//        viewpointsCube = new List<Vector3>();
//        viewpointsSphere = new List<Vector3>();
//        //viewpointsCylinder = new List<Vector3>();
//        //viewpointsTriangle = new List<Vector3>();
//        //viewpointsDiamond = new List<Vector3>();
//        viewpointsStar = new List<Vector3>();
//        hand_pos = new List<Vector3>();

//        screenPosCube = new Vector3();
//        screenPosSphere = new Vector3();
//        //screenPosCylinder = new Vector3();
//        //screenPosTriangle = new Vector3();
//        //screenPosDiamond = new Vector3();
//        screenPosStar = new Vector3();
//        hand_position = new Vector3();

        
//        //setup tagCodes and positionCodes
//        tagCodes = new Dictionary<string,int>();
//        positionCodes = new Dictionary<Vector3,int>();
//        colorCodes = new Dictionary<Color,int>();
//        posProbeCodes = new Dictionary<Vector3,int>();

//        conditionCountBlock = trials / conditions.Length;

//        //camHeight = new Vector3(0.0f, 1.2f, 0.0f);
//        heightOffset = new Vector3(0.0f,0.0f,0.0f);
        
//        // generate condition sequence
//        //int trials_per_cond = totalTrials / conditions.Length;
//        int counterr = 0;
//        for (int bl=0; bl < blocks; bl++)
//        {
//            List<string> condSeqBlock = new List<string>();
//            foreach (string cond in conditions)
//            {
//                for (int cc=0; cc<conditionCountBlock; cc++)
//                {
//                    condSeqBlock.Add(cond);
//                    counterr += 1;
//                }
//            }
//            condSeqBlock = condSeqBlock.OrderBy(x => UnityEngine.Random.value).ToList();
//            foreach (string csb in condSeqBlock)
//            {
//                conditionSequence.Add(csb);
//            }
//        }
        
 
        
//        // preset collections
//        colors = new Color[] {red, green, blue, magenta, cyan, yellow};
//        shapes = new GameObject[] {cube, sphere, star};//new GameObject[] {cube, sphere, cylinder, triangle, diamond, star};
//        positions = new Vector3[] { new Vector3(-1.5f,2f,0f), new Vector3(1.5f,2f,0f), new Vector3(-1.5f,0.5f,0f), new Vector3(1.5f,0.5f,0f) };
//        positions_2 = new Vector3[] { new Vector3(-1f, 1.25f, 0f), new Vector3(1f, 1.25f, 0f) };
//        positions_6 = new Vector3[] { new Vector3(-1f, 2f, 0f), new Vector3(1f, 2f, 0f), new Vector3(-1f, 0.5f, 0f), new Vector3(1f, 0.5f, 0f), new Vector3(-1.5f, 1.25f, 0f), new Vector3(1.5f, 1.25f, 0f) };
//        positions_all = new Vector3[] { new Vector3(-1.5f,2f,0f), new Vector3(1.5f,2f,0f), new Vector3(-1.5f,0.5f,0f), new Vector3(1.5f,0.5f,0f) };

//        positions_flow = new Vector3[] { new Vector3(-1.5f, 2f, 4f), new Vector3(1.5f, 2f, 4f), new Vector3(-1.5f, 0.5f, 4f), new Vector3(1.5f, 0.5f, 4f) };
//        positions_flow_2 = new Vector3[] { new Vector3(-1f, 1.25f, 4f), new Vector3(1f, 1.25f, 4f) };
//        positions_flow_6 = new Vector3[] { new Vector3(-1f, 2f, 4f), new Vector3(1f, 2f, 4f), new Vector3(-1f, 0.5f, 4f), new Vector3(1f, 0.5f, 4f), new Vector3(-1.5f, 1.25f, 4f), new Vector3(1.5f, 1.25f, 4f) };
//        //positions_probes = positions;
//        positions_probes = new Vector3[] { new Vector3(-0.15f, 1.2f, -3.5f), new Vector3(0f, 1.2f, -3.5f), new Vector3(0.15f, 1.2f, -3.5f) };//positions_probes_flow = new Vector3[] { new Vector3(-0.65f, 0.60f, -2.4f), new Vector3(-0.4f, 0.60f, -2f), new Vector3(0.4f, 0.60f, -2f), new Vector3(0.65f, 0.60f, -2.4f) };
//        positions_probes_2 = new Vector3[] { new Vector3(-0.1f, 1.2f, -3.5f), new Vector3(0.1f, 1.2f, -3.5f)};
//        positions_probes_6 = new Vector3[] { new Vector3(-0.25f, 1.2f, -3.5f), new Vector3(-0.15f, 1.2f, -3.5f), new Vector3(-0.05f, 1.2f, -3.5f), new Vector3(0.05f, 1.2f, -3.5f), new Vector3(0.15f, 1.2f, -3.5f), new Vector3(0.25f, 1.2f, -3.5f) };
//        //override to always 6 probes
//        //positions_probes = positions_probes_6;
//        positions_probes_2 = positions_probes_6;
//        // circular positions
//        positions_circle = new Vector3[] { new Vector3(-0.6f, 1.25f, 4f), new Vector3(0.6f, 1.25f, 4f)};
//        positions_circle_flow = new Vector3[] {new Vector3(-5f, 1.25f, 4f), new Vector3(5f, 1.25f, 4f)};
//        //positions_circle_flow = positions_circle;

//        //colorNames
//        colorNames.Add(red, "red");
//        colorNames.Add(green, "green");
//        colorNames.Add(blue, "blue");
//        colorNames.Add(magenta, "magenta");
//        colorNames.Add(cyan, "cyan");
//        colorNames.Add(yellow, "yellow");

//        // temporary structures
//        sample_shapes = new GameObject[objectCount]; // presentation objects
//        sample_colors = new Color[objectCount]; //presentation colors
//        sample_probes = new GameObject[probeCount]; //probe objects
//        probes_colors = new Color[probeCount]; // probe colors
//        presentationInfo = new int[objectCount, 2]; //this contains object info for presentation array: [index, color, shape]
//        ConfirmationArray = new bool[colors.Length, shapes.Length];
//        ProbeConfirmationArray = new bool[colors.Length, shapes.Length];

//        int somecounter1 = 0;
//        foreach (Vector3 fixedPos in positions_circle)
//        {
//            somecounter1 += 1;
//            positionCodes.Add(fixedPos, somecounter1);
//        }

//        foreach (Vector3 fixedPos in positions_circle_flow)
//        {
//            somecounter1 += 1;
//            positionCodes.Add(fixedPos, somecounter1+2);
//        }

//        somecounter1 = 0;
//        foreach (string fixedName in tagNames)
//        {
//            somecounter1 += 1;
//            tagCodes.Add(fixedName, somecounter1);
//        }
//        somecounter1 = 0;
//        foreach (Color fixedColor in colors)
//        {
//            somecounter1 += 1;
//            colorCodes.Add(fixedColor, somecounter1);
//        }
//        somecounter1 = 0;
//        foreach (Vector3 fixedProbe in positions_probes)
//        {
//            somecounter1 += 1;
//            posProbeCodes.Add(fixedProbe, somecounter1);
//        }


//        timeObj = Instantiate(timer, new Vector3(0,0,0), Quaternion.identity);

//        tm = Instantiate(txtobj, new Vector3(0f,1.25f,5f), Quaternion.identity).GetComponent<TextMesh>();
//        //tm.fontSize = 35;
        
//        if (testing)
//        {
//            tm.text = "Testing mode";
            
//        } else {
//            tm.text = "Press index finger trigger to start calibration.";
//        }
//        //tm.text = conditionSequence.Count.ToString();
//        //tm.text = conditionSequence[5].ToString();

//        //timeObj.GetComponent<Timer>().setTime(5f);
//        //timeObj.GetComponent<Timer>().setRunning(true);

//        // test
//        /*
//        ColorRandomNumber = UnityEngine.Random.Range(0,colors.Length);
//        ShapeRandomNumber = UnityEngine.Random.Range(0,shapes.Length);

//        sample_colors[0] = colors[ColorRandomNumber];
//        sample_shapes[0] = Instantiate(shapes[ShapeRandomNumber], new Vector3(1f,1f,1f), Quaternion.identity); // positions[0]
        
//        Color col = sample_colors[0];
//        string sp = sample_shapes[0].tag;

//        tm.color = col;
//        tm.text = sp;

//        Shapescript shapescr = sample_shapes[0].GetComponent<Shapescript>();
//        shapescr.setColour(col);
//        */

//        //test

//        var leftHandedControllers = new List<UnityEngine.XR.InputDevice>();
//        var desiredCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller;
//        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, leftHandedControllers);
//        //Debug.Log("HELLO");
//        foreach (var device in leftHandedControllers)
//        {
//            //Debug.Log(string.Format("Device name '{0}' has characteristics '{1}'", device.name, device.characteristics.ToString()));
//            controllerList.Add(device);
//        }

//        var rightHandedControllers = new List<UnityEngine.XR.InputDevice>();
//        var desiredCharacteristicsR = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller;
//        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredCharacteristicsR, rightHandedControllers);
//        //Debug.Log("HELLO");
//        foreach (var device in rightHandedControllers)
//        {
//            //Debug.Log(string.Format("Device name '{0}' has characteristics '{1}'", device.name, device.characteristics.ToString()));
//            controllerList.Add(device);
//        }

//    }

//    // Update is called once per frame
//    void Update()
//    {   
//        currentTimeMs = watch.ElapsedMilliseconds;
        
//        //angles test
//        //Quaternion changeInRotation = Quaternion.FromToRotation(Vector3.forward, gazeDirectionCombined);
//        //Vector3 euler = changeInRotation.eulerAngles;
//        //float ax = Vector3.SignedAngle(Vector3.forward, gazeDirectionCombined, Vector3.up);

//        //tm.text = "X-angle: " + ax + "Y-angle: ";


//        //time = (float)Time.realtimeSinceStartup;
//        //register trigger press
//        foreach (var ctrl in controllerList) {
//            if (ctrl.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger,
//                                    out triggerValue)
//                                    && triggerValue > 0.1f) {
//                if (!pressed) {
//                    primaryButtonDown();
//                    pressed = true;
//                }
                    
//            } else {
//                if (pressed) {
//                    pressed = false;
                    
//                }
//            }
            
//        }

//        // eye data read
//        // ViveSR.anipal.Eye.SRanipal_Eye_API.GetEyeData(ref eyeData);
//        //Quaternion q = Quaternion.FromToRotation(Vector3.up, Camera.main.transform.forward);
//        //Vector3 headEuler = Camera.main.transform.eulerAngles;
        
//        //tm.text = GetAngleOnAxis(Vector3.forward, Camera.main.transform.forward, Vector3.up).ToString("F1") + " " + GetAngleOnAxis(Vector3.forward, Camera.main.transform.forward, Vector3.left).ToString("F1");

//        //move presentation objects
//        /*
//        if (stage.Equals("presentation"))
//        {
//            if (currentCondition.Equals("flow"))
//            {
//                foreach (GameObject toMove in sample_shapes)
//                {
//                    toMove.GetComponent<Transform>().position += flowVel * Time.deltaTime;
//                }
//            }
//        }
//        */

//        if (stage.Equals("answer"))
//        {

//            if (answered)
//            {
//                //tm.text = "answered";
//                if (correct)
//                {   
//                    if (currentSelectedObject != null)
//                    {
//                        //currentSelectedObject.GetComponent<Transform>().position = GameObject.FindGameObjectsWithTag(useController)[0].GetComponent<Transform>().position;
//                        //currentSelectedObject.GetComponent<Transform>().rotation = GameObject.FindGameObjectsWithTag(useController)[0].GetComponent<Transform>().rotation;
//                    }
//                }
//            } else {
//                // selection angle
//                // 1. get hand position
//                Vector3 handPos = GameObject.FindGameObjectsWithTag(useController)[0].GetComponent<Transform>().position;
                
//                // angle solution
//                /*
//                Vector3 headPos = GameObject.FindGameObjectsWithTag("MainCamera")[0].GetComponent<Transform>().position;
//                Vector3 shoulderPos = new Vector3();
//                if (useController == "rightHand")
//                {
//                    shoulderPos = headPos + new Vector3(0.15f, -0.15f, 0.2f);
//                } else if (useController == "leftHand")
//                {
//                    shoulderPos = headPos + new Vector3(-0.15f, -0.15f, 0.2f);
//                }
//                Vector3 targetDir = handPos - shoulderPos;
//                Vector3 forward = Vector3.forward;
//                float ang = Vector3.SignedAngle(targetDir, forward, Vector3.up);
//                tm.text = ang.ToString();

//                float difference = 180;
//                for(int ooo=0; ooo < currentNumCondition; ooo++)
//                {
//                    targetDir = sample_probes[ooo].transform.position - shoulderPos;
//                    float tempAng = Vector3.SignedAngle(targetDir, forward, Vector3.up);
//                    if (Mathf.Abs(tempAng-ang) < difference) {
//                        difference = Mathf.Abs(tempAng-ang);
//                        closest = sample_probes[ooo];
//                    }
//                }
//                if (highlightedObject != closest) {
//                    objectHoverEnter(closest);
//                }
//                */

//                // distance solution
//                Vector3 targetLoc;
//                float difference = 10;
//                for(int ooo=0; ooo < currentNumCondition; ooo++)
//                {
//                    targetLoc = sample_probes[ooo].transform.position;
//                    if ((targetLoc-handPos).magnitude < difference) {
//                        difference = (targetLoc-handPos).magnitude;
//                        closest = sample_probes[ooo];
//                    }
//                }
//                if (highlightedObject != closest) {
//                    //objectHoverEnter(closest);
//                }
                
//            }
//        }

//        // tracking data update
//        if (stage.Equals("anticipation") ||
//            stage.Equals("presentation") ||
//            stage.Equals("retention") ||
//            stage.Equals("cue") ||
//            stage.Equals("cue-off") ||
//            stage.Equals("answer"))
//        {
//            screenPosFixation.x = -1;
//            screenPosFixation.y = -1;
//            screenPosGaze.x = -1;
//            screenPosGaze.y = -1;
//            screenPosCube.x = -1;
//            screenPosCube.y = -1;
//            screenPosSphere.x = -1;
//            screenPosSphere.y = -1;
//            //screenPosCylinder.x = -1;
//            //screenPosCylinder.y = -1;
//            //screenPosTriangle.x = -1;
//            //screenPosTriangle.y = -1;
//            //screenPosDiamond.x = -1;
//            //screenPosDiamond.y = -1;
//            screenPosStar.x = -1;
//            screenPosStar.y = -1;


//            ViveSR.anipal.Eye.SRanipal_Eye.GetEyeOpenness(ViveSR.anipal.Eye.EyeIndex.LEFT, out eye_openness_L);
//            ViveSR.anipal.Eye.SRanipal_Eye.GetEyeOpenness(ViveSR.anipal.Eye.EyeIndex.RIGHT, out eye_openness_R);
//            ViveSR.anipal.Eye.SRanipal_Eye.GetPupilPosition(ViveSR.anipal.Eye.EyeIndex.LEFT, out position_L);
//            ViveSR.anipal.Eye.SRanipal_Eye.GetPupilPosition(ViveSR.anipal.Eye.EyeIndex.RIGHT, out position_R);
//            ViveSR.anipal.Eye.SRanipal_Eye.Focus(ViveSR.anipal.Eye.GazeIndex.LEFT, out ray_L, out info_L);
//            ViveSR.anipal.Eye.SRanipal_Eye.Focus(ViveSR.anipal.Eye.GazeIndex.RIGHT, out ray_R, out info_R);
//            ViveSR.anipal.Eye.SRanipal_Eye.Focus(ViveSR.anipal.Eye.GazeIndex.COMBINE, out ray_C, out info_C);
//            pupil_dia_L = eyeData.verbose_data.left.pupil_diameter_mm;
//            pupil_dia_R = eyeData.verbose_data.right.pupil_diameter_mm;
//            pupil_sensor_L = eyeData.verbose_data.left.pupil_position_in_sensor_area;
//            pupil_sensor_R = eyeData.verbose_data.right.pupil_position_in_sensor_area;
//            cameraPos = Camera.main.transform.position;
//            cameraRot = Camera.main.transform.forward;
//            focusNormal = info_C.normal;
//            focusDistance = info_C.distance;
//            focusPoint = info_C.point;
//            eye_openness_L = eyeData.verbose_data.left.eye_openness;
//            eye_openness_R = eyeData.verbose_data.right.eye_openness;
//            gaze_angle_x = GetVectorAngle(gazeDirectionCombined).x;
//            gaze_angle_y = GetVectorAngle(gazeDirectionCombined).y;
//            head_angle_x = GetVectorAngle(Camera.main.transform.forward).x; //Vector3.SignedAngle(Vector3.forward, Camera.main.transform.forward, Vector3.up);
//            head_angle_y = GetVectorAngle(Camera.main.transform.forward).y; //Vector3.SignedAngle(Vector3.forward, Camera.main.transform.forward, Vector3.right);

//            //screen position
//            //screenPosCube = Camera.WorldToViewportPoint();

//            focusTag = "untagged";
//            if (info_C.transform) {
//                if (info_C.transform.tag != null) {
//                    if (info_C.transform.tag == "Untagged") {
//                        focusTag = "none";
//                    } else {
//                        focusTag = info_C.transform.tag;
//                    }
//                }
//            }

//            //gaze
//            //tm.text = "gaze";
//            screenPosGaze = Camera.main.WorldToViewportPoint(focusPoint);

//            //fixation cross
//            //tm.text = "fixationcross";
//            screenPosFixation = Camera.main.WorldToViewportPoint(tm.GetComponent<Transform>().position);

//            //objects
//            if (stage.Equals("presentation")) {
//                if (presCube != null) {
//                    //tm.text = "cube";
//                     screenPosCube = Camera.main.WorldToViewportPoint(presCube.GetComponent<Transform>().position);
//                }
//                if (presSphere != null) {
//                //tm.text = "sphere";
//                    screenPosSphere = Camera.main.WorldToViewportPoint(presSphere.GetComponent<Transform>().position);
//                }
//                /*
//                if (presCylinder != null) {
//                    //tm.text = "cylinder";
//                    screenPosCylinder = Camera.main.WorldToViewportPoint(presCylinder.GetComponent<Transform>().position);
//                }
//                if (presTriangle != null) {
//                    //tm.text = "triangle";
//                    screenPosTriangle = Camera.main.WorldToViewportPoint(presTriangle.GetComponent<Transform>().position);
//                }
//                if (presDiamond != null) {
//                    //tm.text = "diamond";
//                    screenPosDiamond = Camera.main.WorldToViewportPoint(presDiamond.GetComponent<Transform>().position);
//                }
//                */
//                if (presStar != null) {
//                    //tm.text = "star";
//                    screenPosStar = Camera.main.WorldToViewportPoint(presStar.GetComponent<Transform>().position);
//                }
//            }

//            //HANDTRACKING
//            hand_position = GameObject.FindGameObjectsWithTag(useController)[0].GetComponent<Transform>().position;


//            //variables to temp structures
//            gaze_dir.Add(gazeDirectionCombined);
//            head_pos.Add(cameraPos);
//            head_dir.Add(cameraRot);
//            eye_openness_Ls.Add(eye_openness_L);
//            eye_openness_Rs.Add(eye_openness_R);
//            pupil_dia_Ls.Add(pupil_dia_L);
//            pupil_dia_Rs.Add(pupil_dia_R);
//            pupil_sensor_Ls.Add(pupil_sensor_L);
//            pupil_sensor_Rs.Add(pupil_sensor_R);
//            position_Ls.Add(position_L);
//            position_Rs.Add(position_R);
//            ray_Ls.Add(ray_L);
//            ray_Rs.Add(ray_R);
//            ray_Cs.Add(ray_C);
//            gaze_pos.Add(ray_C.origin);
//            info_Ls.Add(info_L);
//            info_Rs.Add(info_R);
//            info_Cs.Add(info_C);
//            focusNormals.Add(focusNormal);
//            focusDistances.Add(focusDistance);
//            focusPoints.Add(focusPoint);
//            triggerCodes.Add(triggerCode);
//            focusTags.Add(focusTag);
//            currentTimesMs.Add(currentTimeMs);
//            currentBlocks.Add(currentBlock);
//            currentTrials.Add(currentTrial);
//            ticks.Add(currentTick);
//            gaze_angles_x.Add(gaze_angle_x);
//            gaze_angles_y.Add(gaze_angle_y);
//            head_angles_x.Add(head_angle_x);
//            head_angles_y.Add(head_angle_y);

//            viewpointsGaze.Add(screenPosGaze);
//            viewpointsFix.Add(screenPosFixation);
//            viewpointsCube.Add(screenPosCube);
//            viewpointsSphere.Add(screenPosSphere);
//            //viewpointsCylinder.Add(screenPosCylinder);
//            //viewpointsTriangle.Add(screenPosTriangle);
//            //viewpointsDiamond.Add(screenPosDiamond);
//            viewpointsStar.Add(screenPosStar);
//            hand_pos.Add(hand_position);

//        }

//        triggerCode = 0;
//        currentTick += 1;

//        //tm.text = hand_position.ToString();

//    }

//    public void timerFinished()
//    {
//        switch(stage)
//        {
            
//            case "preparation":

//                responseLoc = 0;
//                responseLocProbe = 0;
//                targetLocProbe = 0;
//                targetLoc = 0;
//                targetCol = 0;
//                responseShape = 0;

//                //tm.fontSize = 35;
//                tm.text = "+";


//                if (practice) {
//                    practiceTrial += 1;
//                } else {
//                    if (currentTrial < trials)
//                    {
//                        currentTrial += 1;
//                    } else {
//                        currentTrial = 1;
//                        //currentBlock += 1;
//                    }
                    
//                }

//                conditionName = getTrialCondition();
//                //conditionName = "d6";
//                switch(conditionName) {
//                    case "s2":
//                        conditionCode = 1;
//                        currentNumCondition = 2;
//                        break;
//                    case "d2":
//                        conditionCode = 2;
//                        currentNumCondition = 2;
//                        break;
//                    default:
//                        conditionCode = 0;
//                        break;
//                }

//                //tm.text = conditionName;
//                //decide condition
//                //conditionControl = (UnityEngine.Random.Range(0f, 100f) <= 50);

//                if (conditionName.Contains("s"))
//                {
//                    currentCondition = "fixed";
//                    currentPositions = positions_circle;
//                } else
//                {
//                    currentCondition = "flow";
//                    currentPositions = positions_circle_flow;
//                }
//                currentProbePositions = positions_probes;

//                //decide object number factor
//                //float objNumOdds = UnityEngine.Random.Range(0f, 100f);
               

//                sample_shapes = new GameObject[currentNumCondition]; // presentation objects
//                sample_colors = new Color[currentNumCondition]; //presentation colors
//                sample_probes = new GameObject[currentProbePositions.Length]; //probe objects
//                probes_colors = new Color[currentProbePositions.Length]; // probe colors

//                tm.color = black;
//                stage = "anticipation";
//                addition_num = 0;
//                triggerCode = addition_num + conditionCode;
//                timeObj.GetComponent<Timer>().setTime(anticipationTime);
//                timeObj.GetComponent<Timer>().setRunning(true);
//                break;

//            case "anticipation":
//                stage = "presentation";

//                //reset view infos
//                presCube = null;
//                presSphere = null;
//                //presCylinder = null;
//                //presTriangle = null;
//                //presDiamond = null;
//                presStar = null;


//                addition_num = 10;
//                triggerCode = addition_num + conditionCode;
//                timeObj.GetComponent<Timer>().setTime(presentationTime);
//                timeObj.GetComponent<Timer>().setRunning(true);

//                //generate and present 2 random shapes
//                Array.Clear(ConfirmationArray, 0, ConfirmationArray.Length);
//                Array.Clear(presentationInfo, 0, presentationInfo.Length);

//                //pre-determine random shape and color of presentation
//                shapeSeq = new List<int>();
//                colSeq = new List<int>();
//                int newInd;
//                for (int p = 0; p < currentNumCondition; p++)
//                {
//                    do
//                    {
//                        newInd = UnityEngine.Random.Range((int)0, shapes.Length);
//                    } while (shapeSeq.Contains(newInd));
//                    shapeSeq.Add(newInd);

//                    do
//                    {
//                        newInd = UnityEngine.Random.Range((int)0, colors.Length);
//                    } while (colSeq.Contains(newInd));
//                    colSeq.Add(newInd);
//                }
                
//                //tm.text = shapeSeq[0].ToString();

//                //determine positions from circle
//                circle_inds = new List<int>();
//                int newInd3;
//                //left half
//                for (int p = 0; p < currentNumCondition/2; p++)
//                {
//                    do
//                    {
//                        newInd3 = UnityEngine.Random.Range((int)0, positions_circle.Length/2);
//                    } while (circle_inds.Contains(newInd3));
//                    circle_inds.Add(newInd3);
//                }
//                //right half
//                List<int> temporary = new List<int>(circle_inds);
//                for (int q=0; q< temporary.Count;q++)
//                {
//                    circle_inds.Add(temporary[q]+1);
//                }

//                for (int i = 0; i < currentNumCondition; i++)
//                {

//                        //tm.color = green;

//                        /*
//                        bool presentationDupl = true;
//                        while (presentationDupl)
//                        {
//                            //string clr = colorNames[colors[ColorRandomNumber]];
//                            //string shp = shapes[ShapeRandomNumber].tag;
//                            //tm.text = i.ToString() + " " + clr + " " + shp;
//                            ColorRandomNumber = UnityEngine.Random.Range((int)0, colors.Length);
//                            ShapeRandomNumber = UnityEngine.Random.Range((int)0, shapes.Length);
//                            if (ConfirmationArray[ColorRandomNumber, ShapeRandomNumber] == false)
//                            {
//                                presentationDupl = false;
//                            } // this while will make a randim number until it is a "new" number
//                        }
//                        */
//                    ColorRandomNumber = colSeq[i];
//                    ShapeRandomNumber = shapeSeq[i];
                    
//                    //tm.color = cyan;
                    
//                    sample_colors[i] = colors[ColorRandomNumber];

//                    sample_shapes[i] = Instantiate(shapes[ShapeRandomNumber], currentPositions[circle_inds[i]] - refPos + heightOffset, Quaternion.identity); //shapes[ShapeRandomNumber].transform.rotation

//                    Shapescript shapescr = sample_shapes[i].GetComponent<Shapescript>();
//                    shapescr.setColour(sample_colors[i]);
//                    shapescr.spawnPosition = currentPositions[circle_inds[i]] + heightOffset + relHeight;
//                    shapescr.spawnPositionCode = circle_inds[i] + 1;
//                    shapescr.shapeCode = ShapeRandomNumber + 1;
//                    shapescr.colorCode = ColorRandomNumber + 1;

//                    //tm.text = (currentPositions[circle_inds[i]] + heightOffset + relHeight).ToString();

//                    //tm.text = "switch";
//                    switch(sample_shapes[i].tag) {
//                        case "cube":
//                            //tm.text = "case cube";
//                            presCube = sample_shapes[i];
//                            break;
//                        case "sphere":
//                            //tm.text = "case sphere";
//                            presSphere = sample_shapes[i];
//                            break;
//                        /*
//                        case "cylinder":
//                            //tm.text = "case cylinder";
//                            presCylinder = sample_shapes[i];
//                            break;
//                        case "triangle":
//                            //tm.text = "case triangle";
//                            presTriangle = sample_shapes[i];
//                            break;
//                        case "diamond":
//                            //tm.text = "case diamond";
//                            presDiamond = sample_shapes[i];
//                            break;
//                        */
//                        case "star":
//                            //tm.text = "case star";
//                            presStar = sample_shapes[i];
//                            break;
//                        default:
//                            break;
//                    }


//                    //scale
//                    //sample_shapes[i].transform.localScale /= 2;

//                    ConfirmationArray[ColorRandomNumber, ShapeRandomNumber] = true; // So it will not be picked again
//                    presentationInfo[i, 0] = ColorRandomNumber;
//                    presentationInfo[i, 1] = ShapeRandomNumber;

//                    //selection appearance change
//                    //var trans = 0.5f;
//                    //var col = sample_shapes[i].GetComponent<Renderer>().material.color;
//                    //col.a = trans;


//                }
//                //presentationTimeStamp = oldTime
//                break;

//            case "presentation":
//                stage = "retention";
//                addition_num = 20;
//                triggerCode = addition_num + conditionCode;
//                timeObj.GetComponent<Timer>().setTime(retentionTime);
//                timeObj.GetComponent<Timer>().setRunning(true);

//                //hide the 4 random shapes
//                foreach (GameObject oldObject in sample_shapes)
//                {
//                    //Destroy(oldObject);
//                    //oldObject.GetComponent<Renderer>().enabled = false;
//                    Shapescript shapescr2 = oldObject.GetComponent<Shapescript>();
//                    shapescr2.setVisible(false);
//                }

//                break;

//            case "retention":
//                stage = "cue";
//                addition_num = 30;
//                triggerCode = addition_num + conditionCode;
//                timeObj.GetComponent<Timer>().setTime(cueTime);
//                timeObj.GetComponent<Timer>().setRunning(true);

//                //decide target
//                targetIndex = UnityEngine.Random.Range((int)0, currentNumCondition);
//                targetColor = colors[presentationInfo[targetIndex, 0]];
//                tm.color = targetColor;

//                targetObject = shapes[presentationInfo[targetIndex, 1]];
//                //tm.text = targetObject.tag;
//                break;

//            case "cue":
//                stage = "cue-off";
//                addition_num = 40;
//                triggerCode = addition_num + conditionCode;
//                tm.color = black;
//                timeObj.GetComponent<Timer>().setTime(cueOffTime);
//                timeObj.GetComponent<Timer>().setRunning(true);
//                break;

//            case "cue-off":
//                oldTime = watch.ElapsedMilliseconds;
//                stage = "answer";
//                addition_num = 50;
//                triggerCode = addition_num + conditionCode;
//                selected = null;
//                highlightedObject = null;
//                currentSelectedObject = null;
//                answered = false;
//                closest = null;

//                //tm.text = "Which object was present?";
//                //timeObj.GetComponent<Timer>().setTime(answerTime);
//                //timeObj.GetComponent<Timer>().setRunning(true);
//                // determine and show probe shapes
//                bool targetPresent = (UnityEngine.Random.Range(0f, 100f) <= presenceChance);
//                if (presenceChance == 0f)
//                {
//                    targetPresent = false;
//                }
//                if (presenceChance == 100f)
//                {
//                    targetPresent = true;
//                }
//                int destinationIndex = UnityEngine.Random.Range((int)0, currentProbePositions.Length);
//                //tm.text = destinationIndex.ToString();

//                //in-place probes / quick probe calc
//                /*
//                //swap potisions
//                takenPos = new List<int>();
//                int newInd;
//                for (int k = 0; k < 4; k++)
//                {
//                    do
//                    {
//                        newInd = UnityEngine.Random.Range((int)0, 4);
//                    } while (takenPos.Contains(newInd));
//                    takenPos.Add(newInd);
//                }

//                m = 0;
//                //show the 4 random shapes again, colorless
//                foreach (GameObject oldObject in sample_shapes)
//                {
//                    oldObject.GetComponent<Transform>().position = positions[takenPos[m]];
//                    oldObject.GetComponent<Renderer>().enabled = true;
//                    // color
//                    Shapescript shapescr2 = oldObject.GetComponent<Shapescript>();
//                    shapescr2.setColour(gray);
//                    m += 1;
//                }

//                Array.Clear(ProbeConfirmationArray, 0, ProbeConfirmationArray.Length);
//                */

//                takenPos = new List<int>();
//                int newInd2;
//                for (int k = 0; k < currentProbePositions.Length; k++)
//                {
//                    do
//                    {
//                        newInd2 = UnityEngine.Random.Range((int)0, currentProbePositions.Length);
//                    } while (takenPos.Contains(newInd2));
//                    takenPos.Add(newInd2);
//                }

//                for (int i = 0; i < currentProbePositions.Length; i++)
//                {
//                    //if (targetPresent && i == destinationIndex)
//                    //    continue;

//                    /*
//                    bool dupl = true;
//                    while (dupl)
//                    {
//                        ColorRandomNumber = UnityEngine.Random.Range((int)0, colors.Length);
//                        ShapeRandomNumber = UnityEngine.Random.Range((int)0, shapes.Length);
//                        if (ConfirmationArray[ColorRandomNumber, ShapeRandomNumber] == false && ProbeConfirmationArray[ColorRandomNumber, ShapeRandomNumber] == false)
//                        {
//                            dupl = false;
//                        } // this while will make a randim number until it is a "new" number
//                    }
//                    */
//                    ShapeRandomNumber = takenPos[i];
//                    //tm.text = "ShapeRandomNumber set";
//                    ColorRandomNumber = 0;


//                    probes_colors[i] = colors[ColorRandomNumber];
//                    //tm.text = "probes_colors[i] set";
//                    sample_probes[i] = Instantiate(shapes[ShapeRandomNumber], currentProbePositions[i] - refPosProbe + camHeight + relHeightProbe, Quaternion.identity);
//                    //tm.text = "sample_probess[i] set";
//                    Shapescript probescr = sample_probes[i].GetComponent<Shapescript>();
//                    //tm.text = "probescr set";
//                    probescr.setColour(black); //probescr.setColour(gray); //probes_colors[i]
//                    //probescr.initProbe();
//                    probescr.probePositionCode = i+1;
//                    probescr.shapeCode = ShapeRandomNumber+1;
//                    probescr.spawnPosition = currentProbePositions[i] + relHeightProbe;
//                    //tm.text = "probescr.spawnPosition set";

//                    probescr.experiment = gameObject;

//                    //probescr.scaleUp();
//                    //probescr.scaleDown();
//                    //probescr.setScale(probeScale.x, probeScale.y, probeScale.z);//sample_probes[i].transform.localScale /= 4f;
//                    //sample_probes[i].transform.localScale *= probescr.;

//                    ProbeConfirmationArray[ColorRandomNumber, ShapeRandomNumber] = true; // So it will not be picked again
//                }


//                // the target
//                /*
//                if (targetPresent)
//                {
//                    int targetColorIndex = presentationInfo[targetIndex, 0];
//                    int targetShapeIndex = presentationInfo[targetIndex, 1];
//                    sample_probes[destinationIndex] = Instantiate(sample_shapes[takenPos[targetShapeIndex]], positions_probes[destinationIndex], Quaternion.identity);
//                    Shapescript targetscr = sample_probes[destinationIndex].GetComponent<Shapescript>();
//                    targetscr.setColour(gray); //colors[targetColorIndex]
//                    probes_colors[destinationIndex] = colors[targetColorIndex];
//                    sample_probes[destinationIndex].transform.localScale /= 2;
//                }
//                */

                

//                // TO-DO show visible time bar

//                break;

//            case "answer":
//                absoluteTrial = getAbsoluteTrial(); // 1-500
//                //tm.fontSize = 35;
//                //tm.text = "WriteTrialData";
//                WriteTrialData();

//                if (practice) {
//                    if (practiceTrial.Equals(2))
//                    {
//                        tm.text = "Saving...";
//                        WriteTimeData();
//                        ClearTempData();
//                        practice = false;
//                        stage = "none";
//                        tm.text = "Practice finished. Press to continue to calibration.";
//                        //timeObj.GetComponent<Timer>().setTime(preparationTime);
//                        //timeObj.GetComponent<Timer>().setRunning(true);
//                    } else {
//                        // next trial
//                        stage = "preparation";
//                        timeObj.GetComponent<Timer>().setTime(preparationTime);
//                        timeObj.GetComponent<Timer>().setRunning(true);
//                    }
//                } else {
//                    if (currentTrial < trials)
//                    {
//                        // next trial
//                        stage = "preparation";
//                        timeObj.GetComponent<Timer>().setTime(preparationTime);
//                        timeObj.GetComponent<Timer>().setRunning(true);
//                    } else {
//                        // save block data
//                        tm.text = "Saving...";
//                        WriteTimeData();
//                        ClearTempData();
                        

//                        //currentTrial = 0;
//                        if (currentBlock >= blocks)
//                        {
//                            // run finished
//                            tm.text = "Run Complete!";
//                        } else {
//                            // break
//                            currentBlock += 1;
//                            stage = "none";
//                            //tm.text = "break";
//                            tm.text = "Time for a break? Or press trigger to calibrate.";
//                            timeObj.GetComponent<Timer>().setRunning(false);
//                        }

//                    }
//                    //tm.color = red;
//                }

//                //clean up trial
//                answered = false;
//                highlightedObject = null;
//                currentSelectedObject = null;
                
//                //tm.text = "+";
//                tm.color = black;
//                //tm.text = "+";
//                //trialCount += 1;
//                //blockCount = trialCount/trials;

//                //destroy the 4 random shapes
//                foreach (GameObject oldObject in sample_shapes)
//                {
//                    Destroy(oldObject);
//                    //oldObject.GetComponent<Renderer>().enabled = false;
//                }
//                //destroy probes
//                foreach (GameObject oldProbe in sample_probes)
//                {
//                    Destroy(oldProbe);
//                }

//                Array.Clear(sample_shapes, 0, sample_shapes.Length);
//                Array.Clear(sample_colors, 0, sample_colors.Length);
//                Array.Clear(sample_probes, 0, sample_probes.Length);
//                Array.Clear(probes_colors, 0, probes_colors.Length);
//                Array.Clear(ConfirmationArray, 0, ConfirmationArray.Length);
//                Array.Clear(ProbeConfirmationArray, 0, ProbeConfirmationArray.Length);
//                Array.Clear(presentationInfo, 0, presentationInfo.Length);

//                break;
//            case "none":
//                // just in case (no pun intended)
//                stage = "preparation";
//                timeObj.GetComponent<Timer>().setTime(preparationTime);
//                timeObj.GetComponent<Timer>().setRunning(true);
//               // tm.fontSize = 35;
//                tm.text = "+";
//                //tm.color = magenta;
//                break;
//            default:
//                // probably never fires
//                break;
//        }

//        //tm.text = stage;
//    }


//    public void primaryButtonDown()
//    {   
//        //pressCount += 1;
//        //tm.text = pressCount.ToString();
//        switch (stage)
//        {
//            case "none":
//                //tm.text = "pressed";
//                if (testing)
//                {
//                    if (testinstance == null)
//                    {

//                        //tm.text = "spawn";
//                        Vector3 spawn = new Vector3(0f,1f,0f);
//                        testinstance = Instantiate(star, spawn, Quaternion.identity);
//                        //Shapescript sc = testinstance.GetComponent<Shapescript>();
//                        //sc.initProbe();
//                        //testinstance.GetComponent<Shapescript>().scaleUp();
//                    } else {
                        
//                        //scale up - WORKS!!
//                        //testinstance.GetComponent<Shapescript>().scaleUp();
                        
//                        //break it
                        
//                        if (testinstance.GetComponent<Shapescript>().exploded)
//                        {
//                            //tm.text = "found";
//                            Destroy(testinstance);
//                            Vector3 spawn = new Vector3(0f,1f,0f);
//                            testinstance = Instantiate(star, spawn, Quaternion.identity);
//                        } else
//                        {
//                            //testinstance.GetComponent<Shapescript>().explode();
//                            testinstance.AddComponent<TriangleExplosion>();
//                            StartCoroutine(testinstance.GetComponent<TriangleExplosion>().SplitMesh(false));
//                        }
//                         //break it
//                    }
//                } else {
//                    /*
//                    stage = "preparation";
//                    //tm.text = "???";
//                    timeObj.GetComponent<Timer>().setTime(preparationTime);
//                    timeObj.GetComponent<Timer>().setRunning(true);
//                    */
//                    stage = "calibration";

//                    //height
//                    ResetHeight();

//                    //calibrate position
//                    //subSys.TryRecenter();

//                    //calibrate eye
//                    //ViveSR.anipal.Eye.SRanipal_Eye.LaunchEyeCalibration();
//                    string block_type;
//                    if (practice) {
//                        block_type = "practice trials";
//                    } else {
//                        block_type = "block " + currentBlock.ToString() + "/" + blocks.ToString();
//                    }
//                    tm.text = "Press Index Trigger to start " + block_type + ".";
                    
//                }
//                break;

//            case "calibration":
//                    stage = "preparation";
//                    addition_num = 0;
//                    //tm.text = "???";
//                    timeObj.GetComponent<Timer>().setTime(preparationTime);
//                    timeObj.GetComponent<Timer>().setRunning(true);
//                break;

//            case "answer":
                
//                if (highlightedObject != null && answered == false)
//                    {   
//                        //tm.text = "answered";
//                        newTime = watch.ElapsedMilliseconds;
//                        responseTime = newTime - oldTime;
//                        addition_num = 60;
//                        triggerCode = addition_num + conditionCode;
//                        //tm.text = "triggerCode set";
//                        //currentBlock
//                        //currentTrial
//                        //conditionCode; // 1-6
//                        //responseTime
//                        //correct // 0|1
//                        answered = true;
//                        //tm.text = "currentSelectedObject";
//                        currentSelectedObject = highlightedObject;
//                        //tm.text = "set to null";
//                        responseInstance = null;
//                        targetInstance = null;
//                        responseProbe = null;
//                        targetProbe = null;
//                        //tm.text = "loop 1";
//                        foreach(GameObject shp in sample_shapes)
//                        {
//                            //tm.text = sample_shapes.Length;
//                            shp_tag = shp.tag;
//                            //tm.text = "trg_tag set";
//                            trg_tag = targetObject.tag;
//                            if (shp_tag == trg_tag)
//                            {
//                                //tm.text = "shp_tag";
//                                targetInstance = shp;
//                            }
//                            //tm.text = "end iteration 1";
//                        }
//                        /*
//                        if (targetInstance) {
//                            tm.text = targetInstance.tag + " existed";
//                        } else {
//                            tm.text = targetObject.tag + " presentation object does not exist";
//                        }
//                        */
                        
//                        //tm.text = currentSelectedObject.tag + " " + targetObject.tag;

//                        //tm.text = "loop 2";
//                        foreach(GameObject shp2 in sample_shapes)
//                        {
//                            shp_tag = shp2.tag;
//                            trg_tag = currentSelectedObject.tag;
//                            if (shp_tag == trg_tag)
//                            {
//                                responseInstance = shp2;
//                            }
//                        }

//                        //test responseInstance
//                        if (responseInstance) {
//                            string startPosition = responseInstance.GetComponent<Shapescript>().spawnPosition.ToString();
//                            //tm.text = "presentation position: " + startPosition;
//                        } else {
//                            //tm.text = currentSelectedObject.tag + " was not presented";
//                        }

//                        //tm.text = "loop 3";
//                        foreach(GameObject prb in sample_probes)
//                        {
//                            shp_tag = prb.tag;
//                            trg_tag = targetObject.tag;
//                            if (shp_tag == trg_tag)
//                            {
//                                targetProbe = prb;
//                            }
                            
//                        }
//                        //tm.text = "loop 4";
//                        foreach(GameObject prb2 in sample_probes)
//                        {
//                            shp_tag = prb2.tag;
//                            trg_tag = currentSelectedObject.tag;
//                            if (shp_tag == trg_tag)
//                            {
//                                responseProbe = prb2;
//                            }
                            
//                        }


//                        //tm.text = "targetShape";
//                        targetShape = targetInstance.GetComponent<Shapescript>().shapeCode; //tagCodes[targetObject.tag]; // 1-6
//                        //tm.text += targetShape;
//                        //tm.text = "targetCol";
//                        targetCol = targetInstance.GetComponent<Shapescript>().colorCode; //colorCodes[targetColor];
//                        //tm.text += targetCol;
//                        //tm.text = "targetLoc";
//                        targetLoc = targetInstance.GetComponent<Shapescript>().spawnPositionCode;
//                        //tm.text += targetLoc;
//                        //tm.text = "targetLocProbe";
//                        targetLocProbe = targetProbe.GetComponent<Shapescript>().probePositionCode;
//                        //tm.text += targetLocProbe;
//                        //tm.text = "responseShape";
//                        responseShape = currentSelectedObject.GetComponent<Shapescript>().shapeCode;
//                        //tm.text += responseShape;
//                        //tm.text = "responseLocProbe";
//                        responseLocProbe = currentSelectedObject.GetComponent<Shapescript>().probePositionCode;
//                        //tm.text += responseLocProbe;
//                        if (responseInstance) {
//                            //tm.text = "existed";
//                            responseColor = responseInstance.GetComponent<Shapescript>().colorCode;
//                            responseLoc = responseInstance.GetComponent<Shapescript>().spawnPositionCode;
//                        } else {
//                            responseColor = 0;
//                            responseLoc = 0;
//                            //tm.text = "did not exist";
//                        }
                        

//                        //tm.text = "responseTime";
//                        //responseTime = ((float)Time.realtimeSinceStartup - presentationTimeStamp) * 1000f;
                        
                        
//                        //currentSelectedObject.GetComponent<Transform>().position = GameObject.FindGameObjectsWithTag(useController)[0].GetComponent<Transform>().position;


//                        if (currentSelectedObject.tag.Equals(targetObject.tag))
//                        {
//                            //correct answer!
//                            //tm.text = ":-)";
//                            //tm.color = green;
//                            //currentSelectedObject.GetComponent<Shapescript>().setColour(targetColor);
//                            correct = true;

//                            //explode it
//                            currentSelectedObject.AddComponent<TriangleExplosion>();
//                            StartCoroutine(currentSelectedObject.GetComponent<TriangleExplosion>().SplitMesh(false));

//                        } else 
//                        {
//                            //false answer!
//                            //tm.text = ":-(";
//                            //tm.color = red;
//                            correct = false;
//                            currentSelectedObject.GetComponent<Rigidbody>().useGravity = true;

//                            /*
//                            foreach (GameObject obbj in sample_shapes)
//                            {
//                                if (obbj.tag.Equals(currentSelectedObject.tag))
//                                {
//                                    currentSelectedObject.GetComponent<Shapescript>().setColour(obbj.GetComponent<Shapescript>().getColour());
//                                }
//                            }  
//                            */

//                        }
                        

//                        // leave presentation
//                        timeObj.GetComponent<Timer>().setTime(1f);
//                        timeObj.GetComponent<Timer>().setRunning(true);
//                    }
//                break;

//            default:
//                /*
//                if (paused)
//                {
//                    // resume
//                    timeObj.GetComponent<Timer>().setRunning(true);
//                    tm.text = "resumed";
//                    paused = false;
//                }
//                else 
//                {
//                    // pause
//                    timeObj.GetComponent<Timer>().setRunning(false);
//                    tm.text = stage;
//                    tm.color = cyan;
//                    paused = true;
//                }
//                */
//                break;
//        }
//    }

//    public void primaryButtonUp()
//    {
//        //tm.text = "primary button up";
//    }

//    public void objectHoverEnter(GameObject obj_)
//    {
//        highlightedObject = obj_; //GameObject.FindGameObjectsWithTag(objname)[0];
//        highlightedObject.GetComponent<Shapescript>().scaleUp();
//        highlightedObject.GetComponent<Shapescript>().highlighted = true;
//        //tm.text = highlightedObject.tag;
        
//        foreach (GameObject obb in sample_probes)
//        {
//            if (!obb.tag.Equals(highlightedObject.tag))
//            {
//                obb.GetComponent<Shapescript>().scaleDown();
//                obb.GetComponent<Shapescript>().highlighted = false;
//            }
//        }
        
//    }

//    public void objectHoverExited(GameObject go)
//    {
//        /*
//        go.GetComponent<Shapescript>().scaleDown();
//        //tm.text = "exited";
//        if (go.tag.Equals(highlightedObject.tag))
//        {
//            highlightedObject = null;
//        }
//        */
//    }

//    public string getTrialCondition()
//    {
//        if (practice)
//        {
//            return conditions[practiceTrial-1];
//        } else {
//            int abs_trial = getAbsoluteTrial();
//            //tm.text = conditionSequence.Count.ToString();
//            string rt = conditionSequence[abs_trial-1];
//            //tm.text = rt;
//            return rt;
//        }

//    }

//    public int getAbsoluteTrial()
//    {
//        //tm.text = "((" + currentBlock.ToString()+"-1" + " * " + trials.ToString() + " + " + currentTrial.ToString();
//        int ab_t = ((currentBlock-1) * trials) + currentTrial;
//        return ab_t;
//    }

//    private string getTrialDataPath(bool timeformat)
//    {
//        if (timeformat) {
//            #if UNITY_EDITOR
//            return Application.dataPath + "/TSV/" + "time_data_" + participant_nr.ToString() + ".tsv";
//            #elif UNITY_ANDROID
//            return Application.persistentDataPath+"time_data_" + participant_nr.ToString() + ".tsv";
//            #elif UNITY_IPHONE
//            return Application.persistentDataPath+"/"+"time_data_" + participant_nr.ToString() + ".tsv";
//            #else
//            return Application.dataPath +"/"+"time_data_" + participant_nr.ToString() + ".tsv";
//            #endif
//        } else {
//            #if UNITY_EDITOR
//            return Application.dataPath + "/TSV/" + "trial_data_" + participant_nr.ToString() + ".tsv";
//            #elif UNITY_ANDROID
//            return Application.persistentDataPath+"trial_data_" + participant_nr.ToString() + ".tsv";
//            #elif UNITY_IPHONE
//            return Application.persistentDataPath+"/"+"trial_data_" + participant_nr.ToString() + ".tsv";
//            #else
//            return Application.dataPath +"/"+"trial_data_" + participant_nr.ToString() + ".tsv";
//            #endif
//        }
//    }

//    private string getPracticeDataPath(bool timeformat)
//    {
//        if (timeformat) {
//            #if UNITY_EDITOR
//            return Application.dataPath + "/TSV/" + "practice_time_data_" + participant_nr.ToString() + ".tsv";
//            #elif UNITY_ANDROID
//            return Application.persistentDataPath+"practice_time_data_" + participant_nr.ToString() + ".tsv";
//            #elif UNITY_IPHONE
//            return Application.persistentDataPath+"/"+"practice_time_data_" + participant_nr.ToString() + ".tsv";
//            #else
//            return Application.dataPath +"/"+"practice_time_data_" + participant_nr.ToString() + ".tsv";
//            #endif
//        } else {
//            #if UNITY_EDITOR
//            return Application.dataPath + "/TSV/" + "practice_trial_data_" + participant_nr.ToString() + ".tsv";
//            #elif UNITY_ANDROID
//            return Application.persistentDataPath+"practice_trial_data_" + participant_nr.ToString() + ".tsv";
//            #elif UNITY_IPHONE
//            return Application.persistentDataPath+"/"+"practice_trial_data_" + participant_nr.ToString() + ".tsv";
//            #else
//            return Application.dataPath +"/"+"practice_trial_data_" + participant_nr.ToString() + ".tsv";
//            #endif
//        }
//    }

//    private void WriteTimeData ()
//    {
//        string filePath;

//        if (practice) {
//            filePath = getPracticeDataPath(true);
//        } else {
//            filePath = getTrialDataPath(true);
//        }
//        //tm.text = "filePath set";
//        //writer writes to filepath


//        //StreamWriter writer = new StreamWriter(filePath, true);


//        //tm.text = "writer created";
//        string header = "Tick\tTime\tTriggerCode\tBlock\tTrial\tGazeDirX\tGazeDirY\tGazeDirZ\tGazePosX\tGazePosY\tGazePosZ\tHeadPosX\tHeadPosY\tHeadPosZ\tHeadDirX\tHeadDirY\tHeadDirZ\tEyeOpenL\tEyeOpenR\tPupilDiaL\tPupilDiaR\tPupilSensorLx\tPupilSensorLy\tPupilSensorRx\tPupilSensorRy\tPositionLx\tPositionLy\tPositionRx\tPositionRy\tFocusObject\tFocusNormalX\tFocusNormalY\tFocusNormalZ\tFocusDistance\tFocusPointX\tFocusPointY\tFocusPointZ\tGazeAngleX\tGazeAngleY\tHeadAngleX\tHeadAngleY\tViewGazeX\tViewGazeY\tViewGazeZ\tViewFixX\tViewFixY\tViewFixZ\tViewTriangleX\tViewTriangleY\tViewTriangleZ\tViewSphereX\tViewSphereY\tViewSphereZ\tViewStarX\tViewStarY\tViewStarZ\tHandPosX\tHandPosY\tHandPosZ";
//        if (!System.IO.File.Exists(filePath)) {
//            File.AppendAllText(filePath, header);
//            File.AppendAllText(filePath, "\n");
//            //tm.text = "wrote time header";
//        }
//        //File.AppendAllText(filePath, "Tick\tTriggerCode\tBlock\tTrial\tGazeDirX\tGazeDirY\tGazeDirZ\tGazePosX\tGazePosY\tGazePosZ\tHeadPosX\tHeadPosY\tHeadPosZ\tHeadDirX\tHeadDirY\tHeadDirZ\tEyeOpenL\tEyeOpenR\tPupilDiaL\tPupilDiaR\tFocusObject\tFocusNormalX\tFocusNormalY\tFocusNormalZ\tFocusDistance\tFocusPointX\tFocusPointY\tFocusPointZ");
//        //writer.WriteLine ();
        
//        //test

//        /*
//        File.AppendAllText(filePath, "\n");
//        //tm.text = "wrote header";
//        tm.text = triggerCodes.Count.ToString();
//        File.AppendAllText(filePath, "\t" + triggerCodes.Count.ToString());
//        tm.text = currentBlock.ToString();
//        File.AppendAllText(filePath, "\t" + currentBlock.ToString());
//        tm.text = currentTrial.ToString();
//        File.AppendAllText(filePath, "\t" + currentTrial.ToString());
//        tm.text = gaze_dir.Count.ToString();
//        File.AppendAllText(filePath, "\t" + gaze_dir.Count.ToString() );
//        tm.text = gaze_pos.Count.ToString();
//        File.AppendAllText(filePath, "\t" + gaze_pos.Count.ToString());
//        tm.text = head_pos.Count.ToString();
//        File.AppendAllText(filePath, "\t" + head_pos.Count.ToString());
//        tm.text = head_dir.Count.ToString();
//        File.AppendAllText(filePath, "\t" + head_dir.Count.ToString());
//        tm.text = eye_openness_Ls.Count.ToString();
//        File.AppendAllText(filePath, "\t" + eye_openness_Ls.Count.ToString()); 
//        tm.text = eye_openness_Rs.Count.ToString();
//        File.AppendAllText(filePath, "\t" + eye_openness_Rs.Count.ToString());
//        tm.text = pupil_dia_Ls.Count.ToString();
//        File.AppendAllText(filePath, "\t" + pupil_dia_Ls.Count.ToString());
//        tm.text = pupil_dia_Rs.Count.ToString();
//        File.AppendAllText(filePath, "\t" + pupil_dia_Rs.Count.ToString());
//        tm.text = info_Cs.Count.ToString();
//        File.AppendAllText(filePath, "\t" + info_Cs.Count.ToString());
//        tm.text = focusNormals.Count.ToString();
//        File.AppendAllText(filePath, "\t" + focusNormals.Count.ToString());
//        tm.text = focusDistances.Count.ToString();
//        File.AppendAllText(filePath, "\t" + focusDistances.Count.ToString());
//        tm.text = focusPoints.Count.ToString();
//        File.AppendAllText(filePath, "\t" + focusPoints.Count.ToString());

//        File.AppendAllText(filePath, "\n");
//        */

//        //File.AppendAllText(filePath, currentBlock.ToString());
//        //File.AppendAllText(filePath, "\n");
//        //This loops through everything
//        for (int iiii = 0; iiii < gaze_dir.Count; ++iiii) {
//            File.AppendAllText(filePath, ticks[iiii].ToString() +
//            "\t" + currentTimesMs[iiii].ToString() +
//            "\t" + triggerCodes[iiii].ToString() +
//            "\t" + currentBlocks[iiii].ToString() +
//            "\t" + currentTrials[iiii].ToString() +
//            "\t" + gaze_dir[iiii].x.ToString() +
//            "\t" + gaze_dir[iiii].y.ToString() +
//            "\t" + gaze_dir[iiii].z.ToString() +
//            "\t" + gaze_pos[iiii].x.ToString() +
//            "\t" + gaze_pos[iiii].y.ToString() +
//            "\t" + gaze_pos[iiii].z.ToString() +
//            "\t" + head_pos[iiii].x.ToString() +
//            "\t" + head_pos[iiii].y.ToString() +
//            "\t" + head_pos[iiii].z.ToString() +
//            "\t" + head_dir[iiii].x.ToString() +
//            "\t" + head_dir[iiii].y.ToString() +
//            "\t" + head_dir[iiii].z.ToString() +
//            "\t" + eye_openness_Ls[iiii].ToString() +
//            "\t" + eye_openness_Rs[iiii].ToString() +
//            "\t" + pupil_dia_Ls[iiii].ToString() +
//            "\t" + pupil_dia_Rs[iiii].ToString() +
//            "\t" + pupil_sensor_Ls[iiii].x.ToString() +
//            "\t" + pupil_sensor_Ls[iiii].y.ToString() +
//            "\t" + pupil_sensor_Rs[iiii].x.ToString() +
//            "\t" + pupil_sensor_Rs[iiii].y.ToString() +
//            "\t" + position_Ls[iiii].x.ToString() +
//            "\t" + position_Ls[iiii].y.ToString() +
//            "\t" + position_Rs[iiii].x.ToString() +
//            "\t" + position_Rs[iiii].y.ToString() +
//            "\t" + focusTags[iiii] +
//            "\t" + focusNormals[iiii].x.ToString() +
//            "\t" + focusNormals[iiii].y.ToString() +
//            "\t" + focusNormals[iiii].z.ToString() +
//            "\t" + focusDistances[iiii].ToString() +
//            "\t" + focusPoints[iiii].x.ToString() +
//            "\t" + focusPoints[iiii].y.ToString() +
//            "\t" + focusPoints[iiii].z.ToString() +
//            "\t" + gaze_angles_x[iiii].ToString() +
//            "\t" + gaze_angles_y[iiii].ToString() +
//            "\t" + head_angles_x[iiii].ToString() +
//            "\t" + head_angles_y[iiii].ToString() +
//            "\t" + viewpointsGaze[iiii].x.ToString() +
//            "\t" + viewpointsGaze[iiii].y.ToString() +
//            "\t" + viewpointsGaze[iiii].z.ToString() +
//            "\t" + viewpointsFix[iiii].x.ToString() +
//            "\t" + viewpointsFix[iiii].y.ToString() +
//            "\t" + viewpointsFix[iiii].z.ToString() +
//            "\t" + viewpointsCube[iiii].x.ToString() +
//            "\t" + viewpointsCube[iiii].y.ToString() +
//            "\t" + viewpointsCube[iiii].z.ToString() +
//            "\t" + viewpointsSphere[iiii].x.ToString() +
//            "\t" + viewpointsSphere[iiii].y.ToString() +
//            "\t" + viewpointsSphere[iiii].z.ToString() +
//            "\t" + viewpointsStar[iiii].x.ToString() +
//            "\t" + viewpointsStar[iiii].y.ToString() +
//            "\t" + viewpointsStar[iiii].z.ToString() +
//            "\t" + hand_pos[iiii].x.ToString() +
//            "\t" + hand_pos[iiii].y.ToString() +
//            "\t" + hand_pos[iiii].z.ToString() +
//            "\n");

//            /*
//            File.AppendAllText(filePath, (iiii+1).ToString()); 
//            File.AppendAllText(filePath, "\t" + time.ToString());
//            File.AppendAllText(filePath, "\t" + triggerCodes[iiii].ToString());
//            File.AppendAllText(filePath, "\t" + currentBlock.ToString());
//            File.AppendAllText(filePath, "\t" + currentTrial.ToString());
//            File.AppendAllText(filePath, "\t" + gaze_dir[iiii].x.ToString() );
//            File.AppendAllText(filePath, "\t" + gaze_dir[iiii].y.ToString());
//            File.AppendAllText(filePath, "\t" + gaze_dir[iiii].z.ToString());
//            File.AppendAllText(filePath, "\t" + gaze_pos[iiii].x.ToString());
//            File.AppendAllText(filePath, "\t" + gaze_pos[iiii].y.ToString());
//            File.AppendAllText(filePath, "\t" + gaze_pos[iiii].z.ToString());
//            File.AppendAllText(filePath, "\t" + head_pos[iiii].x.ToString());
//            File.AppendAllText(filePath, "\t" + head_pos[iiii].y.ToString());
//            File.AppendAllText(filePath, "\t" + head_pos[iiii].z.ToString());
//            File.AppendAllText(filePath, "\t" + head_dir[iiii].x.ToString());
//            File.AppendAllText(filePath, "\t" + head_dir[iiii].y.ToString());
//            File.AppendAllText(filePath, "\t" + head_dir[iiii].z.ToString());
//            File.AppendAllText(filePath, "\t" + eye_openness_Ls[iiii].ToString());
//            File.AppendAllText(filePath, "\t" + eye_openness_Rs[iiii].ToString());
//            File.AppendAllText(filePath, "\t" + pupil_dia_Ls[iiii].ToString());
//            File.AppendAllText(filePath, "\t" + pupil_dia_Rs[iiii].ToString());
//            File.AppendAllText(filePath, "\t" + pupil_sensor_Ls[iiii].x.ToString());
//            File.AppendAllText(filePath, "\t" + pupil_sensor_Ls[iiii].y.ToString());
//            File.AppendAllText(filePath, "\t" + pupil_sensor_Rs[iiii].x.ToString());
//            File.AppendAllText(filePath, "\t" + pupil_sensor_Rs[iiii].y.ToString());
//            File.AppendAllText(filePath, "\t" + position_Ls[iiii].x.ToString());
//            File.AppendAllText(filePath, "\t" + position_Ls[iiii].y.ToString());
//            File.AppendAllText(filePath, "\t" + position_Rs[iiii].x.ToString());
//            File.AppendAllText(filePath, "\t" + position_Rs[iiii].y.ToString());
//            File.AppendAllText(filePath, "\t" + focusTags[iiii]);
//            File.AppendAllText(filePath, "\t" + focusNormals[iiii].x.ToString());
//            File.AppendAllText(filePath, "\t" + focusNormals[iiii].y.ToString());
//            File.AppendAllText(filePath, "\t" + focusNormals[iiii].z.ToString());
//            File.AppendAllText(filePath, "\t" + focusDistances[iiii].ToString());
//            File.AppendAllText(filePath, "\t" + focusPoints[iiii].x.ToString());
//            File.AppendAllText(filePath, "\t" + focusPoints[iiii].y.ToString());
//            File.AppendAllText(filePath, "\t" + focusPoints[iiii].z.ToString());
//            File.AppendAllText(filePath, "\n");
//            */
            
//        }
        
//    }


//    private void WriteTrialData ()
//    {
//        string filePath;

//        if (practice) {
//            filePath = getPracticeDataPath(false);
//        } else {
//            filePath = getTrialDataPath(false);
//        }

//        //StreamWriter writer = new StreamWriter(filePath, true);
        
//        //File.AppendAllText(filePath, "Tick\tTriggerCode\tBlock\tTrial\tGazeDirX\tGazeDirY\tGazeDirZ\tGazePosX\tGazePosY\tGazePosZ\tHeadPosX\tHeadPosY\tHeadPosZ\tHeadDirX\tHeadDirY\tHeadDirZ\tEyeOpenL\tEyeOpenR\tPupilDiaL\tPupilDiaR\tFocusObject\tFocusNormalX\tFocusNormalY\tFocusNormalZ\tFocusDistance\tFocusPointX\tFocusPointY\tFocusPointZ");
//        string header = "Pp\tTrial\tBlock\tBlockTrial\tConditionCode\tCondition\tLoad\tCorrect\tRT\tTargetShape\tTargetCol\tTargetLoc\tTargetLocProbe\tRespShape\tRespCol\tRespLoc\tRespLocProbe\tHeightOffset";
//        for (int tc=0; tc<tagNames.Length; tc++)
//        {
//            foreach (string prp in obj_properties) {
//                header += "\t"+tagNames[tc] + "_" + tagCodes[tagNames[tc]].ToString() + "_" + prp.ToString();
//            }
            
//        }
//        header += "\n";

//        //writer.WriteLine ();
//        if (!System.IO.File.Exists(filePath)) {
//            File.AppendAllText(filePath, header);
//            //tm.text = "wrote header";
//        }

//        /*
//        //tm.text = "ppNr";
//        File.AppendAllText(filePath, participant_nr.ToString() );
//        //tm.text = "absTrial";
//        File.AppendAllText(filePath, "\t"+ absoluteTrial.ToString() );
//        //tm.text = "currBlck";
//        File.AppendAllText(filePath, "\t"+ currentBlock.ToString() );
//        //tm.text = "currTrial";
//        File.AppendAllText(filePath, "\t"+ currentTrial.ToString() );
//        //tm.text = "condCode";
//        File.AppendAllText(filePath, "\t"+ conditionCode.ToString() );
//        //tm.text = "currentCondition";
//        File.AppendAllText(filePath, "\t"+ currentCondition.ToString() );
//        //tm.text = "currentNumCondition";
//        File.AppendAllText(filePath, "\t"+ currentNumCondition.ToString() );
//        //tm.text = "correct";
//        File.AppendAllText(filePath, "\t"+ Convert.ToInt32(correct).ToString() );
//        //tm.text = "respTime";
//        File.AppendAllText(filePath, "\t"+ Mathf.Round(responseTime).ToString() );
//        //tm.text = "targshape";
//        File.AppendAllText(filePath, "\t"+ targetShape.ToString() );
//        //tm.text = "targCol";
//        File.AppendAllText(filePath, "\t"+ targetCol.ToString() );
//        //tm.text = "targLoc";
//        File.AppendAllText(filePath, "\t"+ targetLoc.ToString() );
//        //tm.text = "targLocProbe";
//        File.AppendAllText(filePath, "\t"+ targetLocProbe.ToString() );
//        //tm.text = "respShape";
//        File.AppendAllText(filePath, "\t"+ responseShape.ToString() );
//        //tm.text = "respCol";
//        File.AppendAllText(filePath, "\t"+ responseColor.ToString() );
//        //tm.text = "respLoc";
//        File.AppendAllText(filePath, "\t"+ responseLoc.ToString() );
//        //tm.text = "respLocProbe";
//        File.AppendAllText(filePath, "\t"+ responseLocProbe.ToString());
//        File.AppendAllText(filePath, "\t"+ heightOffset.y.ToString());
//        */

//        File.AppendAllText(filePath, participant_nr.ToString() +
//        "\t"+ absoluteTrial.ToString() +
//        "\t"+ currentBlock.ToString() +
//        "\t"+ currentTrial.ToString() +
//        "\t"+ conditionCode.ToString() +
//        "\t"+ currentCondition.ToString() +
//        "\t"+ currentNumCondition.ToString() +
//        "\t"+ Convert.ToInt32(correct).ToString() +
//        "\t"+ responseTime.ToString() +
//        "\t"+ targetShape.ToString() +
//        "\t"+ targetCol.ToString() +
//        "\t"+ targetLoc.ToString() +
//        "\t"+ targetLocProbe.ToString() +
//        "\t"+ responseShape.ToString() +
//        "\t"+ responseColor.ToString() +
//        "\t"+ responseLoc.ToString() +
//        "\t"+ responseLocProbe.ToString() +
//        "\t"+ heightOffset.y.ToString() );

//        //object infos col, loc, probe_loc, ..... exitViewTimestamp, exitViewLocation, centerExitViewTimestamp, centerExitViewLocation
        
//        foreach(string shapename in tagNames) {
//            int temp_shape_ = 0;
//            int temp_col_ = 0;
//            int temp_loc_ = 0;
//            int temp_loc_probe_ = 0;
//            temp_shape_ = tagCodes[shapename];

//            /*
//            foreach(GameObject prbObj in sample_probes) {
//                if (prbObj.CompareTag(shapename)) {
//                    //the shapename was presented
//                    temp_loc_probe_ = prbObj.GetComponent<Shapescript>().probePositionCode;
//                }
//            }
//            */

//            for (int fi=0;fi<sample_probes.Length; fi++) {
//                if (sample_probes[fi].CompareTag(shapename)) {
//                    //the shapename was presented
//                    temp_loc_probe_ = sample_probes[fi].GetComponent<Shapescript>().probePositionCode;
//                }
//            }
            
//            //col and loc if exists
//            /*
//            foreach(GameObject colcheck in sample_shapes) {
//                if (colcheck.CompareTag(shapename)) {
//                    //the shapename was presented
//                    temp_col_ = colcheck.GetComponent<Shapescript>().colorCode;
//                    temp_loc_ = colcheck.GetComponent<Shapescript>().spawnPositionCode;
//                }
//            }
//            */

//            for(int fii=0; fii<sample_shapes.Length;fii++) {
//                if (sample_shapes[fii].CompareTag(shapename)) {
//                    //the shapename was presented
//                    temp_col_ = sample_shapes[fii].GetComponent<Shapescript>().colorCode;
//                    temp_loc_ = sample_shapes[fii].GetComponent<Shapescript>().spawnPositionCode;
//                }
//            }

//            //write this shape info
//            File.AppendAllText(filePath, "\t" + temp_shape_.ToString());
//            File.AppendAllText(filePath, "\t"+ temp_col_.ToString() );
//            File.AppendAllText(filePath, "\t"+ temp_loc_.ToString() );
//            File.AppendAllText(filePath, "\t"+ temp_loc_probe_.ToString() );

//        }

//        File.AppendAllText(filePath, "\n");

//        //actual
//        /*
//        File.AppendAllText(filePath, 
//        participant_nr.ToString() +
//        "\t" + absoluteTrial.ToString() +
//        "\t" + currentBlock.ToString() +
//        "\t" + currentTrial.ToString() +
//        "\t" + conditionCode.ToString() +
//        "\t" + correct.ToString() +
//        "\t" + responseTime.ToString() +
//        "\t" + targetShape.ToString() +
//        "\t" + targetCol.ToString() +
//        "\t" + targetLoc.ToString() +
//        "\t" + targetLocProbe.ToString() +
//        "\t" + responseShape.ToString() +
//        "\t" + responseColor.ToString() +
//        "\t" + responseLoc.ToString() +
//        "\t" + responseLocProbe.ToString()
//        );
//        */
        
//        //tm.text = "writing finished";
//        /*
//        writer.Flush();
//        tm.text = "writer flushed";
//        writer.Close();
//        tm.text = "writer closed";
//        */
        
//    }

//    /*
//    private void WriteTrialData ()
//    {
//        string filePath;

//        if (practice) {
//            filePath = getPracticeDataPath(false);
//        } else {
//            filePath = getTrialDataPath(false);
//        }
        
//        //writer writes to filepath
//        StreamWriter writer2 = new StreamWriter(filePath, true);
        
//        //writer.WriteLine ("Trial\tBlock\tBlockTrial\tCondition\tLoad");
//        writer2.WriteLine ((iiii+1).ToString() + 
//                "\t" + currentBlock.ToString() +
//                "\t" + currentTrial.ToString() +
//                "\t" + conditionCode.ToString() +
//                "\t" + correct.ToString() +
//                "\t" + responseTime.ToString() +
//                "\t" + targetShape.ToString() +
//                "\t" + responseShape.ToString() +
//                "\t" + responseColor.ToString() +
//                "\t" + targetColor.ToString() +
//                );
//        }
//        writer2.Flush();
//        //This closes the file
//        writer2.Close();
//    }
//    */
    

//    private void ClearTempData()
//    {
//        //time series
//        gaze_dir.Clear();
//        gaze_pos.Clear();
//        head_pos.Clear();
//        head_dir.Clear();
//        //presentation_positions.Clear();
//        //presentation_anglediffs.Clear();
//        eye_openness_Ls.Clear();
//        eye_openness_Rs.Clear();
//        pupil_dia_Ls.Clear();
//        pupil_dia_Rs.Clear();
//        pupil_sensor_Ls.Clear();
//        pupil_sensor_Rs.Clear();
//        position_Ls.Clear();
//        position_Rs.Clear();
//        ray_Ls.Clear();
//        ray_Rs.Clear();
//        ray_Cs.Clear();
//        info_Ls.Clear();
//        info_Rs.Clear();
//        info_Cs.Clear();
//        focusNormals.Clear();
//        focusDistances.Clear();
//        focusPoints.Clear();
//        triggerCodes.Clear();
//        focusTags.Clear();
//        currentTimesMs.Clear();
//        ticks.Clear();
//        viewpointsGaze.Clear();
//        viewpointsFix.Clear();
//        viewpointsCube.Clear();
//        viewpointsSphere.Clear();
//        //viewpointsCylinder.Clear();
//        //viewpointsTriangle.Clear();
//        //viewpointsDiamond.Clear();
//        viewpointsStar.Clear();
//        hand_pos.Clear();
//        currentTrials.Clear();
//        currentBlocks.Clear();
//    }

//    public void ResetHeight() {
//        camHeight = new Vector3(0.0f, Camera.main.transform.position.y, 0.0f);
        
//        tm.transform.position = new Vector3(0f,0.0f,5.0f) + camHeight + relHeight;

//        heightOffset.y = tm.transform.position.y;
//        //refPos.y = Camera.main.transform.position.y;
//    }

//    public int positionToCode(Vector3 spawn_pos) {
//        return positionCodes[spawn_pos-heightOffset];
//    }

//    public float GetAngleOnAxis(Vector3 dir1, Vector3 dir2, Vector3 axis)
//    {
//        Vector3 perpendicularDir1 = Vector3.Cross(axis, dir1);
//        Vector3 perpendicularDir2 = Vector3.Cross(axis, dir2);
//        return Vector3.SignedAngle(perpendicularDir1, perpendicularDir2, axis);
//    }

//    public Vector2 GetVectorAngle(Vector3 vec){
//        float xAngle = GetAngleOnAxis(Vector3.forward, vec, Vector3.up);
//        float yAngle = GetAngleOnAxis(Vector3.forward, vec, Vector3.left);
//        return new Vector2(xAngle,yAngle);
//    }

//}
