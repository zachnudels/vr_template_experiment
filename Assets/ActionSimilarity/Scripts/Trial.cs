using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using UXF;
using TMPro;


namespace ActionSimilarity
{
    
    public enum FaceDirection
    {
        Front = 1,
        Back = -1,
    }
    
    [System.Serializable]
    public class CodeDictionary
    {
        public string key;
        public int value;
    }
    
    [System.Serializable]
    public class Vector3Dictionary
    {
        public string key;
        public Vector3 value;
    }


    [Serializable]
    public class FixationSettings
    {
        public GameObject fixationSphere;
        [SerializeField]
        private int fixationDepth;
        public int FixationDepth => fixationDepth;

        public TextMeshPro textMeshPro;
    }

    [Serializable]
    public class ShapeSettings
    {
        [HideInInspector] public int count;
        public GameObject encodingShape;
        public GameObject reportingShape;
        public Mesh[] shapeMeshes;
        public Color[] shapeColours;
        public List<Vector3Dictionary> reportingShapeRotations;
        public List<Vector3Dictionary> encodingShapeRotations;
        public List<Vector3Dictionary> shapeScales;
        public float[] shapeSpacing;

        [HideInInspector] public Vector3[] encodingShapePositions;
        [HideInInspector] public List<int> shapePositions; // which mesh goes on which encoding shape
        [HideInInspector] public List<int> colorPositions; // which color goes on which encoding shape
        [HideInInspector] public Vector3[] reportingShapePositions;
        
        [HideInInspector] public List<int> shapeRows; 
        [HideInInspector] public List<int> colorCols;
        [HideInInspector] public Dictionary<string, Vector3> reportingShapeRotationMap;
        [HideInInspector] public Dictionary<string, Vector3> encodingShapeRotationMap;
        [HideInInspector] public Dictionary<string, Vector3> shapeScaleMap;

        public void Init()
        {
            
            InitEncodingPositions();
            InitReportingPositions();
                
            if (this.shapeMeshes.Length != this.shapeColours.Length || this.shapeColours.Length != this.encodingShapePositions.Length)
            {
                throw new UnityException("Mesh, Color, Position Length must all be equal. See Trial object.");
            }

            this.count = this.shapeMeshes.Length;
            if (count * count != this.reportingShapePositions.Length)
            {
                throw new UnityException("reportingShapes must be square of other values!");
            }
            
            reportingShapeRotationMap = reportingShapeRotations.ToDictionary(e => e.key, e => e.value);
            encodingShapeRotationMap = encodingShapeRotations.ToDictionary(e => e.key, e => e.value);
            shapeScaleMap = shapeScales.ToDictionary(e => e.key, e => e.value);

        }

        private void InitEncodingPositions()
        {
            encodingShapePositions = new Vector3[2 * shapeSpacing.Length];
            
            for (int i = 0; i != shapeSpacing.Length; ++i)
            {
                encodingShapePositions[i * 2] = new Vector3(0f, 0f, shapeSpacing[i]);
                encodingShapePositions[i * 2 + 1] = new Vector3(0f, 0f, -shapeSpacing[i]);
            }
            Array.Sort(encodingShapePositions, (a, b) => a.z.CompareTo(b.z));

        }

        private void InitReportingPositions()
        {
            reportingShapePositions = new Vector3[4 * shapeSpacing.Length * shapeSpacing.Length];
            
            List<float> directedShapePositions = new List<float>();
            foreach (float s in shapeSpacing)
            {
                directedShapePositions.Add(-s);
                directedShapePositions.Add(s);
            }

            directedShapePositions.Sort();
            
            int index = 0;
            foreach (float y in directedShapePositions)
            {
                foreach (float x in directedShapePositions)
                {
                    reportingShapePositions[index] = new Vector3(x, y, 0f);
                    index += 1;
                }
            }
        }
    }
    
    public class Trial : MonoBehaviour
    {
        public Session session;
        public FixationSettings fixationSettings;
        public ShapeSettings shapeSettings;
        public List<CodeDictionary> codes;
        private Dictionary<string, int> codeMap;
        public TextController textControllerWall;
        public bool simulating;
        public bool pause;
        public bool initializedEnvironment = false;

