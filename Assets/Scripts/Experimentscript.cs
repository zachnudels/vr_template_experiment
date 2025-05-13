using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine.XR;
using System.Diagnostics;
using UnityEngine.XR.Interaction.Toolkit;


public class Experimentscript : MonoBehaviour
{

    // search for "changed" to uncomment them

    // 40 deg at 8m is 6.71
    // 5 = 0.70
    // 15 = 2.14
    // 25 = 3.73
    // 35 = 5.60

    // settings
    public string expid; // Append experiment name with 'practice' where appropriate
    public int blocks = 10;
    public int trials = 40;
    public const float fixationOffset = 0.0f; //-0.05f; // -20 cm
    public const float probeOffset = 0.0f; // -10 cm
    public const float fixedStartDelay = 1.0f;

    private float feedbackTime = 1.0f;
    //public string runType = "none";
    public string participant_id = "";
    private int trialsPerBlock;
    private string feedback = "acc"; // may include "rt", "acc" or "err"

    private string trialdataName, timedataName;
    private int probeRows = 4;
    private int probeCols = 4;
    private int respCount = 4;

    //constants
    //public const float precueDuration = 0.750f;
    public const float preparationTime = 0f;
    public const float anticipationTime = 0f;
    public const float presentationTime = 0.011f;
    private const float fps = 90f;
    private const float turnTime = 2.5f; // 3 sec turn + 1s wait
    private float degPerSec = 180f / turnTime;
    private float degPerStep;
    public float jitterTimeMin = fixedStartDelay; // 500ms
    public float jitterTimeMax = fixedStartDelay; // 1000ms

    private const float verticalDev = 0.10f; //0.25f;
    private Color defaultFixationColour = new Color(77f / 255f, 77f / 255f, 77f / 255f);
    private Color defaultFixationvisualColour = new Color(255f, 0f, 0f, 0.3f);
    private float fixationDepth = 2.0f;//2.50f; //1.25f;
    private Vector3 textPosFar = new Vector3(0f, 1.8f, 5f);
    private int farFontSize = 20;
    private int nearFontSize = 20;
    private int viewFontSize = 20;
    private float farCharacterSize = 0.1f;
    private float nearCharacterSize = 0.015f;
    private float viewCharacterSize = 0.02f;
    private float interStimuliDistance = 0.15f;


    private Vector3 xMulVec = new Vector3(-1f, 1f, 1f);
    private Vector3 zMulVec = new Vector3(1f, 1f, -1f);

    private bool calibratedHeight = false;

    // used for stimuli feature
    public static Color green = Color.green;
    public static Color magenta = Color.magenta;
    public static Color cyan = Color.cyan;
    public static Color yellow = Color.yellow;

    public static Color red = Color.red;
    public static Color blue = Color.blue;
    public static Color gray = Color.gray;
    public static Color black = Color.black;

    private Color defaultTxtColour = black;
    private Color errorTxtColour = red;

    public ViveSR.anipal.Eye.EyeData eyeData;

    // the data structures.
    //time series
    public Vector3 gazeDirectionCombined;
    public Vector3 centerEyePos;
    Dictionary<string, int> tagCodes;
    Dictionary<string, int> conditionCodes;
    Dictionary<Vector3, int> positionCodes;
    Dictionary<Color, int> colorCodes;
    Dictionary<Vector3, int> posProbeCodes;
    float time;
    float presentationTimeStamp;
    float gaze_angle_x, gaze_angle_y, head_angle_x, head_angle_y;
    private float EscTime = 0;

    private List<Vector3> gaze_dir;
    private List<Vector3> gaze_pos;
    private List<Vector3> head_pos;
    private List<Vector3> head_dir;
    private List<Vector3> presentation_positions;
    private List<Vector3> presentation_anglediffs;
    List<float> eye_openness_Ls, eye_openness_Rs;
    List<float> pupil_dia_Ls, pupil_dia_Rs;
    List<Vector2> position_Ls, position_Rs;
    List<Vector2> pupil_sensor_Ls, pupil_sensor_Rs;
    List<Ray> ray_Ls, ray_Rs, ray_Cs;
    List<ViveSR.anipal.Eye.FocusInfo> info_Ls, info_Rs, info_Cs;
    List<Vector3> focusNormals;
    List<float> focusDistances;
    List<Vector3> focusPoints;
    List<int> triggerCodes;
    List<string> focusTags;
    List<long> currentTimesMs;
    List<int> currentBlocks;
    List<int> currentTrials;
    List<int> ticks;
    List<float> gaze_angles_x, gaze_angles_y;
    List<float> head_angles_x, head_angles_y;

    List<Vector3> viewpointsFix, viewpointsGaze;
    List<Vector3> viewpointsCube, viewpointsSphere, viewpointsDiamond, viewpointsStar; //viewpointsCylinder,viewpointsTriangle


    List<Vector3> screenPosGazes;
    List<Vector3> screenPosFixations;
    List<Vector3> screenPosCubes;
    //List<Vector3> screenPosBars;
    List<Vector3> screenPosSpheres;
    //List<Vector3> screenPosCylinders;
    //List<Vector3> screenPosTriangles;
    List<Vector3> screenPosDiamonds;
    List<Vector3> screenPosStars;

    //HANDTRACKING
    List<Vector3> hand_pos;
    List<Vector3> hand_dir;
    List<Vector3> hand_posL;
    List<Vector3> hand_dirL;
    List<Vector3> fixationPoints;
    List<int> visualMatchesHand, worldMatchesHand;

    private bool present;
    private bool encodingShown = false;
    private int removeAttempts = 0;


    GameObject presCube, presSphere, presDiamond, presStar;//presCube, presTriangle, presStar, presBar; //presCube, presSphere,presCylinder,presTriangle,presDiamond,presStar;

    //other eye data
    float eye_openness_L, eye_openness_R;
    float pupil_dia_L, pupil_dia_R;
    Vector2 position_L, position_R;
    Vector2 pupil_sensor_L, pupil_sensor_R;
    Ray ray_L, ray_R, ray_C;
    ViveSR.anipal.Eye.FocusInfo info_L, info_R, info_C;
    Vector3 focusNormal;
    float focusDistance;
    Vector3 focusPoint;
    Vector3 cameraPos;
    Vector3 cameraRot;
    string focusTag;
    Vector3 intersection = new Vector3(0f, 0f, 0f);
    Vector3 origin_L = new Vector3(0f, 0f, 0f);
    Vector3 direction_L = new Vector3(0f, 0f, 0f);
    Vector3 origin_R = new Vector3(0f, 0f, 0f);
    Vector3 direction_R = new Vector3(0f, 0f, 0f);

    // pseudo-random 6x50 conditions
    public List<string> conditionSequence;

    // pseudo-random 3x3 locations
    public List<Vector3> locSequence;

    // external input
    public GameObject fixationtarget;
    public GameObject txtobj;
    public GameObject timer;
    private GameObject[] timers;

    //public GameObject respObj;
    public Shader outline;

    //alternative choice objects
    //private GameObject visualitem;
    //private GameObject worlditem;

    public bool practice = true;
    public int practiceTrial = 0;
    public bool testing = false;

    private bool calibrated;
    private bool calibrationStarted;
    private int front = -1;
    private int correctness = 0;
    private bool textFollowView = false;
    private Vector3 viewTextDistVec = new Vector3(0.3f, 0.3f, 0.3f);

    public GameObject leftHandObj;
    public GameObject rightHandObj;

    private int colEnc, shapeEnc;

    // constants
    private string[] conditions = new string[] { "l", "r" };// 1 through 4 respectively past to future locations

    private string[] tagNames = new string[] { "cube", "sphere", "diamond", "star" };
    string[] obj_properties = new string[] { "or", "color", "loc", "probe" };

    private const int objectCount = 4;
    private const int probeCount = 16;
    private const float presenceChance = 100f;
    private const float probeScaleFactor = 0.2f;
    private int totalTrials;
    public int conditionCountBlock;
    private int locationCountBlock = 1;
    private string useController = "rightHand";
    private Vector3 probeScale = new Vector3(0.1f, 0.1f, 0.1f);
    private Vector3 probeScaleBig = new Vector3(0.12f, 0.12f, 0.12f);
    Vector3 refPos = new Vector3(0.0f, 1.25f, 0.0f);
    Vector3 refPosProbe = new Vector3(0.0f, 1.2f, 0.0f);
    Vector3 relHeight = new Vector3(0f, fixationOffset, 0f);
    Vector3 relHeightProbe = new Vector3(0f, probeOffset, 0f);
    private float turnSign = 1f;
    private float currentOrbit = 0f;
    private float remainingOrbit = 0f;
    private Vector2 newXY;
    private string facing = "front";
    private float catchProb;
    private int catchCount;
    private float RTscore = 0f;
    private int ansCount = 0;
    private int facingAdd = 0;

    //probes
    private List<int> shapeOrder = new List<int> { 0, 1, 2, 3 };
    private List<int> colOrder = new List<int> { 0, 1, 2, 3 };
    private List<int> encShapeOrder = new List<int> { 0, 1, 2, 3 };
    private List<int> encColOrder = new List<int> { 0, 1, 2, 3 };

    // collection components
    //public GameObject bar;
    public GameObject cube;
    public GameObject sphere;
    //public GameObject cylinder;
    //public GameObject triangle;
    public GameObject diamond;
    public GameObject star;
    public GameObject respHand;
    private string handUsed;
    private string correctHand;

    private GameObject obj1;
    private GameObject obj2;

    // preset collections
    public Color[] colors;
    public GameObject[] shapes;
    public Vector3[] positions;
    public Vector3[] positions_2;
    public Vector3[] positions_6;
    public Vector3[] positions_flow;
    public Vector3[] positions_flow_2;
    public Vector3[] positions_flow_6;
    public Vector3[] positions_probes;
    public Vector3[] positions_probes_2;
    public Vector3[] positions_probes_6;
    public Vector3[] positions_all;
    public Vector3[] positions_circle;
    public Vector3[] positions_circle_flow;
    public Vector3[] left_half;
    public Vector3[] right_half;
    public Vector3[] left_half_flow;
    public Vector3[] right_half_flow;
    public Vector3[] positions_up;
    public Vector3[] positions_down;
    private Vector3[] positions_localisation;
    private int[] rotations;

    public Vector3[] positions1;
    public Vector3[] positions2;
    public Vector3[] positions3;
    public Vector3[] positions4;
    private Vector3[] positions_vert;
    public Vector3 encodingVerticalDeviation;

    public Dictionary<Color, string> colorNames = new Dictionary<Color, string>();

    // temporary structures
    private GameObject[] sample_shapes; // presentation objects
    private Color[] sample_colors; //presentation colors
    private GameObject[] sample_probes; //probe objects
    private Color[] probes_colors; // probe colors
    private int[,] presentationInfo; //this contains object info for presentation array: [index, color, shape]
    //private bool[] ConfirmationArray;
    //private bool[] ProbeConfirmationArray;

    //structure to store encoding location response info
    private int[,] encLocInfo;
    private int[,] respInfo;
    private int[,] colInfo;
    private int[,] shapeInfo;

    // flags and counters
    //private bool paused = false;
    public string stage = "none";
    private int blockCount = 1;
    private int trialCount = 1;
    public GameObject selected;
    public int timerCounter = 0;
    private int ColorRandomNumber;
    private int ShapeRandomNumber;
    private bool isATarget;
    public string currentCondition = "none"; // unused
    public string currentConditionTurn = "none"; // "left", "right"
    bool conditionControl;
    public int currentNumCondition = 0; // 0, 2, 4, 6
    public Vector3[] currentPositions;
    public Vector3[] currentProbePositions;
    public GameObject currentSelectedObject;
    float triggerValue;
    public GameObject highlightedObject;
    public GameObject targetObject;
    private GameObject distractorObject;
    private bool answered = false;
    Color targetColor;
    Color distractorColor;
    private bool correct = false;
    public GameObject closest = null;
    public GameObject testinstance;
    public bool pressed = false;
    public int pressCount = 0;
    public int currentTrial = 0;
    public int currentBlock = 0;
    int triggerCode = 0;
    string conditionName;
    int addition_num = 0;
    int currentTick;
    private bool assessmentShown;
    private int respLoc = 0;

    // formatted trial data
    int absoluteTrial = 0; // 1-500
    //currentBlock
    //currentTrial
    int conditionCode = 0; // 1-6
    //correct // 0|1
    long responseTime; //ms
    int targetShape = 0; // 1-6

    int targetCol;
    int targetLoc;
    int targetLocProbe;
    int responseShape;
    int responseColor;
    int responseLoc;
    int responseLocProbe;

    Vector3 screenPosGaze;
    Vector3 screenPosFixation;
    Vector3 screenPosCube;
    Vector3 screenPosSphere;
    //Vector3 screenPosCylinder;
    //Vector3 screenPosTriangle;
    Vector3 screenPosDiamond;
    Vector3 screenPosStar;
    Vector3 hand_position;
    Vector3 hand_direction;
    Vector3 hand_positionL;
    Vector3 hand_directionL;
    Vector3 fixationPoint;