        float leftOffset;
        float downOffset;
        FaceDirection faceDirection;
        private TurnDirection _turnDirection;
        Transform eyes;
        string hand;
        private FixationRotator fixationRotator;
        DataProcessing data;
        private bool debug;
        private Dictionary<(int, int), GameObject> reportingStimuli;
        private int reportedIndex;
        private UXF.Trial _uxf;
        private float responseStartTime;

        private int _colorCode;

        bool practiceBlock;
        bool firstTrial;
        bool sessionStart;

        private string _stageName;
        private List<ResponseShapeMetadata> _reportedStimuli;

        private int _n_correct;
        
        /*
        * code will be sent to the eye tracking recorder
        * Please set the mapping from event to code
        * Make sure to leave enough space between each event code
        * This allows you to manipulate the code based on a condition at the start 
        * of the stage
        */


        void Awake()
        {
            shapeSettings.Init();
            codeMap = codes.ToDictionary(e => e.key, e => e.value);
        }


        // Start is called before the first frame update
        void Start()
        {
            
            Application.targetFrameRate = 90;

            // Find eyes depending on simulation or not
            SetEyes();

            fixationSettings.fixationSphere = InstantiateObject(fixationSettings.fixationSphere,
                fixationSettings.fixationSphere.transform.position,
                new Vector3(0.0f, 0.0f, 0.0f),
                "fixation"
            );

            fixationSettings.textMeshPro = fixationSettings.fixationSphere.GetComponentInChildren<TextMeshPro>();
            fixationSettings.textMeshPro.enabled = false;

            leftOffset = simulating ? 0.0f : -0.0075f;
            downOffset = simulating ? 0.0f : -0.15f;

            _stageName = "none";
            fixationRotator = new FixationRotator();
            faceDirection = FaceDirection.Front;
        }