    // instances
    public TextMesh tm;
    private GameObject testObj;
    private GameObject timeObj;
    public GameObject targetInstance;
    public GameObject responseInstance;
    public GameObject targetProbe;
    public GameObject responseProbe;

    private string raySet = "none";

    private int targetIndex;
    private int distractorIndex;
    List<int> takenPos;
    int m;

    List<int> shapeSeq;
    List<int> colSeq;
    List<int> extra_list;
    string shp_tag;
    string trg_tag;

    Vector3 camHeight;
    Vector3 heightOffset;

    Stopwatch watch;
    long oldTime;
    long newTime;
    long currentTimeMs;
    private string condstr = "";

    private UnityEngine.XR.InputDevice handR;
    private UnityEngine.XR.InputDevice handL;


    XRInputSubsystem subSys;

    //OUTPUT FILE
    //tab-separated
    //

    //Dictionary<string, List<string>> dataset = new Dictionary<string, List<string>>();
    //private List<Vector3> head_positions

    //controllers
    List<UnityEngine.XR.InputDevice> controllerList = new List<UnityEngine.XR.InputDevice>();

    // Start is called before the first frame update


    //PAUSE RELATED
    /*
    bool isPaused = false;
    void OnGUI()
    {
        if (isPaused)
            GUI.Label(new Rect(100, 100, 50, 30), "Game paused");
    }
    void OnApplicationFocus(bool hasFocus)
    {
        isPaused = !hasFocus;
    }
    void OnApplicationPause(bool pauseStatus)
    {
        isPaused = pauseStatus;
    }
    // END PAUSE RELATED
    */


    void Start()
    {

        //UnityEngine.Application.targetFrameRate = 3;

        timedataName = expid + "_time_data_" + participant_id;
        trialdataName = expid + "_trial_data_" + participant_id;

        totalTrials = blocks * trials;

        positions4 = new Vector3[] { 
            new Vector3(-fixationDepth, 0.0f, 0.3f), 
            new Vector3(-fixationDepth, 0.0f, 0.1f),
            new Vector3(-fixationDepth, 0.0f, -0.1f),
            new Vector3(-fixationDepth, 0.0f, -0.3f)
        };
        positions_probes = new Vector3[] {
            new Vector3(-0.3f, 0.3f, fixationDepth), new Vector3(-0.1f, 0.3f, fixationDepth), new Vector3(0.1f, 0.3f, fixationDepth), new Vector3(0.3f, 0.3f, fixationDepth),
            new Vector3(-0.3f, 0.1f, fixationDepth), new Vector3(-0.1f, 0.1f, fixationDepth), new Vector3(0.1f, 0.1f, fixationDepth), new Vector3(0.3f, 0.1f, fixationDepth),
            new Vector3(-0.3f, -0.1f, fixationDepth), new Vector3(-0.1f, -0.1f, fixationDepth), new Vector3(0.1f, -0.1f, fixationDepth), new Vector3(0.3f, -0.1f, fixationDepth),
            new Vector3(-0.3f, -0.3f, fixationDepth), new Vector3(-0.1f, -0.3f, fixationDepth), new Vector3(0.1f, -0.3f, fixationDepth), new Vector3(0.3f, -0.3f, fixationDepth)
        };

        eyeData = new ViveSR.anipal.Eye.EyeData();

        degPerStep = degPerSec / fps;
        //hideRespObj();
        //hideRespObj();

        handR = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        handL = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        leftHandObj = GameObject.FindGameObjectWithTag("leftHand");
        rightHandObj = GameObject.FindGameObjectWithTag("rightHand");

        //catchProb = catchPercent / 100f;
        //expid += groupType; obsolete grouptype
        //float matchProb = catchProb;
        //int totalCount = (int)Math.Floor(trials / matchProb);
        //catchCount = (int)Math.Floor(trials * catchProb);

        /*
        if (groupType.ToLower().Equals("c"))
        {
            currentConditions = drawconditions;
            catchCount = 0;

            if (runType.Equals("learn"))
            {
                expid += "LEARN";
                trials = 16;
                blocks = 1;
                
            }
            else if (runType.Equals("try"))
            {
                expid += "TRY";
                trials = 16;
                blocks = 1;
            }

        } else
        {
            currentConditions = learnconditions; //= conditions

            if (runType.Equals("learn"))
            {
                expid += "LEARN";
                trials = 16;
                blocks = 1;
                currentConditions = learnconditions;
                catchCount = 0;//trials;
                catchProb = 0f;
            }
            else if (runType.Equals("try"))
            {
                expid += "TRY";
                trials = 16;
                blocks = 1;
                
            }
        }
        */

        conditionCountBlock = trials / conditions.Length; // 

        //trialsPerBlock = trials + ((catchCount/drawconditions.Length)*4);

        currentBlock = 1;
        currentTick = 1;
        currentTimeMs = 0L;
        watch = Stopwatch.StartNew();
        subSys = new XRInputSubsystem();

        targetInstance = new GameObject();
        responseInstance = new GameObject();
        targetProbe = new GameObject();
        responseProbe = new GameObject();

        gaze_dir = new List<Vector3>();
        gaze_pos = new List<Vector3>();
        head_pos = new List<Vector3>();
        head_dir = new List<Vector3>();
        eye_openness_Ls = new List<float>();
        eye_openness_Rs = new List<float>();
        pupil_dia_Ls = new List<float>();
        pupil_dia_Rs = new List<float>();
        position_Ls = new List<Vector2>();
        position_Rs = new List<Vector2>();
        pupil_sensor_Ls = new List<Vector2>();
        pupil_sensor_Rs = new List<Vector2>();
        ray_Ls = new List<Ray>();
        ray_Rs = new List<Ray>();
        ray_Cs = new List<Ray>();
        info_Ls = new List<ViveSR.anipal.Eye.FocusInfo>();
        info_Rs = new List<ViveSR.anipal.Eye.FocusInfo>();
        info_Cs = new List<ViveSR.anipal.Eye.FocusInfo>();
        focusNormals = new List<Vector3>();
        focusDistances = new List<float>();
        focusPoints = new List<Vector3>();
        triggerCodes = new List<int>();
        focusTags = new List<string>();
        currentTimesMs = new List<long>();
        currentBlocks = new List<int>();
        currentTrials = new List<int>();
        ticks = new List<int>();
        gaze_angles_x = new List<float>();
        gaze_angles_y = new List<float>();
        head_angles_x = new List<float>();
        head_angles_y = new List<float>();
        viewpointsFix = new List<Vector3>();
        viewpointsGaze = new List<Vector3>();
        viewpointsCube = new List<Vector3>();
        //viewpointsBar = new List<Vector3>();
        viewpointsSphere = new List<Vector3>();
        //viewpointsCylinder = new List<Vector3>();
        //viewpointsTriangle = new List<Vector3>();
        viewpointsDiamond = new List<Vector3>();
        viewpointsStar = new List<Vector3>();
        hand_pos = new List<Vector3>();
        hand_dir = new List<Vector3>();
        hand_posL = new List<Vector3>();
        hand_dirL = new List<Vector3>();
        fixationPoints = new List<Vector3>();
        visualMatchesHand = new List<int>();
        worldMatchesHand = new List<int>();

        screenPosCube = new Vector3();
        //screenPosBar = new Vector3();
        screenPosSphere = new Vector3();
        //screenPosCylinder = new Vector3();
        //screenPosTriangle = new Vector3();
        screenPosDiamond = new Vector3();
        screenPosStar = new Vector3();
        hand_position = new Vector3();
        fixationPoint = new Vector3(0f, 0f, 0f);

        //setup tagCodes and positionCodes
        tagCodes = new Dictionary<string, int>();
        positionCodes = new Dictionary<Vector3, int>();
        colorCodes = new Dictionary<Color, int>();
        posProbeCodes = new Dictionary<Vector3, int>();
        conditionCodes = new Dictionary<string, int>();


        //camHeight = new Vector3(0.0f, 1.2f, 0.0f);
        heightOffset = new Vector3(0.0f, 0.0f, 0.0f);

        assessmentShown = false;

        // generate condition sequence
        //int trials_per_cond = totalTrials / conditions.Length;


        int counterr = 0;

        // 1st half
        for (int bl = 0; bl < blocks; bl++) // block instead of blocks/2, as 2nd half run separately
        {
            List<string> condSeqBlock = new List<string>();
            foreach (string cond in conditions) //changed to exclude reach
            {

                for (int cc = 0; cc < conditionCountBlock; cc++) //conditionCountBlock
                {
                    condSeqBlock.Add(cond);
                    counterr += 1;
                }

            }
            condSeqBlock = condSeqBlock.OrderBy(x => UnityEngine.Random.value).ToList(); //shuffle condition sequence of current block
            foreach (string csb in condSeqBlock)
            {
                conditionSequence.Add(csb);
                //condstr += " " + csb;
            }
        }

        // switch the condition type for 2nd half
        // REVERTED : second half is run separately
        /*
        if (ordergroup.Equals("a"))
        {
            currentConditions = grabconditions; // reversed

        }
        else if (ordergroup.Equals("b"))
        {
            currentConditions = reportconditions; // reversed

        }

        // 2nd half
        for (int bl = 0; bl < blocks / 2; bl++)
        {
            List<string> condSeqBlock = new List<string>();
            foreach (string cond in currentConditions) //changed to exclude reach
            {

                for (int cc = 0; cc < conditionCountBlock; cc++) //conditionCountBlock
                {
                    condSeqBlock.Add(cond);
                    counterr += 1;
                }

            }
            condSeqBlock = condSeqBlock.OrderBy(x => UnityEngine.Random.value).ToList(); //shuffle condition sequence of current block
            foreach (string csb in condSeqBlock)
            {
                conditionSequence.Add(csb);
                //condstr += " " + csb;
            }
        }

        //revert condition type to original for the ordergroup
        if (ordergroup.Equals("a"))
        {
            currentConditions = reportconditions;

        }
        else if (ordergroup.Equals("b"))
        {
            currentConditions = grabconditions;

        }
        */

        // preset collections
        colors = new Color[] { green, magenta, cyan, yellow };
        //shapes = new GameObject[] { cube, triangle, star };//new GameObject[] {cube, sphere, cylinder, triangle, diamond, star};
        shapes = new GameObject[] { cube, sphere, diamond, star };

        //positions4 = new Vector3[] { new Vector3(-5.60f, 1.25f, 4f), new Vector3(5.60f, 1.25f, 4f) }; // 35 deg {new Vector3(-5f, 1.25f, 4f), new Vector3(5f, 1.25f, 4f)};
        //positions_circle_flow = positions_circle;

        positions_down = new Vector3[] { new Vector3(0f, 0f, fixationDepth - 0.50f), new Vector3(0f, 0f, fixationDepth + 0.50f) }; //new Vector3[] { new Vector3(0f, 0f, -3f), new Vector3(0f, 0f, -1f) };

        rotations = new int[] { 0, 0, 0, 0 };

        //colorNames
        colorNames.Add(green, "green");
        colorNames.Add(magenta, "magenta");
        colorNames.Add(cyan, "cyan");
        colorNames.Add(yellow, "yellow");

        // temporary structures
        sample_shapes = new GameObject[objectCount]; // presentation objects
        sample_colors = new Color[objectCount]; //presentation colors
        sample_probes = new GameObject[probeCount]; //probe objects
        probes_colors = new Color[probeCount]; // probe colors
        presentationInfo = new int[objectCount, 2]; //this contains object info for presentation array: [index, color, shape]
        //ConfirmationArray = new bool[colors.Length, shapes.Length];
        //ProbeConfirmationArray = new bool[colors.Length, shapes.Length];

        encLocInfo = new int[objectCount, 6]; // per location: correct, respRank
        respInfo = new int[respCount, 8];
        colInfo = new int[colors.Length,7];
        shapeInfo = new int[shapes.Length,7];

        int somecounter1 = 0;
        foreach (Vector3 fixedPos in positions4)
        {
            somecounter1 += 1;
            positionCodes.Add(fixedPos, somecounter1);
        }

        /*
        foreach (Vector3 fixedPos in positions_circle_flow)
        {
            somecounter1 += 1;
            positionCodes.Add(fixedPos, somecounter1 + 2);
        }
        */



        somecounter1 = 0;
        foreach (string fixedName in conditions)
        {
            somecounter1 += 1;
            conditionCodes.Add(fixedName, somecounter1);
        }

        somecounter1 = 0;
        foreach (string fixedName in tagNames)
        {
            somecounter1 += 1;
            tagCodes.Add(fixedName, somecounter1);
        }
        somecounter1 = 0;
        foreach (Color fixedColor in colors)
        {
            somecounter1 += 1;
            colorCodes.Add(fixedColor, somecounter1);
        }
        somecounter1 = 0;
        foreach (Vector3 fixedProbe in positions_probes)
        {
            somecounter1 += 1;
            posProbeCodes.Add(fixedProbe, somecounter1);
        }


        timeObj = Instantiate(timer, new Vector3(0, 0, 0), Quaternion.identity);

        tm = Instantiate(txtobj, new Vector3(0f, 1.5f, 5f), Quaternion.identity).GetComponent<TextMesh>(); // z=5f is front wall
        //tm.fontSize = 35;

        if (testing)
        {
            tm.text = "testing"; // Application.dataPath + Application.persistentDataPath;

        }
        else
        {
            tm.text = "Press index finger trigger to start calibration.";
        }
        //tm.text = condstr;

        //tm.text = conditionSequence.Count.ToString();
        //tm.text = conditionSequence[5].ToString();

        //timeObj.GetComponent<Timer>().setTime(5f);
        //timeObj.GetComponent<Timer>().setRunning(true);

        //test

        var leftHandedControllers = new List<UnityEngine.XR.InputDevice>();
        var desiredCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller;
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, leftHandedControllers);
        //Debug.Log("HELLO");
        foreach (var device in leftHandedControllers)
        {
            //Debug.Log(string.Format("Device name '{0}' has characteristics '{1}'", device.name, device.characteristics.ToString()));
            controllerList.Add(device);
        }

        var rightHandedControllers = new List<UnityEngine.XR.InputDevice>();
        var desiredCharacteristicsR = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller;
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredCharacteristicsR, rightHandedControllers);
        //Debug.Log("HELLO");
        foreach (var device in rightHandedControllers)
        {
            //Debug.Log(string.Format("Device name '{0}' has characteristics '{1}'", device.name, device.characteristics.ToString()));
            controllerList.Add(device);
        }


        //set fixationvisual alpha
        if (GameObject.FindGameObjectsWithTag("fixationvisual").Length > 0)
        {
            //tm.text = "object";
            GameObject.FindGameObjectsWithTag("fixationvisual")[0].GetComponent<MeshRenderer>().material.color = defaultFixationvisualColour;
        }



        //debug text condition sequence
        Dictionary<string, int> conditionCounts = new Dictionary<string, int>();
        foreach (string cnm in conditions)
        {
            conditionCounts.Add(cnm, 0);
        }
        foreach (string cd in conditionSequence)
        {
            conditionCounts[cd] = conditionCounts[cd] + 1;
        }
        //debug experiment values
        /*
       tm.text = "total trial count: " + conditionSequence.Count.ToString();
       foreach (string cstr in conditions)
       {
           int c_count = conditionCounts[cstr];
           tm.text += "\n" + cstr + ": " + c_count;
       }
        */



        //UnityEngine.Debug.Log("start ended");
    }

    void Update()
    {
        UnityEngine.Debug.Log("update started");
        currentTimeMs = watch.ElapsedMilliseconds;

        //float handRotation = GameObject.FindGameObjectsWithTag(useController)[0].transform.eulerAngles.y;
        //tm.text = handRotation.ToString();

        handL.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out Quaternion rotL);
        handR.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out Quaternion rotR);
        /*
        handTiltR = Vector3.SignedAngle(Vector3.up, Vector3.ProjectOnPlane(rotR * Vector3.forward, Vector3.forward), Vector3.forward);
        handRollR = Vector3.SignedAngle(Vector3.up, Vector3.ProjectOnPlane(rotR * Vector3.up, Vector3.forward), Vector3.forward);
        handTiltL = Vector3.SignedAngle(Vector3.up, Vector3.ProjectOnPlane(rotL * Vector3.forward, Vector3.forward), Vector3.forward);
        handRollL = Vector3.SignedAngle(Vector3.up, Vector3.ProjectOnPlane(rotL * Vector3.up, Vector3.forward), Vector3.forward);


        handTiltR = (handTiltR < 0) ? 360f + handTiltR : handTiltR;
        handTiltL = (handTiltL < 0) ? 360f + handTiltL : handTiltL;
        handRollR = (handRollR < 0) ? 360f + handRollR : handRollR;
        handRollL = (handRollL < 0) ? 360f + handRollL : handRollL;

        if (facing.Equals("back"))
        {
            handRollR = 360f - handRollR;
            handRollL = 360f - handRollL;
        }
        else if (facing.Equals("front"))
        {
            handTiltR = 360f - handTiltR;
            handTiltL = 360f - handTiltL;
        }

        
        if (handMatchRot < 0)
        {
            handMatchRot = 360f + handMatchRot;
        }
        if (handMatchRotZ < 0)
        {
            handMatchRotZ = 360f + handMatchRotZ;
        }
        */

        /*
        currentRotation = rotR.eulerAngles.z; // will be overwritten

        if (trialType.Equals("match"))
        {
            currentRotation = handRollR;
        }
        else if (trialType.Equals("reach"))
        {
            if (currentConditionLat.Equals("left"))
            {
                currentRotation = handTiltR;
            }
            else if (currentConditionLat.Equals("right"))
            {
                currentRotation = handTiltL;
            }
        }




        currentRotationMirrored = 360f - currentRotation;

        visualMatchHand = -1;
        worldMatchHand = -1;
        */

        /*
        tm.text = "\ntargetRotation: " + targetRotation.ToString()+ "\n" +
            "tiltL: " + handTiltL.ToString() + "\ttiltR: " + handTiltR.ToString();
        */

        //tm.text = currentRotation.ToString() + "\n" + rotations[0].ToString() + "\n" + ScoreFromAngles(currentRotation,(float)rotations[0]).ToString();


        /* //debug print orientations
        if (stage.Equals("answer"))
        {
            tm.text = "target: " + targetRotation.ToString() + "\n"
                + "visual frame response: " + ((int)currentRotation).ToString() + "\n"
                + "world frame response: " + ((int)currentRotationMirrored).ToString() + "\n"
                + "visual frame score: " + ScoreFromAngles(currentRotation, (float)targetRotation).ToString() + "\n"
                + "world frame score: " + ScoreFromAngles(currentRotation, 360f - (float)targetRotation) + "\n"
                + "visual frame error: " + SignedAngleError((float)targetRotation, currentRotation) + "\n"
                + "world frame error: " + SignedAngleError(360f - (float)targetRotation, currentRotation);

        }
        */

        // increment quit timer or reset
        /*
        if (Input.GetKey("escape"))
        {
            EscTime += Time.deltaTime;
        } else
        {
            EscTime = 0f;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ViveSR.anipal.Eye.SRanipal_Eye.LaunchEyeCalibration();
        }

        // quit condition
        if (EscTime >= 1f)
        {
            Application.Quit();
        }
        */



        if (stage.Equals("turn"))
        {

            //tm.text = "pre-turn/turnback check Update";
            if (remainingOrbit > 0f)
            {
                float orbitstep = 0f;
                remainingOrbit -= degPerStep;
                //tm.text = "remaining";
                if (remainingOrbit >= 90f)
                {
                    orbitstep = (3f/turnTime)*turnSign * ((180f - remainingOrbit) / 67.5f);

                    if (remainingOrbit - (degPerStep) < 90f)
                    {
                        if (!encodingShown)
                        {
                            timerFinished();
                            encodingShown = true;
                        }
                    }

                }
                else
                {
                    orbitstep = (3f/turnTime) * turnSign * (remainingOrbit / 67.5f);
                }

                currentOrbit += orbitstep;

                newXY = calculateOrbit(currentOrbit, fixationDepth, new Vector2(0f, 0f));
                fixationtarget.transform.position = new Vector3(newXY.x, fixationtarget.transform.position.y, newXY.y);
            }
            else
            {
                remainingOrbit = 0f;
                //tm.text = "not remaining";
            }
            //tm.text = "post-turn/turnback check Update";
        }
        //angles test
        //Quaternion changeInRotation = Quaternion.FromToRotation(Vector3.forward, gazeDirectionCombined);
        //Vector3 euler = changeInRotation.eulerAngles;
        //float ax = Vector3.SignedAngle(Vector3.forward, gazeDirectionCombined, Vector3.up);

        //tm.text = "X-angle: " + ax + "Y-angle: ";


        //time = (float)Time.realtimeSinceStartup;
        //register trigger press
        foreach (var ctrl in controllerList)
        {
            if (ctrl.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger,
                                    out triggerValue)
                                    && triggerValue > 0.1f)
            {
                if (!pressed)
                {
                    //if (!(trialType.Equals("reach") && stage.Equals("answer"))) //trialType.Equals("reach") && stage.eq..

                    if (!(stage.Equals("jitter") || stage.Equals("turn") || stage.Equals("answer") || stage.Equals("feedback")))
                    {
                        primaryButtonDown();
                        pressed = true;
                        break;
                    }


                }

            }
            else
            {
                if (pressed)
                {
                    pressed = false;

                }
            }

        }


        // tracking data update
        if (stage.Equals("jitter") ||
            stage.Equals("turn") ||
            stage.Equals("answer") ||
            stage.Equals("feedback"))
        {
            screenPosFixation.x = -1;
            screenPosFixation.y = -1;
            screenPosGaze.x = -1;
            screenPosGaze.y = -1;
            screenPosCube.x = -1;
            screenPosCube.y = -1;
            //screenPosBar.x = -1;
            //screenPosBar.y = -1;
            screenPosSphere.x = -1;
            screenPosSphere.y = -1;
            //screenPosCylinder.x = -1;
            //screenPosCylinder.y = -1;
            //screenPosTriangle.x = -1;
            //screenPosTriangle.y = -1;
            screenPosDiamond.x = -1;
            screenPosDiamond.y = -1;
            screenPosStar.x = -1;
            screenPosStar.y = -1;

            //turn obj in turn or turnback stage




            //ViveSR.anipal.Eye.SRanipal_Eye.GetEyeOpenness(ViveSR.anipal.Eye.EyeIndex.LEFT, out eye_openness_L);
            //ViveSR.anipal.Eye.SRanipal_Eye.GetEyeOpenness(ViveSR.anipal.Eye.EyeIndex.RIGHT, out eye_openness_R);
            //ViveSR.anipal.Eye.SRanipal_Eye.GetPupilPosition(ViveSR.anipal.Eye.EyeIndex.LEFT, out position_L);
            //ViveSR.anipal.Eye.SRanipal_Eye.GetPupilPosition(ViveSR.anipal.Eye.EyeIndex.RIGHT, out position_R);
            //ViveSR.anipal.Eye.SRanipal_Eye.Focus(ViveSR.anipal.Eye.GazeIndex.LEFT, out ray_L, out info_L);
            //ViveSR.anipal.Eye.SRanipal_Eye.Focus(ViveSR.anipal.Eye.GazeIndex.RIGHT, out ray_R, out info_R);
            //ViveSR.anipal.Eye.SRanipal_Eye.Focus(ViveSR.anipal.Eye.GazeIndex.COMBINE, out ray_C, out info_C);
            eye_openness_L = eyeData.verbose_data.left.eye_openness;
            eye_openness_R = eyeData.verbose_data.right.eye_openness;
            position_L = eyeData.verbose_data.left.pupil_position_in_sensor_area;
            position_R = eyeData.verbose_data.right.pupil_position_in_sensor_area;
            //info_L = eyeData.verbose_data.left.focus_info;
            //info_R = eyeData.verbose_data.right.focus_info;
            //info_C = eyeData.verbose_data.combined.focus_info;

            pupil_dia_L = eyeData.verbose_data.left.pupil_diameter_mm;
            pupil_dia_R = eyeData.verbose_data.right.pupil_diameter_mm;
            pupil_sensor_L = eyeData.verbose_data.left.pupil_position_in_sensor_area;
            pupil_sensor_R = eyeData.verbose_data.right.pupil_position_in_sensor_area;
            cameraPos = Camera.main.transform.position;
            cameraRot = Camera.main.transform.forward;
            focusNormal = info_C.normal;
            focusDistance = info_C.distance;
            focusPoint = info_C.point;
            eye_openness_L = eyeData.verbose_data.left.eye_openness;
            eye_openness_R = eyeData.verbose_data.right.eye_openness;
            gaze_angle_x = GetVectorAngle(gazeDirectionCombined).x;
            gaze_angle_y = GetVectorAngle(gazeDirectionCombined).y;
            head_angle_x = GetVectorAngle(Camera.main.transform.forward).x; //Vector3.SignedAngle(Vector3.forward, Camera.main.transform.forward, Vector3.up);
            head_angle_y = GetVectorAngle(Camera.main.transform.forward).y; //Vector3.SignedAngle(Vector3.forward, Camera.main.transform.forward, Vector3.right);


            //screen position
            //screenPosCube = Camera.WorldToViewportPoint();

            focusTag = "untagged";
            if (info_C.transform)
            {
                if (info_C.transform.tag != null)
                {
                    if (info_C.transform.tag == "Untagged")
                    {
                        focusTag = "none";
                    }
                    else
                    {
                        focusTag = info_C.transform.tag;
                    }
                }
            }

            //gaze
            //tm.text = "gaze";
            screenPosGaze = Camera.main.WorldToViewportPoint(focusPoint);

            //fixation cross
            //tm.text = "fixationcross";
            screenPosFixation = Camera.main.WorldToViewportPoint(fixationtarget.GetComponent<Transform>().position);

            //objects
            if (stage.Equals("presentation"))
            {
                if (presCube != null)
                {
                    //tm.text = "cube";
                    screenPosCube = Camera.main.WorldToViewportPoint(presCube.GetComponent<Transform>().position);
                }

                if (presSphere != null)
                {
                    //tm.text = "sphere";
                    screenPosSphere = Camera.main.WorldToViewportPoint(presSphere.GetComponent<Transform>().position);
                }
                /*
                if (presCylinder != null) {
                    //tm.text = "cylinder";
                    screenPosCylinder = Camera.main.WorldToViewportPoint(presCylinder.GetComponent<Transform>().position);
                }
                
                if (presTriangle != null)
                {
                    //tm.text = "triangle";
                    screenPosTriangle = Camera.main.WorldToViewportPoint(presTriangle.GetComponent<Transform>().position);
                }
                */
                if (presDiamond != null)
                {
                    //tm.text = "diamond";
                    screenPosDiamond = Camera.main.WorldToViewportPoint(presDiamond.GetComponent<Transform>().position);
                }

                if (presStar != null)
                {
                    //tm.text = "star";
                    screenPosStar = Camera.main.WorldToViewportPoint(presStar.GetComponent<Transform>().position);
                }
                /*
                if (presBar != null)
                {
                    //tm.text = "star";
                    screenPosBar = Camera.main.WorldToViewportPoint(presBar.GetComponent<Transform>().position);
                }
                */
            }

            //HANDTRACKING
            hand_position = GameObject.FindGameObjectsWithTag(useController)[0].GetComponent<Transform>().position;
            hand_direction = GameObject.FindGameObjectsWithTag(useController)[0].GetComponent<Transform>().forward;
            //hand_positionL = GameObject.FindGameObjectsWithTag("leftHand")[0].GetComponent<Transform>().position;
            //hand_directionL = GameObject.FindGameObjectsWithTag("leftHand")[0].GetComponent<Transform>().forward;

            //GAZE DEPTH
            origin_L = eyeData.verbose_data.left.gaze_origin_mm;
            direction_L = eyeData.verbose_data.left.gaze_direction_normalized;
            origin_R = eyeData.verbose_data.right.gaze_origin_mm;
            direction_R = eyeData.verbose_data.right.gaze_direction_normalized;

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

            ///*
            if (LineLineIntersection(out intersection, origin_L, direction_L, origin_R, direction_R))
            {
                fixationPoint.x = intersection.x;
                fixationPoint.y = intersection.y;
                fixationPoint.z = intersection.z;


                if (GameObject.FindGameObjectsWithTag("fixationvisual").Length > 0)
                {
                    //tm.text = "object";
                    GameObject.FindGameObjectsWithTag("fixationvisual")[0].GetComponent<Transform>().position = intersection;
                }

            }



            //variables to temp structures
            gaze_dir.Add(gazeDirectionCombined);
            head_pos.Add(cameraPos);
            head_dir.Add(cameraRot);
            eye_openness_Ls.Add(eye_openness_L);
            eye_openness_Rs.Add(eye_openness_R);
            pupil_dia_Ls.Add(pupil_dia_L);
            pupil_dia_Rs.Add(pupil_dia_R);
            pupil_sensor_Ls.Add(pupil_sensor_L);
            pupil_sensor_Rs.Add(pupil_sensor_R);
            position_Ls.Add(position_L);
            position_Rs.Add(position_R);
            ray_Ls.Add(ray_L);
            ray_Rs.Add(ray_R);
            ray_Cs.Add(ray_C);
            gaze_pos.Add(ray_C.origin);
            info_Ls.Add(info_L);
            info_Rs.Add(info_R);
            info_Cs.Add(info_C);
            focusNormals.Add(focusNormal);
            focusDistances.Add(focusDistance);
            focusPoints.Add(focusPoint);
            triggerCodes.Add(triggerCode);
            focusTags.Add(focusTag);
            currentTimesMs.Add(currentTimeMs);
            currentBlocks.Add(currentBlock);
            currentTrials.Add(currentTrial);
            ticks.Add(currentTick);
            gaze_angles_x.Add(gaze_angle_x);
            gaze_angles_y.Add(gaze_angle_y);
            head_angles_x.Add(head_angle_x);
            head_angles_y.Add(head_angle_y);

            viewpointsGaze.Add(screenPosGaze);
            viewpointsFix.Add(screenPosFixation);
            viewpointsCube.Add(screenPosCube);
            viewpointsSphere.Add(screenPosSphere);
            //viewpointsCylinder.Add(screenPosCylinder);
            //viewpointsTriangle.Add(screenPosTriangle);
            viewpointsDiamond.Add(screenPosDiamond);
            viewpointsStar.Add(screenPosStar);
            hand_pos.Add(hand_position);
            hand_dir.Add(hand_direction);
            //hand_posL.Add(hand_positionL);
            //hand_dirL.Add(hand_directionL);
            fixationPoints.Add(fixationPoint);
            //visualMatchesHand.Add(visualMatchHand);
            //worldMatchesHand.Add(worldMatchHand);

        }

        triggerCode = 0;
        currentTick += 1;

        if (stage.Equals("turn"))
        {
            bool encVis = sample_shapes[0].GetComponent<Renderer>().enabled;
            if (encVis && removeAttempts<2)
            {

                //hide encoding array
                int extraNum = removeAttempts * 50;
                addition_num = 400;
                triggerCode = addition_num + conditionCode + facingAdd + extraNum;

                removeAttempts += 1;
                present = false;
                foreach (GameObject oldObject in sample_shapes)
                {
                    //Destroy(oldObject);
                    //oldObject.GetComponent<Renderer>().enabled = false;
                    Shapescript shapescr2 = oldObject.GetComponent<Shapescript>();
                    shapescr2.setVisible(false);
                    Collider[] colListObj = oldObject.GetComponentsInChildren<Collider>();
                    //Collider[] colListHand = GameObject.FindGameObjectsWithTag("rightHand")[0].GetComponentsInChildren<Collider>();
                    foreach (Collider colobj in colListObj)
                    {
                        colobj.enabled = false;
                    }
                }
            }
        }

        // pointer ray
        if (stage.Equals("answer"))
        {
            if (raySet.Equals("none"))
            {
                raySet = "ans";
                GameObject.FindGameObjectsWithTag("rightHand")[0].GetComponent<XRInteractorLineVisual>().lineLength = 100;
                GameObject.FindGameObjectsWithTag("rightHand")[0].GetComponent<XRInteractorLineVisual>().lineWidth = 0.02f;
                //tm.text = timeObj.GetComponent<Timer>().getTime().ToString();
            }
        }
        else if (raySet.Equals("ans"))
        {
            raySet = "none";
            GameObject.FindGameObjectsWithTag("rightHand")[0].GetComponent<XRInteractorLineVisual>().lineLength = 0;
            GameObject.FindGameObjectsWithTag("rightHand")[0].GetComponent<XRInteractorLineVisual>().lineWidth = 0.0f;
        }



        //tm.text = hand_position.ToString();
        //overwrite left and right ray
        //ViveSR.anipal.Eye.SRanipal_Eye.GetGazeRay(ViveSR.anipal.Eye.GazeIndex.LEFT, out ray_L);
        //ViveSR.anipal.Eye.SRanipal_Eye.GetGazeRay(ViveSR.anipal.Eye.GazeIndex.RIGHT, out ray_R);



        //tm.text = origin_L.ToString() + origin_R.ToString();
        //*/

        //GameObject.FindGameObjectsWithTag("rightHand")[0].GetComponent<XRInteractorLineVisual>().lineWidth = 0.0f;

        if (textFollowView && tm)
        {
            tm.transform.position = Camera.main.transform.position + Vector3.Scale(Camera.main.transform.forward, viewTextDistVec);
            tm.transform.rotation = Camera.main.transform.rotation;
        }

    }

    public void timerFinished()
    {
        switch (stage)
        {

            case "preparation":

                front *= -1;
                //GameObject.FindGameObjectsWithTag("rightHand")[0].GetComponent<LineRenderer>().enabled = false;
                remainingOrbit = 180f;
                responseLoc = 0;
                responseLocProbe = 0;
                targetLocProbe = 0;
                targetLoc = 0;
                targetCol = 0;
                responseShape = 0;
                correct = false;
                ansCount = 0;
                present = false;
                encodingShown = false;
                removeAttempts = 0;

                GameObject.FindGameObjectsWithTag("rightHand")[0].GetComponent<XRInteractorLineVisual>().lineWidth = 0.0f;

                if (facing.Equals("front"))
                {
                    facing = "back";
                    facingAdd = 0;
                    tm.transform.position = new Vector3(0f, 1.5f, -5f);
                    tm.transform.eulerAngles = new Vector3(0f, 180f, 0f);
                }
                else
                {
                    facing = "front";
                    facingAdd = 2;
                    tm.transform.position = new Vector3(0f, 1.5f, 5f);
                    tm.transform.eulerAngles = new Vector3(0f, 0f, 0f);
                }

                if (facing.Equals("front"))
                {
                    facingAdd = 2;
                }

                //tm.fontSize = 35;

                if (practice)
                {
                    practiceTrial += 1;
                }
                else
                {
                    if (currentTrial < trials)
                    {
                        currentTrial += 1;
                    }
                    else
                    {
                        currentTrial = 1;
                        //currentBlock += 1;
                    }

                }

                conditionName = getTrialCondition();
                //conditionName = "d6";
                // TO-DO
                currentNumCondition = 4;

                conditionCode = conditionCodes[conditionName];

                currentPositions = positions4;
                //tm.transform.position = new Vector3(0f, heightOffset.y, -2f);
                tm.text = ""; 


                //turn direction condition

                if (conditionName.Contains("r"))
                {
                    currentConditionTurn = "right";
                }
                else if (conditionName.Contains("l"))
                {
                    currentConditionTurn = "left";
                }


                currentProbePositions = positions_probes;

                //decide object number factor
                //float objNumOdds = UnityEngine.Random.Range(0f, 100f);


                sample_shapes = new GameObject[currentNumCondition]; // presentation objects
                sample_colors = new Color[currentNumCondition]; //presentation colors
                sample_probes = new GameObject[currentProbePositions.Length]; //probe objects
                probes_colors = new Color[currentProbePositions.Length]; // probe colors


                shapeOrder = shapeOrder.OrderBy(x => UnityEngine.Random.value).ToList();
                colOrder = colOrder.OrderBy(x => UnityEngine.Random.value).ToList();
                encShapeOrder = encShapeOrder.OrderBy(x => UnityEngine.Random.value).ToList();
                encColOrder = encColOrder.OrderBy(x => UnityEngine.Random.value).ToList();

                //
                // NEW PREPARATION CODE
                //generate and present 2 random shapes
                //Array.Clear(ConfirmationArray, 0, ConfirmationArray.Length);
                Array.Clear(presentationInfo, 0, presentationInfo.Length);

                //pre-determine random shape and color of presentation
                shapeSeq = encShapeOrder;
                colSeq = encColOrder;

                //
                //

                clearFixation();
                //tm.text = conditionName;
                stage = "jitter";
                addition_num = 100;
                triggerCode = addition_num + conditionCode + facingAdd;
                timeObj.GetComponent<Timer>().setTime(jitterTimeMax); //timeObj.GetComponent<Timer>().setTime(UnityEngine.Random.Range(jitterTimeMin, jitterTimeMax));
                timeObj.GetComponent<Timer>().setRunning(true);
                break;

            case "jitter":

                stage = "turn";
                addition_num = 200;
                triggerCode = addition_num + conditionCode + facingAdd;

                bool turnLeft = (UnityEngine.Random.Range(0f, 100f) <= 50f);
                if (conditionName.Contains("l"))
                {
                    turnLeft = true;
                    turnSign = 1;
                }
                else
                {
                    turnLeft = false;
                    turnSign = -1;
                }

                timeObj.GetComponent<Timer>().setTime(turnTime);
                timeObj.GetComponent<Timer>().setRunning(true);

                //shapescrv.setVisible(false);
                //shapescrw.setVisible(false);

                //presentationTimeStamp = oldTime
                /*
                    foreach (GameObject oldObject in sample_shapes)
                    {
                        //Destroy(oldObject);
                        //oldObject.GetComponent<Renderer>().enabled = false;
                        Shapescript shapescr2 = oldObject.GetComponent<Shapescript>();
                        shapescr2.setVisible(false);
                        Collider[] colListObj = oldObject.GetComponentsInChildren<Collider>();
                        //Collider[] colListHand = GameObject.FindGameObjectsWithTag("rightHand")[0].GetComponentsInChildren<Collider>();

                        foreach (Collider colobj in colListObj)
                        {
                            colobj.enabled = false;
                        }
                    }
                */

                break;


            case "turn":

                bool halfway = timeObj.GetComponent<Timer>().getRunning();

                if (halfway)
                {
                    //PRESENTATION
                    present = true;
                    //reset view infos
                    presCube = null;
                    presSphere = null;
                    //presCylinder = null;
                    //presTriangle = null;
                    presDiamond = null;
                    presStar = null;


                    addition_num = 300;
                    
                    triggerCode = addition_num + conditionCode + facingAdd;
                    //timeObj.GetComponent<Timer>().setTime(presentationTime);
                    //timeObj.GetComponent<Timer>().setRunning(true);


                    xMulVec = new Vector3(turnSign, 1f, 1f);

                    for (int i = 0; i < currentNumCondition; i++)
                    {

                        ColorRandomNumber = encColOrder[i];
                        ShapeRandomNumber = encShapeOrder[i];

                        //tm.color = cyan;
                        sample_colors[i] = colors[ColorRandomNumber];
                                                

                        sample_shapes[i] = Instantiate(shapes[ShapeRandomNumber], Vector3.Scale(currentPositions[i], xMulVec) * front + heightOffset + encodingVerticalDeviation, Quaternion.identity); //shapes[ShapeRandomNumber].transform.rotation

                        Shapescript shapescr = sample_shapes[i].GetComponent<Shapescript>();
                        shapescr.setColour(sample_colors[i]);
                        shapescr.spawnPosition = currentPositions[i] + heightOffset + relHeight + encodingVerticalDeviation;
                        shapescr.spawnPositionCode = i + 1;
                        shapescr.shapeCode = ShapeRandomNumber + 1;
                        shapescr.colorCode = ColorRandomNumber + 1;


                        //tm.text = (currentPositions[circle_inds[i]] + heightOffset + relHeight).ToString();

                        //tm.text = "switch";
                        switch (sample_shapes[i].tag)
                        {
                            case "cube":
                                //tm.text = "case cube";
                                presCube = sample_shapes[i];
                                break;

                            case "sphere":
                                //tm.text = "case sphere";
                                presSphere = sample_shapes[i];
                                break;
                            /*
                            case "cylinder":
                                //tm.text = "case cylinder";
                                presCylinder = sample_shapes[i];
                                break;
                    
                            case "triangle":
                                //tm.text = "case triangle";
                                presTriangle = sample_shapes[i];
                                break;
                        */
                            case "diamond":
                                //tm.text = "case diamond";
                                presDiamond = sample_shapes[i];
                                break;

                            case "star":
                                //tm.text = "case star";
                                presStar = sample_shapes[i];
                                break;
                            /*
                                case "bar":
                                    //tm.text = "case star";
                                    presBar = sample_shapes[i];
                                    break;
                            */
                            default:
                                break;
                        }


                        //scale
                        //sample_shapes[i].transform.localScale /= 2;

                        //ConfirmationArray[ColorRandomNumber] = true; // So it will not be picked again
                        presentationInfo[i, 0] = ColorRandomNumber;
                        presentationInfo[i, 1] = ShapeRandomNumber;

                        //selection appearance change
                        //var trans = 0.5f;
                        //var col = sample_shapes[i].GetComponent<Renderer>().material.color;
                        //col.a = trans;


                    }

                    Camera.main.Render();

                }
                else
                {
                    // TURN END
                    //tm.text = "a";
                    oldTime = watch.ElapsedMilliseconds;
                    remainingOrbit = 180f;
                    stage = "answer";
                    addition_num = 500;
                    triggerCode = addition_num + conditionCode + facingAdd;

                    if (!fixationtarget.transform.position.x.Equals(0))
                    {
                        fixationtarget.transform.position = new Vector3(0f, heightOffset.y, fixationDepth*front*-1);
                        //tm.text = "b";
                    }
                    //tm.text = "c";
                    for (int i = 0; i < sample_shapes.Length; i++)
                    {
                        encLocInfo[i, 0] = 0; //correct
                        encLocInfo[i, 1] = 0; //rank
                        encLocInfo[i, 2] = 0; //shape
                        encLocInfo[i, 3] = 0; //col
                        encLocInfo[i, 4] = 0; //probeloc
                        encLocInfo[i, 5] = 0; //respTime
                        //tm.text = "d";

                    }

                    for (int i = 0; i < respCount; i++)
                    {
                        respInfo[i, 0] = 0; //correct
                        respInfo[i, 1] = 0; //encloc
                        respInfo[i, 2] = 0; //shape
                        respInfo[i, 3] = 0; //col
                        respInfo[i, 4] = 0; //loc
                        respInfo[i, 5] = 0; //respTime
                        respInfo[i, 6] = 0; //encColLoc
                        respInfo[i, 7] = 0; //encShapeLoc
                        //tm.text = "e";
                    }

                    for (int i = 0; i < colors.Length; i++)
                    {
                        colInfo[i, 0] = 0; //correct
                        colInfo[i, 1] = 0; //encloc
                        colInfo[i, 2] = 0; //encshape
                        colInfo[i, 3] = 0; //probeloc
                        colInfo[i, 4] = 0; //probeshape
                        colInfo[i, 5] = 0; //respRank
                        colInfo[i, 6] = 0; //respTime
                        //tm.text = "f";
                    }

                    for (int i = 0; i < shapes.Length; i++)
                    {
                        shapeInfo[i, 0] = 0; //correct
                        shapeInfo[i, 1] = 0; //encloc
                        shapeInfo[i, 2] = 0; //enccol
                        shapeInfo[i, 3] = 0; //probeloc
                        shapeInfo[i, 4] = 0; //probecol
                        shapeInfo[i, 5] = 0; //respRank
                        shapeInfo[i, 6] = 0; //respTime
                        //tm.text = "g";
                    }
                    //tm.text = "h";
                    clearFixation();
                    //tm.text = "i";

                    //probes
                    for (int i = 0; i < probeCount / 4; i++)
                    {
                        for (int j = 0; j < probeCount / 4; j++)
                        {
                            ShapeRandomNumber = shapeOrder[i];
                            ColorRandomNumber = colOrder[j];

                            //tm.color = cyan;
                            int probeIndex = (i * 4) + j;
                            probes_colors[probeIndex] = colors[ColorRandomNumber];

                            sample_probes[probeIndex] = Instantiate(shapes[ShapeRandomNumber], currentProbePositions[probeIndex] * front *-1 + heightOffset, Quaternion.identity); //shapes[ShapeRandomNumber].transform.rotation

                            Shapescript shapescr = sample_probes[probeIndex].GetComponent<Shapescript>();
                            shapescr.setColour(probes_colors[probeIndex]);
                            shapescr.spawnPosition = currentProbePositions[probeIndex] + heightOffset + relHeight;
                            shapescr.spawnPositionCode = probeIndex + 1;
                            shapescr.shapeCode = ShapeRandomNumber + 1;
                            shapescr.colorCode = ColorRandomNumber + 1;

                        }
                        //tm.text = "j";


                        //tm.text = (currentPositions[circle_inds[i]] + heightOffset + relHeight).ToString();


                        //scale
                        //sample_shapes[i].transform.localScale /= 2;

                        //ConfirmationArray[ColorRandomNumber] = true; // So it will not be picked again
                        //presentationInfo[i,0] = ColorRandomNumber;
                        //presentationInfo[i, 1] = rotations[i];

                        //selection appearance change
                        //var trans = 0.5f;
                        //var col = sample_shapes[i].GetComponent<Renderer>().material.color;
                        //col.a = trans;
                    }
                    //tm.text = "k";
                }

                break;

            case "answer":
                //tm.text = "answer end";
                stage = "feedback";
                //tm.text = "l";
                addition_num = 1000;
                triggerCode = addition_num + conditionCode + facingAdd;
                tm.text = "before score computing";
                correctness = encLocInfo[0, 0] + encLocInfo[1, 0] + encLocInfo[2, 0] + encLocInfo[3, 0];
                tm.text = "after score computing";
                feedbackTime = 1f;

                setTextLoc("near");
                tm.color = defaultTxtColour;


                switch (feedback)
                {
                    case "rt":
                        //tm.text = Convert.ToInt32(responseTime).ToString();
                        //tm.text = scoreTouch.ToString();
                        break;
                    case "acc":
                        tm.text = correctness.ToString();
                        break;
                    case "err":
                        //tm.text = errorTouch.ToString();
                        break;
                    default:
                        tm.text = "";
                        break;
                }



                //tm.text = respLoc + 


                timeObj.GetComponent<Timer>().setTime(feedbackTime);
                timeObj.GetComponent<Timer>().setRunning(true);
                break;

            case "feedback":

                tm.text = "";
                tm.color = defaultTxtColour;
                setTextLoc("far");


                GameObject.FindGameObjectsWithTag("rightHand")[0].GetComponent<XRInteractorLineVisual>().lineWidth = 0.0f;
                //tm.text = "1";
                absoluteTrial = getAbsoluteTrial(); // 1-500
                //tm.fontSize = 35;
                //tm.text = "WriteTrialData";
                //tm.text = "2";

                if (currentTrial >= trials)
                {
                    //tm.transform.position = new Vector3(tm.transform.position.x, heightOffset.y, 5f);
                    tm.text = "Saving...";
                }
                //tm.text = "3";
                tm.text = "before trial data write";
                WriteTrialData();
                tm.text = "after trial data write";
                //tm.text = "4";
                if (practice)
                {
                    if (practiceTrial.Equals(conditions.Length))
                    {
                        //tm.transform.position = new Vector3(tm.transform.position.x, heightOffset.y, 5f);
                        tm.text = "Saving...";
                        WriteTimeData();
                        ClearTempData();
                        practice = false;
                        stage = "none";
                        //GameObject.FindGameObjectsWithTag("rightHand")[0].GetComponent<LineRenderer>().enabled = false;
                        tm.text = "Practice finished. Press to continue to calibration.";
                        //timeObj.GetComponent<Timer>().setTime(preparationTime);
                        //timeObj.GetComponent<Timer>().setRunning(true);
                    }
                    else
                    {
                        // next trial
                        stage = "preparation";
                        timeObj.GetComponent<Timer>().setTime(preparationTime);
                        timeObj.GetComponent<Timer>().setRunning(true);
                    }
                }
                else
                {
                    //tm.text = "5";
                    if (currentTrial < trials)
                    {
                        //tm.text = "6";
                        // next trial
                        stage = "preparation";
                        timeObj.GetComponent<Timer>().setTime(preparationTime);
                        timeObj.GetComponent<Timer>().setRunning(true);
                    }
                    else
                    {
                        // save block data
                        //new WaitForSecondsRealtime(1);
                        //tm.transform.position = new Vector3(tm.transform.position.x, heightOffset.y, 5f);
                        tm.text = "Score: ";
                        //new WaitForSecondsRealtime(1);

                        WriteTimeData();
                        ClearTempData();


                        //currentTrial = 0;
                        if (currentBlock >= blocks)
                        {
                            // run finished
                            tm.text = "Run Complete!";
                            stage = "completed";
                        }
                        else
                        {
                            // break
                            currentBlock += 1;
                            stage = "none";
                            //GameObject.FindGameObjectsWithTag("rightHand")[0].GetComponent<LineRenderer>().enabled = false;

                            //GameObject.FindGameObjectsWithTag("rightHand")[0].GetComponent<>().enabled = false;
                            //tm.text = "break";
                            tm.text = "Time for a break? Or press trigger to calibrate.";
                            timeObj.GetComponent<Timer>().setRunning(false);
                        }

                    }
                    //tm.color = red;
                }
                //tm.text = "8";
                //clean up trial
                answered = false;
                highlightedObject = null;
                currentSelectedObject = null;

                //tm.text = "+";
                clearFixation();
                //tm.text = "9";
                //tm.text = "+";
                //trialCount += 1;
                //blockCount = trialCount/trials;

                //destroy the 4 random shapes
                foreach (GameObject oldObject in sample_shapes)
                {
                    Destroy(oldObject);
                    //oldObject.GetComponent<Renderer>().enabled = false;
                }
                foreach (GameObject oldProbe in sample_probes)
                {
                    Destroy(oldProbe);
                    //oldObject.GetComponent<Renderer>().enabled = false;
                }
                //Destroy(visualitem);
                //Destroy(worlditem);
                //tm.text = "10";
                //destroy probes

                Array.Clear(sample_shapes, 0, sample_shapes.Length);
                Array.Clear(sample_colors, 0, sample_colors.Length);
                //tm.text = "11";
                //Array.Clear(sample_probes, 0, sample_probes.Length);
                //Array.Clear(probes_colors, 0, probes_colors.Length);
                //Array.Clear(ConfirmationArray, 0, ConfirmationArray.Length);
                //Array.Clear(ProbeConfirmationArray, 0, ProbeConfirmationArray.Length);
                Array.Clear(presentationInfo, 0, presentationInfo.Length);

                break;

            case "calibrationn":

                if (testing)
                {
                    /*
                    addition_num = 1000;
                    //fixationtarget.transform.position = locSequence[LocalisationCounter] + heightOffset;
                    triggerCode = addition_num + localisationPosToInt(locSequence[LocalisationCounter]);
                    LocalisationCounter += 1;

                    timeObj.GetComponent<Timer>().setTime(2f);
                    timeObj.GetComponent<Timer>().setRunning(true);
                    */
                }
                else
                {

                    //finally
                    //fixationtarget.transform.position = new Vector3(0f, 0f, fixationDepth) + heightOffset;
                    string block_type;
                    if (practice)
                    {
                        block_type = "practice trials";
                    }
                    else
                    {
                        block_type = "block " + currentBlock.ToString() + "/" + blocks.ToString();
                    }
                    //tm.transform.position = new Vector3(tm.transform.position.x, heightOffset.y, 5f);
                    calibrated = true;
                    tm.text = "Press trigger to start " + block_type + ".";
                }
                break;

            case "none":
                // just in case (no pun intended)
                stage = "preparation";
                timeObj.GetComponent<Timer>().setTime(preparationTime);
                timeObj.GetComponent<Timer>().setRunning(true);
                // tm.fontSize = 35;
                tm.text = "";
                //tm.color = magenta;
                break;
            default:
                // probably never fires
                break;
        }

        //tm.text = stage;
    }


    public void primaryButtonDown()
    {
        //pressCount += 1;
        //tm.text = pressCount.ToString();
        switch (stage)
        {
            case "none":
                //tm.text = "pressed";
                if (false)
                {
                    if (testinstance == null)
                    {

                        //tm.text = "spawn";
                        Vector3 spawn = new Vector3(0f, 1f, 0f);
                        testinstance = Instantiate(star, spawn, Quaternion.identity);
                        //Shapescript sc = testinstance.GetComponent<Shapescript>();
                        //sc.initProbe();
                        //testinstance.GetComponent<Shapescript>().scaleUp();
                    }
                    else
                    {

                        //scale up
                        //testinstance.GetComponent<Shapescript>().scaleUp();

                        //break it

                        if (testinstance.GetComponent<Shapescript>().exploded)
                        {
                            //tm.text = "found";
                            Destroy(testinstance);
                            Vector3 spawn = new Vector3(0f, 1f, 0f);
                            testinstance = Instantiate(star, spawn, Quaternion.identity);
                        }
                        else
                        {
                            //testinstance.GetComponent<Shapescript>().explode();
                            testinstance.AddComponent<TriangleExplosion>();
                            StartCoroutine(testinstance.GetComponent<TriangleExplosion>().SplitMesh(false));
                        }
                        //break it
                    }
                }
                else
                {
                    /*
                    stage = "preparation";
                    //tm.text = "???";
                    timeObj.GetComponent<Timer>().setTime(preparationTime);
                    timeObj.GetComponent<Timer>().setRunning(true);
                    */
                    stage = "calibration";
                    addition_num = 2000;

                    calibrated = false;
                    calibrationStarted = false;
                    //height

                    if (!calibratedHeight)
                    {
                        calibratedHeight = true;
                        ResetHeight();

                    }


                    //calibrate position
                    //subSys.TryRecenter();

                    //calibrate eye
                     ViveSR.anipal.Eye.SRanipal_Eye.LaunchEyeCalibration();

                    string block_type = "block " + currentBlock.ToString() + "/" + blocks.ToString();

                    //tm.transform.position = new Vector3(tm.transform.position.x, heightOffset.y, 5f);

                    tm.text = "Press trigger to start " + block_type + ".";
                    calibrated = true;


                }
                break;

            case "calibration":
                if (calibrated)
                {
                    stage = "preparation";
                    addition_num = 0;
                    //tm.text = "???";
                    timeObj.GetComponent<Timer>().setTime(preparationTime);
                    timeObj.GetComponent<Timer>().setRunning(true);
                }

                break;


            case "answer":
                //tm.text = "answered";
                /*
                if (answered)
                {
                    break;
                }
                */
                //tm.text = "m";
                newTime = watch.ElapsedMilliseconds;
                responseTime = newTime - oldTime;
                oldTime = newTime;
                addition_num = 600;
                triggerCode = addition_num + 100 * ansCount + conditionCode + facingAdd;

                ansCount += 1; //rank

                tm.text = "";
                setTextLoc("far");
                //hideRespObj();
                //tm.text = "triggerCode set";
                //currentBlock
                //currentTrial
                //conditionCode; // 1-6
                //responseTime
                //correct // 0|1

                // hide fixation cross and reset position
                //tm.text = "";

                //answered = true;
                //tm.text = "currentSelectedObject";
                currentSelectedObject = highlightedObject;
                //tm.text = "set to null";
                correct = false;
                responseInstance = null;
                targetInstance = null;
                responseProbe = null;
                targetProbe = null;

                //tm.text += targetLoc;
                //tm.text = "targetLocProbe";
                //targetLocProbe = targetProbe.GetComponent<Shapescript>().probePositionCode;
                //tm.text += targetLocProbe;
                //tm.text = "responseShape";
                responseShape = currentSelectedObject.GetComponent<Shapescript>().shapeCode; // resp shape
                //tm.text += responseShape;
                //tm.text = "responseLocProbe";
                responseLocProbe = currentSelectedObject.GetComponent<Shapescript>().spawnPositionCode; //probe loc
                //tm.text += responseLocProbe;

                responseInstance = currentSelectedObject;

                //tm.text = "existed";
                responseColor = currentSelectedObject.GetComponent<Shapescript>().colorCode; //resp color
                //responseLoc = responseInstance.GetComponent<Shapescript>().spawnPositionCode;


                //tm.text = "responseTime";
                //responseTime = ((float)Time.realtimeSinceStartup - presentationTimeStamp) * 1000f;

                //currentSelectedObject.GetComponent<Transform>().position = GameObject.FindGameObjectsWithTag(useController)[0].GetComponent<Transform>().position;
                //tm.text = "scores";


                for (int l = 0; l < sample_probes.Length; l++)
                {
                    //ShapeRandomNumber = shapeOrder[i];
                    //ColorRandomNumber = colOrder[i];

                    Shapescript prbScr = sample_probes[l].GetComponent<Shapescript>();
                    int probecol = prbScr.colorCode;
                    int probeshape = prbScr.shapeCode;

                    for (int m = 0; m < sample_shapes.Length; m++)
                    {
                        Shapescript spScr = sample_shapes[m].GetComponent<Shapescript>();
                        int enccol_ = spScr.colorCode;
                        int encshape_ = spScr.shapeCode;

                        if (probeshape.Equals(encshape_) && probecol.Equals(enccol_)) {
                            encLocInfo[m, 4] = l+1;//probeloc
                        }
                    }

                    //disable visible and collision of row/col
                    if (responseColor.Equals(probecol) || responseShape.Equals(probeshape))
                    {
                        prbScr.setVisible(false);
                        Collider[] colListObj = prbScr.GetComponentsInChildren<Collider>();
                        foreach (Collider colobj in colListObj)
                        {
                            colobj.enabled = false;
                        }

                    }
                    //tm.text = "n";

                }

                //respInfo and encLocInfo structuring
                //tm.text = "oo";
                //store response info
                int respInd = ansCount - 1;
                //respInfo[respInd, 0] = 1; // correct
                //respInfo[respInd, 1] = ansCount; // rank
                respInfo[respInd, 2] = responseShape;//shape
                respInfo[respInd, 3] = responseColor;//col
                respInfo[respInd, 4] = responseLocProbe;//probeloc
                respInfo[respInd, 5] = Convert.ToInt32(responseTime);//probeloc//rt

                colInfo[responseColor - 1, 3] = responseLocProbe; //probeloc of col
                shapeInfo[responseShape - 1, 3] = responseLocProbe; //probeloc of shape

                colInfo[responseColor - 1, 4] = responseShape; //probeshape of col
                shapeInfo[responseShape - 1, 4] = responseColor; //probecol of shape

                colInfo[responseColor - 1, 5] = ansCount; //rank of col
                shapeInfo[responseShape - 1, 5] = ansCount; //rank of shape

                colInfo[responseColor - 1, 6] = Convert.ToInt32(responseTime); //rank of col
                shapeInfo[responseShape - 1, 6] = Convert.ToInt32(responseTime); //rank of shape
                //tm.text = "p";
                //check if answer matched 
                for (int k = 0; k < sample_shapes.Length; k++)
                {
                    //tm.text = "q";
                    colEnc = encColOrder[k];
                    shapeEnc = encShapeOrder[k];

                    encLocInfo[k, 2] = shapeEnc + 1;//shape
                    encLocInfo[k, 3] = colEnc + 1;//col
                    

                    if (colEnc.Equals(responseColor - 1) && shapeEnc.Equals(responseShape - 1))
                    {
                        correct = true;
                        encLocInfo[k, 0] = 1; // correct
                        encLocInfo[k, 1] = ansCount; // rank
                        
                        encLocInfo[k, 5] = Convert.ToInt32(responseTime);//probeloc//rt

                        respInfo[respInd, 0] = 1; // correct
                        respInfo[respInd, 1] = k+1;//encloc

                        colInfo[responseColor-1, 0] = 1;
                        colInfo[responseColor - 1, 1] = k+1;
                        shapeInfo[responseShape - 1, 0] = 1;
                        shapeInfo[responseShape - 1, 1] = k+1;

                    }
                    //tm.text = "r";
                    if (colEnc.Equals(responseColor - 1))
                    {
                        respInfo[respInd, 6] = k+1;
                        colInfo[responseColor-1, 2] = shapeEnc+1; //encshape of color
                    }
                    //tm.text = "s";
                    if (shapeEnc.Equals(responseShape - 1))
                    {
                        respInfo[respInd, 7] = k+1;
                        shapeInfo[responseShape - 1, 2] = colEnc + 1; //encCol of shape
                    }
                    //tm.text = "t";

                }


                //remove items

                //tm.text = "correctness";
                //response evaluation

                //tm.text = correctness.ToString() + "%";
                //tm.text = "pre-setTime";
                // leave response stage

                selected = null;
                highlightedObject = null;
                currentSelectedObject = null;
                answered = false;
                closest = null;
                handUsed = "nan";
                //usedFrame = "nan";
                respHand = null;
                respLoc = 0;
                correctHand = "nan";
                correct = false;


                //tm.text = "post-setTime";
                //tm.text = "u";
                if (ansCount >= respCount)
                {
                    //tm.text = "v";
                    timeObj.GetComponent<Timer>().setTime(0f);
                    timeObj.GetComponent<Timer>().setRunning(true);
                }
                //tm.text = "post-setRunning";
                //tm.text = "w";
                break;

            default:
                /*
                if (paused)
                {
                    // resume
                    timeObj.GetComponent<Timer>().setRunning(true);
                    tm.text = "resumed";
                    paused = false;
                }
                else 
                {
                    // pause
                    timeObj.GetComponent<Timer>().setRunning(false);
                    tm.text = stage;
                    tm.color = cyan;
                    paused = true;
                }
                */
                break;
        }
    }

    public void primaryButtonUp()
    {
        //tm.text = "primary button up";
    }

    public void RhandHoverEntered()
    {
        //tm.text = "primary button up";
        respHand = rightHandObj;
    }

    public void LhandHoverEntered()
    {
        //tm.text = "primary button up";
        respHand = leftHandObj;
    }
    public void objectHoverEnter(GameObject obj_)
    {
        highlightedObject = obj_; //GameObject.FindGameObjectsWithTag(objname)[0];
        highlightedObject.GetComponent<Shapescript>().scaleUp();
        highlightedObject.GetComponent<Shapescript>().highlighted = true;
        //tm.text = highlightedObject.tag;

        foreach (GameObject obb in sample_probes)
        {
            if (!obb.tag.Equals(highlightedObject.tag))
            {
                obb.GetComponent<Shapescript>().scaleDown();
                obb.GetComponent<Shapescript>().highlighted = false;
            }
        }

    }

    public void objectHoverExited(GameObject go)
    {
        /*
        go.GetComponent<Shapescript>().scaleDown();
        //tm.text = "exited";
        if (go.tag.Equals(highlightedObject.tag))
        {
            highlightedObject = null;
        }
        */
    }

    public string getTrialCondition()
    {
        if (practice)
        {
            return conditions[practiceTrial - 1];
        }
        else
        {
            int abs_trial = getAbsoluteTrial();
            //tm.text = conditionSequence.Count.ToString();
            string rt = conditionSequence[abs_trial - 1];
            //tm.text = rt;
            return rt;
        }

    }

    public int getAbsoluteTrial()
    {
        //tm.text = "((" + currentBlock.ToString()+"-1" + " * " + trials.ToString() + " + " + currentTrial.ToString();
        int ab_t = ((currentBlock - 1) * trials) + currentTrial;
        return ab_t;
    }

    private string getTrialDataPath(bool timeformat)
    {
        if (timeformat)
        {
#if UNITY_EDITOR
            return Application.dataPath + "/TSV/" + timedataName + ".tsv";
#elif UNITY_ANDROID
            return Application.persistentDataPath+ timedataName + ".tsv";
#elif UNITY_IPHONE
            return Application.persistentDataPath+"/"+ timedataName + ".tsv";
#else
            return Application.dataPath + "/" + timedataName + ".tsv";
#endif
        }
        else
        {
#if UNITY_EDITOR
            return Application.dataPath + "/TSV/" +  trialdataName + ".tsv";
#elif UNITY_ANDROID
            return Application.persistentDataPath+ trialdataName + ".tsv";
#elif UNITY_IPHONE
            return Application.persistentDataPath+ trialdataName + ".tsv";
#else
            return Application.dataPath + "/" + trialdataName + ".tsv";
#endif
        }
    }

    private string getPracticeDataPath(bool timeformat)
    {
        if (timeformat)
        {
#if UNITY_EDITOR
            return Application.dataPath + "/TSV/" + timedataName + ".tsv";
#elif UNITY_ANDROID
            return Application.persistentDataPath+ timedataName + ".tsv";
#elif UNITY_IPHONE
            return Application.persistentDataPath+"/"+ timedataName + ".tsv";
#else
            return Application.dataPath + "/" + timedataName + ".tsv";
#endif
        }
        else
        {
#if UNITY_EDITOR
            return Application.dataPath + "/TSV/" +  trialdataName + ".tsv";
#elif UNITY_ANDROID
            return Application.persistentDataPath+ trialdataName + ".tsv";
#elif UNITY_IPHONE
            return Application.persistentDataPath+"/"+ trialdataName + ".tsv";
#else
            return Application.dataPath + "/" + trialdataName + ".tsv";
#endif
        }
    }

    private void WriteTimeData()
    {
        string filePath;

        if (practice)
        {
            filePath = getPracticeDataPath(true);
        }
        else
        {
            filePath = getTrialDataPath(true);
        }
        //tm.text = "filePath set";
        //writer writes to filepath


        //StreamWriter writer = new StreamWriter(filePath, true);


        //tm.text = "writer created";
        string header = "Tick\tTime\tTriggerCode\tBlock\tTrial\tGazeDirX\tGazeDirY\tGazeDirZ\tHeadPosX\tHeadPosY\tHeadPosZ\tHeadDirX\tHeadDirY\tHeadDirZ\tEyeOpenL\tEyeOpenR\tPupilDiaL\tPupilDiaR\tPupilSensorLx\tPupilSensorLy\tPupilSensorRx\tPupilSensorRy\tPositionLx\tPositionLy\tPositionRx\tPositionRy\tGazeAngleX\tGazeAngleY\tHeadAngleX\tHeadAngleY\tViewGazeX\tViewGazeY\tViewGazeZ\tViewFixX\tViewFixY\tViewFixZ\tHandRPosX\tHandRPosY\tHandRPosZ\tHandRDirX\tHandRDirY\tHandRDirZ\tFixationPointX\tFixationPointY\tFixationPointZ";


        if (!System.IO.File.Exists(filePath))
        {
            File.AppendAllText(filePath, header);
            File.AppendAllText(filePath, "\n");
            //tm.text = "wrote time header";
        }
        //File.AppendAllText(filePath, "Tick\tTriggerCode\tBlock\tTrial\tGazeDirX\tGazeDirY\tGazeDirZ\tGazePosX\tGazePosY\tGazePosZ\tHeadPosX\tHeadPosY\tHeadPosZ\tHeadDirX\tHeadDirY\tHeadDirZ\tEyeOpenL\tEyeOpenR\tPupilDiaL\tPupilDiaR\tFocusObject\tFocusNormalX\tFocusNormalY\tFocusNormalZ\tFocusDistance\tFocusPointX\tFocusPointY\tFocusPointZ");
        //writer.WriteLine ();


        //File.AppendAllText(filePath, currentBlock.ToString());
        //File.AppendAllText(filePath, "\n");
        //This loops through everything
        for (int iiii = 0; iiii < gaze_dir.Count; ++iiii)
        {
            string dataRowString = ticks[iiii].ToString() +
            "\t" + currentTimesMs[iiii].ToString() +
            "\t" + triggerCodes[iiii].ToString() +
            "\t" + currentBlocks[iiii].ToString() +
            "\t" + currentTrials[iiii].ToString() +
            "\t" + gaze_dir[iiii].x.ToString() +
            "\t" + gaze_dir[iiii].y.ToString() +
            "\t" + gaze_dir[iiii].z.ToString() +
            "\t" + head_pos[iiii].x.ToString() +
            "\t" + head_pos[iiii].y.ToString() +
            "\t" + head_pos[iiii].z.ToString() +
            "\t" + head_dir[iiii].x.ToString() +
            "\t" + head_dir[iiii].y.ToString() +
            "\t" + head_dir[iiii].z.ToString() +
            "\t" + eye_openness_Ls[iiii].ToString() +
            "\t" + eye_openness_Rs[iiii].ToString() +
            "\t" + pupil_dia_Ls[iiii].ToString() +
            "\t" + pupil_dia_Rs[iiii].ToString() +
            "\t" + pupil_sensor_Ls[iiii].x.ToString() +
            "\t" + pupil_sensor_Ls[iiii].y.ToString() +
            "\t" + pupil_sensor_Rs[iiii].x.ToString() +
            "\t" + pupil_sensor_Rs[iiii].y.ToString() +
            "\t" + position_Ls[iiii].x.ToString() +
            "\t" + position_Ls[iiii].y.ToString() +
            "\t" + position_Rs[iiii].x.ToString() +
            "\t" + position_Rs[iiii].y.ToString() +
            "\t" + gaze_angles_x[iiii].ToString() +
            "\t" + gaze_angles_y[iiii].ToString() +
            "\t" + head_angles_x[iiii].ToString() +
            "\t" + head_angles_y[iiii].ToString() +
            "\t" + viewpointsGaze[iiii].x.ToString() +
            "\t" + viewpointsGaze[iiii].y.ToString() +
            "\t" + viewpointsGaze[iiii].z.ToString() +
            "\t" + viewpointsFix[iiii].x.ToString() +
            "\t" + viewpointsFix[iiii].y.ToString() +
            "\t" + viewpointsFix[iiii].z.ToString() +
            "\t" + hand_pos[iiii].x.ToString() +
            "\t" + hand_pos[iiii].y.ToString() +
            "\t" + hand_pos[iiii].z.ToString() +
            "\t" + hand_dir[iiii].x.ToString() +
            "\t" + hand_dir[iiii].y.ToString() +
            "\t" + hand_dir[iiii].z.ToString() +
            "\t" + fixationPoints[iiii].x.ToString() +
            "\t" + fixationPoints[iiii].y.ToString() +
            "\t" + fixationPoints[iiii].z.ToString();

            File.AppendAllText(filePath, dataRowString +
            "\n");


        }

    }


    private void WriteTrialData()
    {
        string filePath;

        if (practice)
        {
            filePath = getPracticeDataPath(false);
        }
        else
        {
            filePath = getTrialDataPath(false);
        }

        //StreamWriter writer = new StreamWriter(filePath, true);

        //File.AppendAllText(filePath, "Tick\tTriggerCode\tBlock\tTrial\tGazeDirX\tGazeDirY\tGazeDirZ\tGazePosX\tGazePosY\tGazePosZ\tHeadPosX\tHeadPosY\tHeadPosZ\tHeadDirX\tHeadDirY\tHeadDirZ\tEyeOpenL\tEyeOpenR\tPupilDiaL\tPupilDiaR\tFocusObject\tFocusNormalX\tFocusNormalY\tFocusNormalZ\tFocusDistance\tFocusPointX\tFocusPointY\tFocusPointZ");
        string header = "Pp\tTrial\tBlock\tBlockTrial\tConditionCode\tConditionTurn\tScore\tRT\tHeightOffset\tFacing";


        

        for (int ri = 0; ri < respCount; ri++)
        {
            header += "\t" + "Resp" + (ri+1).ToString() + "_correct";
            header += "\t" + "Resp" + (ri + 1).ToString() + "_encLoc";
            header += "\t" + "Resp" + (ri + 1).ToString() + "_shape";
            header += "\t" + "Resp" + (ri + 1).ToString() + "_colour";
            header += "\t" + "Resp" + (ri + 1).ToString() + "_loc";
            header += "\t" + "Resp" + (ri + 1).ToString() + "_rt";
            header += "\t" + "Resp" + (ri + 1).ToString() + "_colourEncLoc";
            header += "\t" + "Resp" + (ri + 1).ToString() + "_shapeEncLoc";
        }

        /*
        respInfo[i, 0] = 0; //correct
        respInfo[i, 1] = 0; //encloc
        respInfo[i, 2] = 0; //shape
        respInfo[i, 3] = 0; //col
        respInfo[i, 4] = 0; //loc
        respInfo[i, 5] = 0; //respTime
        respInfo[i, 6] = 0; //encColLoc
        respInfo[i, 7] = 0; //encShapeLoc
        */

        for (int ei = 0; ei < sample_shapes.Length; ei++)
        {
            header += "\t" + "Enc" + (ei + 1).ToString() + "_correct";
            header += "\t" + "Enc" + (ei + 1).ToString() + "_rank";
            header += "\t" + "Enc" + (ei + 1).ToString() + "_shape";
            header += "\t" + "Enc" + (ei + 1).ToString() + "_colour";
            header += "\t" + "Enc" + (ei + 1).ToString() + "_respLoc";
            header += "\t" + "Enc" + (ei + 1).ToString() + "_rt";
        }

        /*
        encLocInfo[i, 0] = 0; //correct
        encLocInfo[i, 1] = 0; //rank
        encLocInfo[i, 2] = 0; //shape
        encLocInfo[i, 3] = 0; //col
        encLocInfo[i, 4] = 0; //probeloc
        encLocInfo[i, 5] = 0; //respTime
        */

        for (int ci = 0; ci < colors.Length; ci++)
        {
            header += "\t" + "Colour" + (ci + 1).ToString() + "_correct";
            header += "\t" + "Colour" + (ci + 1).ToString() + "_encLoc";
            header += "\t" + "Colour" + (ci + 1).ToString() + "_encShape";
            header += "\t" + "Colour" + (ci + 1).ToString() + "_respLoc";
            header += "\t" + "Colour" + (ci + 1).ToString() + "_respShape";
            header += "\t" + "Colour" + (ci + 1).ToString() + "_respRank";
            header += "\t" + "Colour" + (ci + 1).ToString() + "_rt";
        }

        /*
        colInfo[i, 0] = 0; //correct
        colInfo[i, 1] = 0; //encloc
        colInfo[i, 2] = 0; //encshape
        colInfo[i, 3] = 0; //probeloc
        colInfo[i, 4] = 0; //probeshape
        colInfo[i, 5] = 0; //respRank
        colInfo[i, 6] = 0; //respTime
        */
        for (int si = 0; si < shapes.Length; si++)
        {
            header += "\t" + "Shape" + (si + 1).ToString() + "_correct";
            header += "\t" + "Shape" + (si + 1).ToString() + "_encLoc";
            header += "\t" + "Shape" + (si + 1).ToString() + "_encColour";
            header += "\t" + "Shape" + (si + 1).ToString() + "_respLoc";
            header += "\t" + "Shape" + (si + 1).ToString() + "_respColour";
            header += "\t" + "Shape" + (si + 1).ToString() + "_respRank";
            header += "\t" + "Shape" + (si + 1).ToString() + "_rt";
        }

        /*
        shapeInfo[i, 0] = 0; //correct
        shapeInfo[i, 1] = 0; //encloc
        shapeInfo[i, 2] = 0; //enccol
        shapeInfo[i, 3] = 0; //probeloc
        shapeInfo[i, 4] = 0; //probecol
        shapeInfo[i, 5] = 0; //respRank
        shapeInfo[i, 6] = 0; //respTime
        */



        header += "\n";

        //writer.WriteLine ();
        if (!System.IO.File.Exists(filePath))
        {
            File.AppendAllText(filePath, header);
            //tm.text = "wrote header";
        }
        //string rt = responseTime.ToString();

        /*
        string visualFrameHand = (Math.Round(responseRotation)).ToString();
        string worldFrameHand = (Math.Round(360f - responseRotation)).ToString();

        string scoreTargetVisual = ScoreFromAngles(responseRotation, (float)targetRotation).ToString();
        string scoreTargetWorld = ScoreFromAngles(responseRotation, 360f - (float)targetRotation).ToString();
        string errorTargetVisual = SignedAngleError((float)targetRotation, responseRotation).ToString();
        string errorTargetWorld = SignedAngleError(360f - (float)targetRotation, responseRotation).ToString();
        string scoreDistractorVisual = ScoreFromAngles(responseRotation, (float)distractorRotation).ToString();
        string scoreDistractorWorld = ScoreFromAngles(responseRotation, 360f - (float)distractorRotation).ToString();
        string errorDistractorVisual = SignedAngleError((float)distractorRotation, responseRotation).ToString();
        string errorDistractorWorld = SignedAngleError(360f - (float)distractorRotation, responseRotation).ToString();
        */
        string score = correctness.ToString();
        /*
        string trialType_ = "report";
        string responded = (Convert.ToInt32(answered)).ToString();
        string visualtop = (Convert.ToInt32(visualTop)).ToString();
        string frameUsed = usedFrame;
        string corrHand = correctHand;
        string usedHand = handUsed;
        */
        //string resploc_ = respLoc.ToString();
        string rt = (respInfo[0, 5] + respInfo[1, 5] + respInfo[2, 5] + respInfo[3, 5]).ToString();


        File.AppendAllText(filePath, participant_id +
        "\t" + absoluteTrial.ToString() +
        "\t" + currentBlock.ToString() +
        "\t" + currentTrial.ToString() +
        "\t" + conditionCode.ToString() +
        "\t" + currentConditionTurn +
        "\t" + score +
        "\t" + rt +
        "\t" + heightOffset.y.ToString() +
        "\t" + facing
        );

        //object infos col, loc, probe_loc, ..... exitViewTimestamp, exitViewLocation, centerExitViewTimestamp, centerExitViewLocation

        /*
        for (int obj = 0; obj < sample_shapes.Length; obj++)
        {
            int temp_or_ = (int)Math.Round(sample_shapes[obj].GetComponent<Shapescript>().rotation);
            int temp_col_ = sample_shapes[obj].GetComponent<Shapescript>().colorCode;

            //write this bar's info
            File.AppendAllText(filePath, "\t" + temp_or_.ToString());
            File.AppendAllText(filePath, "\t" + temp_col_.ToString());
        }
        */

        //RESP
        for (int ri = 0; ri < respCount; ri++)
        {
            string resp_corr_ = respInfo[ri, 0].ToString(); //correct
            string resp_encloc_ = respInfo[ri, 1].ToString(); //encloc
            string resp_shape_ = respInfo[ri, 2].ToString(); //shape
            string resp_col_ = respInfo[ri, 3].ToString(); //col
            string resp_probeloc_ = respInfo[ri, 4].ToString(); //loc
            string resp_rt_ = respInfo[ri, 5].ToString(); //respTime
            string resp_encColLoc_ = respInfo[ri, 6].ToString(); //encColLoc
            string resp_encShapeLoc_ = respInfo[ri, 7].ToString(); //encShapeLoc

            if (resp_corr_.Equals("0")) { resp_encloc_ = "nan"; }

            //write this bar's info
            File.AppendAllText(filePath, "\t" + resp_corr_);
            File.AppendAllText(filePath, "\t" + resp_encloc_);
            File.AppendAllText(filePath, "\t" + resp_shape_);
            File.AppendAllText(filePath, "\t" + resp_col_);
            File.AppendAllText(filePath, "\t" + resp_probeloc_);
            File.AppendAllText(filePath, "\t" + resp_rt_);
            File.AppendAllText(filePath, "\t" + resp_encColLoc_);
            File.AppendAllText(filePath, "\t" + resp_encShapeLoc_);

        }

        //ENC
        for (int ei = 0; ei < sample_shapes.Length; ei++)
        {
            string enc_corr_ = encLocInfo[ei, 0].ToString(); //correct
            string enc_rank_ = encLocInfo[ei, 1].ToString(); //rank
            string enc_shape_ = encLocInfo[ei, 2].ToString(); //shape
            string enc_col_ = encLocInfo[ei, 3].ToString(); //col
            string enc_probeloc_ = encLocInfo[ei, 4].ToString(); //probeloc
            string enc_rt_ = encLocInfo[ei, 5].ToString(); //respTime

            if (enc_corr_.Equals("0"))
            {
                enc_rank_ = "nan";
                enc_rt_ = "nan";
            }

            //write this bar's info
            File.AppendAllText(filePath, "\t" + enc_corr_);
            File.AppendAllText(filePath, "\t" + enc_rank_);
            File.AppendAllText(filePath, "\t" + enc_shape_);
            File.AppendAllText(filePath, "\t" + enc_col_);
            File.AppendAllText(filePath, "\t" + enc_probeloc_);
            File.AppendAllText(filePath, "\t" + enc_rt_);

        }

        //COLOUR
        for (int ci = 0; ci < colors.Length; ci++)
        {
            string col_corr_ = colInfo[ci, 0].ToString(); //correct
            string col_encloc_ = colInfo[ci, 1].ToString(); //encloc
            string col_encshape_ = colInfo[ci, 2].ToString(); //encshape
            string col_probeloc_ = colInfo[ci, 3].ToString(); //probeloc
            string col_probeshape_ = colInfo[ci, 4].ToString(); //probeshape
            string col_rank_ = colInfo[ci, 5].ToString(); //resprank
            string col_rt_ = colInfo[ci, 6].ToString(); //rt

            //write this bar's info
            File.AppendAllText(filePath, "\t" + col_corr_);
            File.AppendAllText(filePath, "\t" + col_encloc_);
            File.AppendAllText(filePath, "\t" + col_encshape_);
            File.AppendAllText(filePath, "\t" + col_probeloc_);
            File.AppendAllText(filePath, "\t" + col_probeshape_);
            File.AppendAllText(filePath, "\t" + col_rank_);
            File.AppendAllText(filePath, "\t" + col_rt_);

        }

        //SHAPE
        for (int si = 0; si < shapes.Length; si++)
        {
            string shape_corr_ = shapeInfo[si, 0].ToString(); //correct
            string shape_encloc_ = shapeInfo[si, 1].ToString(); //encloc
            string shape_enccol_ = shapeInfo[si, 2].ToString(); //enccol
            string shape_probeloc_ = shapeInfo[si, 3].ToString(); //probeloc
            string shape_probecol_ = shapeInfo[si, 4].ToString(); //probecol
            string shape_rank_ = shapeInfo[si, 5].ToString(); //resprank
            string shape_rt_ = shapeInfo[si, 6].ToString(); //rt

            //write this bar's info
            File.AppendAllText(filePath, "\t" + shape_corr_);
            File.AppendAllText(filePath, "\t" + shape_encloc_);
            File.AppendAllText(filePath, "\t" + shape_enccol_);
            File.AppendAllText(filePath, "\t" + shape_probeloc_);
            File.AppendAllText(filePath, "\t" + shape_probecol_);
            File.AppendAllText(filePath, "\t" + shape_rank_);
            File.AppendAllText(filePath, "\t" + shape_rt_);

        }


        File.AppendAllText(filePath, "\n");


    }



    private void ClearTempData()
    {
        //time series
        gaze_dir.Clear();
        gaze_pos.Clear();
        head_pos.Clear();
        head_dir.Clear();
        //presentation_positions.Clear();
        //presentation_anglediffs.Clear();
        eye_openness_Ls.Clear();
        eye_openness_Rs.Clear();
        pupil_dia_Ls.Clear();
        pupil_dia_Rs.Clear();
        pupil_sensor_Ls.Clear();
        pupil_sensor_Rs.Clear();
        position_Ls.Clear();
        position_Rs.Clear();
        ray_Ls.Clear();
        ray_Rs.Clear();
        ray_Cs.Clear();
        info_Ls.Clear();
        info_Rs.Clear();
        info_Cs.Clear();
        focusNormals.Clear();
        focusDistances.Clear();
        focusPoints.Clear();
        triggerCodes.Clear();
        focusTags.Clear();
        currentTimesMs.Clear();
        ticks.Clear();
        viewpointsGaze.Clear();
        viewpointsFix.Clear();
        viewpointsCube.Clear();
        //viewpointsBar.Clear();
        viewpointsSphere.Clear();
        //viewpointsCylinder.Clear();
        //viewpointsTriangle.Clear();
        viewpointsDiamond.Clear();
        viewpointsStar.Clear();
        hand_pos.Clear();
        hand_dir.Clear();
        //hand_posL.Clear();
        //hand_dirL.Clear();
        fixationPoints.Clear();
        currentTrials.Clear();
        currentBlocks.Clear();
        locSequence.Clear();
        visualMatchesHand.Clear();
        worldMatchesHand.Clear();
    }

    public void ResetHeight()
    {
        camHeight = new Vector3(0.0f, Camera.main.transform.position.y, 0.0f); //Camera.main.transform.position.y

        //tm.transform.position = new Vector3(0f, -verticalDev, -2f) + camHeight + relHeight;
        fixationtarget.transform.position = new Vector3(0f, -verticalDev, fixationDepth) + camHeight + relHeight;

        heightOffset.y = fixationtarget.transform.position.y;
        //refPos.y = Camera.main.transform.position.y;
    }

    public int getFront()
    {
        return front;
    }

    public string getFacing()
    {
        return facing;
    }

    public int positionToCode(Vector3 spawn_pos)
    {
        return positionCodes[spawn_pos - heightOffset];
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

    public int ScoreFromAngles(float angle1, float angle2)
    {
        float a1 = angle1 % 180f;
        float a2 = angle2 % 180f;
        float diff = Math.Abs(a1 - a2);
        if (diff > 90)
        {
            diff = 90 - Math.Abs(90 - diff);
        }
        int score = (int)Math.Round((1 - (diff / 90)) * 100);
        return score;
    }
    public int SignedAngleError(float angle2, float angle1)
    {
        angle1 = angle1 % 180 - 180f;
        angle2 = angle2 % 180 - 180f;
        int diff = (int)angle2 - (int)angle1;
        diff += (diff > 90) ? -180 : (diff < -90) ? 180 : 0;
        return diff;
    }

    // / <summary>
    /// Calculates the intersection of two given lines
    /// </summary>
    /// <param name="intersection">returned intersection</param>
    /// <param name="linePoint1">start location of the line 1</param>
    /// <param name="lineDirection1">direction of line 1</param>
    /// <param name="linePoint2">start location of the line 2</param>
    /// <param name="lineDirection2">direction of line2</param>
    /// <returns>true: lines intersect, false: lines do not intersect</returns>

    public Vector2 calculateOrbit(float currentOrbitDegrees, float distanceFromCenterPoint, Vector2 centerPoint)
    {
        float radians = Mathf.Deg2Rad * (currentOrbitDegrees + 90);

        float x = (Mathf.Cos(radians) * distanceFromCenterPoint) + centerPoint.x;
        float y = (Mathf.Sin(radians) * distanceFromCenterPoint) + centerPoint.y;

        return new Vector2(x, y);
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

    /*
    private void showRespObj()
    {
        foreach (Renderer rendcomp in respObj.GetComponentsInChildren<Renderer>())
        {
            rendcomp.enabled = true;
        }
    }
    */
    /*
    public float getHandRot()
    {
        return currentRotation;
    }
    */
    /*
    private void hideRespObj()
    {
        foreach (Renderer rendcomp in respObj.GetComponentsInChildren<Renderer>())
        {
            rendcomp.enabled = false;
        }
    }
    */
    private void showFixation()
    {
        fixationtarget.GetComponent<Renderer>().enabled = true;
    }

    private void hideFixation()
    {
        fixationtarget.GetComponent<MeshRenderer>().enabled = false;
    }
    private void paintFixation(Color colour)
    {
        fixationtarget.GetComponent<Renderer>().enabled = true;
        fixationtarget.GetComponent<MeshRenderer>().material.color = colour;
    }
    private void clearFixation()
    {
        fixationtarget.GetComponent<Renderer>().enabled = true;
        fixationtarget.GetComponent<MeshRenderer>().material.color = defaultFixationColour;
    }

    public void setTextLoc(string loc)
    {
        textFollowView = false;
        tm.color = defaultTxtColour;
        if (loc.Equals("far"))
        {
            tm.fontSize = farFontSize;
            tm.characterSize = farCharacterSize;
            if (facing.Equals("back"))
            {
                tm.transform.position = new Vector3(0f, 1.5f, -5f);
            }
            else if (facing.Equals("front"))
            {
                tm.transform.position = new Vector3(0f, 1.5f, 5f);
            }

        }
        else if (loc.Equals("near"))
        {
            tm.fontSize = nearFontSize;
            tm.characterSize = nearCharacterSize;
            tm.transform.position = new Vector3(fixationtarget.transform.position.x, fixationtarget.transform.position.y + 0.05f, fixationtarget.transform.position.z);

        }
        else if (loc.Equals("view"))
        {
            textFollowView = true;
            tm.color = errorTxtColour;
            tm.fontSize = viewFontSize;
            tm.characterSize = viewCharacterSize;
        }
    }

    public UnityEngine.XR.InputDevice getHand()
    {
        return handR;
    }

}