        void SetEyes()
        {
            GameObject simulateObj = GameObject.Find("Simulate");
            GameObject playerObj = GameObject.Find("Player");

            if (simulateObj != null && simulateObj.activeInHierarchy)
            {
                if (playerObj != null && playerObj.activeInHierarchy)
                {
                    throw new UnityException("Must choose to activate only one between Simulate and Player!");
                }
                simulating = true;
            }
            else if (playerObj != null && playerObj.activeInHierarchy)
            {
                simulating = false;
            }
            else
            {
                Debug.LogError("Neither Simulate nor Player is active in hierarchy");
            }

            GameObject cameraOffset = GameObject.Find("Main Camera");
            if (cameraOffset != null)
            {
                eyes = cameraOffset.transform;
               
            }
            else
            {
                Debug.LogError("Camera not found");
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Assign settings using <varName> = trial.settings.Get<Type>("<settingName>");
        /// for each setting. Assumes <varName> has been instantiated above as a class attribute
        /// and that the type of <varName> == <Type>.
        /// Fo consistency, it's best if <varName> and <settingName> are identical
        ///
        /// You may also want to set some results here already. E.g., if you want to save some
        /// condition: trial.result["<settingName>"] = <varName>;
        /// </summary>
        private void ExtractSettings()
        {
            //shapePair = trial.settings.GetString("shapePair");
            //shapeCategory = trial.settings.GetString("shapeCategory");

            //straightTop = trial.settings.GetBool("straightTop");
            //straightLeft = trial.settings.GetBool("straightLeft");
            //condition = trial.settings.GetInt("condition");
            //sameCondition = condition == 1;
            //actionPos = trial.settings.GetInt("actionPos");
            //compPos = trial.settings.GetInt("compPos");
            //jitter = trial.settings.GetInt("jitter");
            //practiceBlock = trial.settings.GetBool("practice");
            debug = _uxf.settings.GetBool("debug");

            shapeSettings.shapePositions = _uxf.settings.GetIntList("shapePositions");
            shapeSettings.colorPositions = _uxf.settings.GetIntList("colorPositions");
            shapeSettings.colorCols = _uxf.settings.GetIntList("colorCols");
            shapeSettings.shapeRows = _uxf.settings.GetIntList("shapeRows");

            _turnDirection = (TurnDirection)_uxf.settings.GetObject("turnDirection");

            _colorCode = _uxf.settings.GetInt("colorCode");
            // TODO: write results 
            
            _uxf.result["ConditionCode"] = _turnDirection == TurnDirection.Left ? 1 : 2;
            _uxf.result["ConditionTurn"] = _turnDirection.ToString().ToLower();
            _uxf.result["HeightOffset"] = eyes.position.y;
            _uxf.result["Facing"] = faceDirection.ToString().ToLower();

            //trial.result["straightTop"] = straightTop;
            //trial.result["straightLeft"] = straightLeft;
            //trial.result["sameCondition"] = sameCondition;
            //trial.result["actionPos"] = actionPos;

            //trial.result["PerceptionShapeIndex"] = compPos;
            //trial.result["PerceptionJitter"] = jitter;
            //trial.result["PerceptionCategory"] = shapeCategory;
        }

        private void SetHand()
        {
            try
            {
                string handedness = (string)session.participantDetails["subjectHand"];
                if (handedness.Equals("Right")) hand = "RightHand";
                else if (handedness.Equals("Left")) hand = "LeftHand";
                else
                {
                    Debug.LogError("Hand must be either right or left");
                    Application.Quit();
                }
            }
            catch
            {
                hand = "RightHand";
            }
        }

        /// <summary>
        /// Reinitialize any variables for the trial
        /// 
        /// </summary>
        private void InitializeTrial()
        {


            //topDone = false;
            //bottomDone = false;

            Debug.Log("Initialize Trial");


            data = (DataProcessing)_uxf.settings.GetObject("data");

            //actionsDone = false;
            //showingTarget = false;
            pause = false;
            firstTrial = false;
            sessionStart = false;

            // Change which way we are facing
            faceDirection = _uxf.number % 2 == 0 ? FaceDirection.Back : FaceDirection.Front;

            textControllerWall.ChangeWall(faceDirection);

            _reportedStimuli = new List<ResponseShapeMetadata>();

            reportedIndex = 1;

            _n_correct = 0;
        }


        public void setTrigger(int code = 0)
        {
            int turnCondition = (_turnDirection == TurnDirection.Left) ? 0 : 1;
            int faceCondition = (faceDirection == FaceDirection.Front) ? 0 : 1;
            code += faceCondition * 2 + turnCondition + 1;
            Debug.Log("Trigger: " + code);
            session.settings.SetValue("triggerCode", code);
            // front left  = 1
            // front right = 2
            // back left = 3
            // back right = 4
        }


        /// <summary>
        /// Assign this method to the Session OnTrialBegin UnityEvent in its inspector.
        /// This method build the trials and then runs it.
        /// You should not need to manipulate anything in it too much.
        /// Rather, manipulate the methods it calls.
        /// 
        /// </summary>
        public void BuildAndRunTrial(UXF.Trial trial)
        {
            this._uxf = trial;
            Debug.Log("Building Trial");

            // Common Build Methods
            ExtractSettings();

            // Custom Code - reinitialise any variables
            InitializeTrial();

            // Run Trial
            StartCoroutine(RunTrial());


        }

        IEnumerator RunTrial()
        {
            Debug.Log("Starting Trial");
            if (_uxf.numberInBlock == 1)
            {
                Debug.Log("First in block");
                firstTrial = true;
                if (_uxf.number != 1)
                {
                    Debug.Log("Starting Block");
                    yield return RunStage(StartBlock);

                    yield return RunStage(Break);

                    fixationSettings.fixationSphere.transform.position = eyes.position + new Vector3(0.0f, downOffset, fixationSettings.FixationDepth * (int)faceDirection);


                    // if (condition != session.CurrentBlock.settings.GetInt("condition"))
                    // {
                    //     // New condition!
                    //     yield return RunStage(Instructions);
                    // }
                }
                else
                {
                    fixationSettings.fixationSphere.SetActive(false);
                    Debug.Log("Starting Session");
                    yield return RunStage(StartSession);

                    yield return RunStage(Instructions);

                    fixationSettings.fixationSphere.transform.position = eyes.position + new Vector3(0.0f, downOffset, fixationSettings.FixationDepth * (int)faceDirection);
                    fixationSettings.fixationSphere.SetActive(true);


                }
            }
            
            yield return RunStage(WaitToStart);  // preparation + jitter 

            // bool calibrated = ViveSR.anipal.Eye.SRanipal_Eye.LaunchEyeCalibration();

            
            // Display shapes
            // GameObject[] stimuli = InstantiateEncodingStimuli();
            
            yield return RunStage(Turn);  // Turn  

            //foreach (GameObject gameObject in stimuli)
            //{
            //    Destroy(gameObject);
            //}

            // yield return RunStage(Presentation);  // Presentation 

            // yield return RunStage(Cue);  // answer

            yield return RunStage(Report);  // answer

            LogResults();

            yield return RunStage(Feedback);

            if (_uxf == session.LastTrial)
            {
                yield return RunStage(EndSession);
            }


            session.CurrentTrial.End();
            session.NextTrial.Begin();

        }

        IEnumerator RunStage(Func<IEnumerator> stage)
        {
            _stageName = stage.Method.Name;
            UnityEngine.Debug.Log(_stageName);
            textControllerWall.Debug(_stageName);
            // code = stageName.GetHashCode();
            yield return StartCoroutine(stage());
            textControllerWall.Clear();
        }

        /*
        * Trial Stages
        */

        IEnumerator StartSession()
        {
            // condition = session.CurrentBlock.settings.GetInt("condition");
            textControllerWall.Write($"Welcome! We hope you enjoy. Blah blah. Pull trigger to start.");
            // If first trial of session, wait for trigger to start

            pause = true;
            yield return new WaitUntil(() => !pause);
            sessionStart = true;

            if (!debug)
            {
                bool calibrated = ViveSR.anipal.Eye.SRanipal_Eye.LaunchEyeCalibration();
                while (!calibrated)
                {
                    calibrated = ViveSR.anipal.Eye.SRanipal_Eye.LaunchEyeCalibration();
                }
                yield return new WaitUntil(() => calibrated);
            }
            textControllerWall.Write("Pull trigger to continue.");

            SetEyes();
            

            pause = true;
            yield return new WaitUntil(() => !pause);
            textControllerWall.Debug("Unpaused");


        }

        IEnumerator StartBlock()
        // If first trial of new block, show feedback and take a break
        {

            yield return new WaitForSeconds(0.1f);
            textControllerWall.Write("Well done! Time to take a well deserved break"
                            + " \n\n Press any thumb button when you're ready to start the next block");

        }

        IEnumerator Instructions()
        {
            UnityEngine.Debug.Log("Running Instructions");
            textControllerWall.Write("Display your instructions here \n\nPress any thumb button to continue");
            UnityEngine.Debug.Log("pausing");
            yield return null;
            pause = true;
            yield return new WaitUntil(() => !pause);
            UnityEngine.Debug.Log("unpause");

            textControllerWall.Clear();
            setTrigger(codeMap["instructions"]);

            yield return new WaitForSeconds(3f);
        }

        IEnumerator Break()
        {
            yield return new WaitForSeconds(3f);
            data.reset(session.CurrentBlock.trials.Count);

            pause = true;
            yield return new WaitUntil(() => !pause);

            textControllerWall.Clear();
            yield return new WaitForSeconds(3f);

        }

        IEnumerator WaitToStart()
        {
            SetHand();
            SetEyes();
            setTrigger(codeMap["jitter"]);
            yield return new WaitForSeconds(session.CurrentTrial.settings.GetFloat("ITI"));
        }

        IEnumerator Turn()
        {
            fixationRotator.Reset(_turnDirection);

            float elapsed = 0f;
            bool triggerSent = false;
            
            float halfwayTime = fixationRotator.TurnTime / 2f;
            
            setTrigger(codeMap["turn"]);

            while (!fixationRotator.isFinished())
            {
                float dt = Time.deltaTime;
                elapsed += dt;
                fixationRotator.Step(dt); 
                fixationSettings.fixationSphere.transform.position = fixationRotator.GetCurrentPosition(
                    fixationSettings.FixationDepth, fixationSettings.fixationSphere.transform.position, faceDirection);
                
                if (!triggerSent && elapsed >= halfwayTime)
                {
                    setTrigger(codeMap["halfway_turn"]);
                    triggerSent = true;
                    StartCoroutine(ShowEncodingShapesForOneFrame());
                }

                yield return null;
            }
            
            faceDirection = textControllerWall.ChangeWall(faceDirection, true);
        }

        IEnumerator ShowEncodingShapesForOneFrame()
        {
            GameObject[] stimuli = InstantiateEncodingStimuli();
            yield return null;
            foreach (GameObject gameObject in stimuli)
            {
                Destroy(gameObject);
            }


        }


        IEnumerator Report()
        {

            // pause = true;
            // yield return new WaitUntil(() => !pause);

            setTrigger(codeMap["answer"]);

            fixationSettings.fixationSphere.SetActive(false);

            this.reportingStimuli = InstantiateReportStimuli();



            float startTime = Time.time;
            this.responseStartTime = startTime;

            // yield return new WaitForSeconds((session.CurrentTrial.settings.GetFloat("reportTime")));

            // pause = true;
            yield return new WaitUntil(() => reportedIndex == 2);


            if (this.reportingStimuli == null)
            {
                yield break;
            }

            setTrigger(codeMap["answer_done"]);

            float endTime = Time.time;

            foreach (GameObject stimulus in this.reportingStimuli.Values)
            {
                ReportShapeSelected(stimulus, true); // even though we cannot select these, report on them but set to ignore
            }

            yield return null;
            yield return null;

            fixationSettings.fixationSphere.SetActive(true);

            _uxf.result["Score"] = _n_correct;
            _uxf.result["RT"] = endTime - startTime;
            


        }

        IEnumerator Feedback()
        {
            setTrigger(codeMap["feedback"]);

            

            fixationSettings.textMeshPro.text = _n_correct.ToString();

            float feedbackRotation = (faceDirection == FaceDirection.Front) ? 0f : 180f;
            fixationSettings.textMeshPro.transform.rotation = Quaternion.Euler(new Vector3(0f, feedbackRotation, 0f));

            fixationSettings.textMeshPro.enabled = true;

            yield return new WaitForSeconds(session.CurrentTrial.settings.GetFloat("feedbackTime"));
            
            fixationSettings.textMeshPro.enabled = false;

        }

        IEnumerator EndSession()
        // If first trial of new block, show feedback and take a break
        {

            yield return new WaitForSeconds(0.1f);
            textControllerWall.Write("Well done! Thank you so much for joining us today!");
            yield return new WaitForSeconds(20f);
            session.End();

        }


        /*
        * Instantiation methods
        */

        GameObject[] InstantiateEncodingStimuli()
        {

            GameObject[] stimuli = new GameObject[shapeSettings.count];
            Debug.Log($"shapeSettings.count: {shapeSettings.count}");
            Debug.Log($"colorPositions.Length: {shapeSettings.colorPositions.Count}");
            Debug.Log($"shapeColours.Length: {shapeSettings.shapeColours.Length}");
            Debug.Log($"shapePositions.Length: {shapeSettings.shapePositions.Count}");
            Debug.Log($"shapeMeshes.Length: {shapeSettings.shapeMeshes.Length}");
            Debug.Log($"encodingShapePositions.Length: {shapeSettings.encodingShapePositions.Length}");

            for (int i = 0; i < shapeSettings.count; i++)
            {
                Debug.Log($"i: {i}, colorIndex: {shapeSettings.colorPositions[i]}, meshIndex: {shapeSettings.shapePositions[i]}");

                if (shapeSettings.colorPositions[i] >= shapeSettings.shapeColours.Length)
                    Debug.Log($"Invalid color index {shapeSettings.colorPositions[i]} at i={i}");

                if (shapeSettings.shapePositions[i] >= shapeSettings.shapeMeshes.Length)
                    Debug.Log($"Invalid mesh index {shapeSettings.shapePositions[i]} at i={i}");
                
                Color color = shapeSettings.shapeColours[shapeSettings.colorPositions[i]];
                Mesh mesh = shapeSettings.shapeMeshes[shapeSettings.shapePositions[i]];
                Vector3 rotation = shapeSettings.encodingShapeRotationMap.TryGetValue(mesh.name, out var rot) ? rot : Vector3.zero;
                Vector3 scale = shapeSettings.shapeScaleMap.TryGetValue(mesh.name, out var scal) ? scal : Vector3.one;

                // Change to x rotation since we're sideways now
                // Debug.Log(faceDirection.ToString() + " " + _turnDirection.ToString());
                // rotation = new Vector3(rotation.z, rotation.y, rotation.x);
                Vector3 position = shapeSettings.encodingShapePositions[i]
                                   + fixationSettings.fixationSphere.transform.position;
                stimuli[i] = InstantiateObjectWithMeshAndColor(shapeSettings.encodingShape,
                    mesh,
                    position,
                    rotation,
                    color,
                    scale * 0.875f
               );
            }

            return stimuli;
        }

        Dictionary<(int, int), GameObject> InstantiateReportStimuli()
        {
            Dictionary<(int, int), GameObject> stimuli = new Dictionary<(int, int), GameObject>();
            
            Dictionary<(int, int), int> pairIndex = new Dictionary<(int, int), int>();
            for (int i = 0; i < shapeSettings.count; i++)
            {
                pairIndex[(shapeSettings.colorPositions[i], shapeSettings.shapePositions[i])] = i;
            }
            
            for (int colorRow = 0; colorRow != shapeSettings.count; ++colorRow)
            {
                for (int shapeCol = 0; shapeCol != shapeSettings.count; ++shapeCol)
                {
                    int posIndex = colorRow + shapeCol + (colorRow * (shapeSettings.count - 1));
                    int color_i = shapeSettings.shapeRows[colorRow];
                    int shape_i = shapeSettings.colorCols[shapeCol];
                    Debug.Log($"Shape pos: {posIndex} colorI: {color_i}, shape_i: {shape_i}");
                        
                    Color color = color_i == _colorCode ? shapeSettings.shapeColours[color_i] : Color.gray;
                    Mesh mesh = shapeSettings.shapeMeshes[shape_i];
                    Vector3 rotation = shapeSettings.reportingShapeRotationMap.TryGetValue(mesh.name, out var rot) ? rot : Vector3.zero;
                    Vector3 position = shapeSettings.reportingShapePositions[posIndex]
                                       + fixationSettings.fixationSphere.transform.position;
                    Vector3 scale = shapeSettings.shapeScaleMap.TryGetValue(mesh.name, out var scal) ? scal : Vector3.one;


                    GameObject shape = InstantiateObjectWithMeshAndColor(shapeSettings.reportingShape,
                        mesh,
                        position,
                        rotation,
                        color,
                        scale * 0.875f * 0.875f
                    ); ;
                    
                    // Store shapeData object for later processing
                    
                    // Find position of this color/shape in encoding array
                    int colorEncIndex = shapeSettings.colorPositions.IndexOf(color_i);
                    int shapeEncIndex = shapeSettings.shapePositions.IndexOf(shape_i);
                    int encodingIndex = -1;
                    
                    // Determine if object is in encoding array
                    if (pairIndex.TryGetValue((color_i, shape_i), out int index))
                    {
                        encodingIndex = index;
                    }
                    ResponseShapeMetadata shapeData = new ResponseShapeMetadata(
                        color_i,
                        shape_i,
                        posIndex,
                        colorEncIndex,
                        shapeEncIndex,
                        encodingIndex
                    );
                    // Add for logging purposes
                    _reportedStimuli.Add(shapeData);
                    
                    // Add shape Data as a component to the shape GameObject
                    ResponseShapeMetadataObject metadataObject = shape.AddComponent<ResponseShapeMetadataObject>();
                    metadataObject.Data = shapeData;
                    
                    stimuli[(color_i, shape_i)] = shape;

                }
            }

            return stimuli;

        }

        void InstantiateTarget()
        {
        }
        
        /*
         * Logging and cleanup
         */
        public void cleanUpTrial(UXF.Trial trial)
        {
            textControllerWall.Clear();
            StopAllCoroutines();
        }

        void LogResults()
        {
            
            for (int i = 0; i != shapeSettings.count; ++i)
            {
                ResponseShapeMetadata response = _reportedStimuli
                    .FirstOrDefault(item => item.EncodingIndex == i);
                if (response == null)
                {
                    throw new UnityException($"Could not find reported stimuli with encoding index {i}");
                }
                bool notReported = response.RT == -1;

                _uxf.result[$"Enc{i + 1}_rank"] = notReported ? "nan" : response.ReportedIndex.ToString();
                _uxf.result[$"Enc{i + 1}_correct"] = notReported ? 0 : 1;
                _uxf.result[$"Enc{i + 1}_colour"] = shapeSettings.colorPositions[i];
                _uxf.result[$"Enc{i + 1}_shape"] = shapeSettings.shapePositions[i];
                _uxf.result[$"Enc{i + 1}_respLoc"] = response.Loc;
                _uxf.result[$"Enc{i + 1}_rt"] = notReported ? "nan" : response.RT.ToString();
            }
            
            foreach (int i in shapeSettings.colorPositions)
            {
                // if (i != _colorCode)
                // {
                //     continue;
                // }
                ResponseShapeMetadata encodingShape = _reportedStimuli
                    .FirstOrDefault(item => item.ColorIndex == i && item.EncodingIndex != -1);
                if (encodingShape == null)
                {
                    throw new UnityException($"Could not find encoding stimuli with color index {i}");
                }
                
                ResponseShapeMetadata reportedShape = _reportedStimuli
                    .FirstOrDefault(item => item.ColorIndex == i && item.RT != -1);
                if (reportedShape == null && i == _colorCode)
                {
                    throw new UnityException($"Could not find reported stimuli with color index {i}");
                }
                bool notReported = reportedShape == encodingShape;

                _uxf.result[$"Colour{i}_correct"] = notReported ? 0 : 1;
                _uxf.result[$"Colour{i}_encLoc"] = encodingShape.EncodingIndex;
                _uxf.result[$"Colour{i}_encShape"] = encodingShape.ShapeIndex;
                

                _uxf.result[$"Colour{i}_respLoc"] = (reportedShape == null) ? "nan" : reportedShape.Loc.ToString();
                _uxf.result[$"Colour{i}_respShape"] = (reportedShape == null) ? "nan" : reportedShape.ShapeIndex.ToString();
                _uxf.result[$"Colour{i}_respRank"] = (reportedShape == null) ? "nan" : reportedShape.ReportedIndex.ToString();
                _uxf.result[$"Colour{i}_rt"] = (reportedShape == null) ? "nan" : reportedShape.RT.ToString();
            }
            
            foreach (int i in shapeSettings.shapePositions)
            {
                ResponseShapeMetadata encodingShape = _reportedStimuli
                    .FirstOrDefault(item => item.ShapeIndex == i && item.EncodingIndex != -1);
                if (encodingShape == null)
                {
                    throw new UnityException($"Could not find encoding stimuli with shape index {i}");
                }
                
                ResponseShapeMetadata reportedShape = _reportedStimuli
                    .FirstOrDefault(item => item.ShapeIndex == i && item.RT != -1);
                if (reportedShape == null)
                {
                    // throw new UnityException($"Could not find reported stimuli with shape index {i}");
                    // continue;
                    reportedShape = null;
                }
                bool notReported = reportedShape == encodingShape;

                _uxf.result[$"Shape{i}_correct"] = notReported ? 0 : 1;
                _uxf.result[$"Shape{i}_encLoc"] = encodingShape.EncodingIndex;
                _uxf.result[$"Shape{i}_encColour"] = encodingShape.ColorIndex;
                
                _uxf.result[$"Shape{i}_respLoc"] = reportedShape == null ? "nan" : reportedShape.Loc.ToString();
                _uxf.result[$"Shape{i}_respColour"] = reportedShape == null ? "nan" : reportedShape.ColorIndex.ToString();
                _uxf.result[$"Shape{i}_respRank"] = reportedShape == null ? "nan" : reportedShape.ReportedIndex.ToString();
                _uxf.result[$"Shape{i}_rt"] = reportedShape == null ? "nan" : reportedShape.RT.ToString();
            }
            
        }
        
         public static GameObject InstantiateObjectWithMesh(GameObject prefab,
             Transform transform,
             Mesh mesh,
             Vector3 position,
             Vector3 rotation,
             string location)
         {

             //prefab.GetComponent<Stimulus>().session = session;
             //prefab.GetComponent<Stimulus>().location = location;

             GameObject gameObject = Instantiate(
                 prefab,
                 position,
                 Quaternion.Euler(rotation),
                 transform);
             gameObject.GetComponent<MeshFilter>().mesh = mesh;
             //gameObject.transform.localScale = new Vector3(2f, 2f, 2f);
             return gameObject;
         }
         
         GameObject InstantiateObjectWithMeshAndColor(GameObject prefab,
             Mesh mesh,
             Vector3 position,
             Vector3 rotation,
             Color color,
             Vector3? scale = null)
         {
             // scale = Vector3.one;

            Vector3 localScale = scale ?? Vector3.one;

            GameObject gameObject = Instantiate(
                 prefab,
                 position,
                 Quaternion.Euler(rotation),
                 this.transform);
             gameObject.GetComponent<MeshFilter>().mesh = mesh;
             gameObject.GetComponent<Renderer>().material.color = color;
             //gameObject.transform.localScale = new Vector3(2f, 2f, 2f);
             gameObject.SetActive(true);
             gameObject.transform.localScale = localScale;
             return gameObject;
         }

         GameObject InstantiateObject(GameObject prefab,
             Vector3 position,
             Vector3 rotation,
             string location)
         {

             //prefab.GetComponent<Stimulus>().session = session;
             //prefab.GetComponent<Stimulus>().location = location;

             GameObject gameObject = Instantiate(
                 prefab,
                 position,
                 Quaternion.Euler(rotation),
                 this.transform);
             //gameObject.transform.localScale = new Vector3(2f, 2f, 2f);
             return gameObject;
         }

         public void ReportShapeSelected(GameObject selectedShape, bool ignoring)
         {
             if (selectedShape.GetComponent<Renderer>().material.color != shapeSettings.shapeColours[_colorCode])
             {
                 return;
             } 
             // Determine reaction time
             float currentTime = Time.time;
             int reactionTime = (int)((currentTime - responseStartTime) * 1000);
             responseStartTime = currentTime;
             
             ResponseShapeMetadata shapeMetadata = selectedShape.GetComponent<ResponseShapeMetadataObject>().Data;

            if (shapeMetadata.Processed)
            {
                // If we have already processed this, don't do it again
                // Unity will try since the ray could hit twice technically
                return;   
            }

            if (!ignoring)
            {
                shapeMetadata.Processed = true;
                shapeMetadata.RT = reactionTime;
                shapeMetadata.ReportedIndex = reportedIndex;
                Debug.Log($"Adding {shapeMetadata.Correct} to _n_correct = {_n_correct}");
                _n_correct += shapeMetadata.Correct;
            }
             
             
             Tuple<int, int> shapePair = shapeMetadata.GetShapePair();
             // Debug.Log($"shape pos: {shapeMetadata.}");
             
             // Destroy same colors and shapes
             for (int i = 0; i != shapeSettings.count; ++i)
             {
                //  Debug.Log($"Destroying ({shapePair.Item1}, {i}) and ({i}, {shapePair.Item2})");
                 Destroy(this.reportingStimuli[(shapePair.Item1, i)]); // 0,0, 0,1
                 Destroy(this.reportingStimuli[(i, shapePair.Item2)]); // 0,0, 1,0
             }

             // Log everything to the results dictionary
             foreach (KeyValuePair<string, string> kvp in shapeMetadata.ToDictionary(reportedIndex))
             {
                 _uxf.result[kvp.Key] = kvp.Value; 
             }
             
             // Update the shape we're reporting on for the next time
             reportedIndex++;
         }
         
    }
}