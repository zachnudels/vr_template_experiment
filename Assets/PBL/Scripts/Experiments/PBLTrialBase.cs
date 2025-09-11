using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ActionSimilarity;
using PBL.DataHandler;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using UXF;
using TMPro;
using PBL.Types;
using PBL.TrialComponents;

namespace PBL.Experiments
{
    
    public abstract class PBLTrialBase : MonoBehaviour
    {
        public Session session;
        
        private Dictionary<string, int> codeMap;
        private bool simulating; // from exp
        
        public TextController textControllerWall;

        [SerializeField] protected TrialSettings settings; 

        [HideInInspector] public bool pause;

        [HideInInspector] public Transform FixationTf;

        // [HideInInspector] public bool ShouldShowRay => _stageName == "Report";
        [HideInInspector] public event Action<string> StageChanged;


        // public bool initializedEnvironment = false;

        FaceDirection faceDirection;
        protected TurnDirection _turnDirection;
        Transform eyes;
        string hand;
        private FixationRotator fixationRotator;
        protected bool debug;
        protected bool _randomSimulationDebug;

        protected Dictionary<(int, int), GameObject> reportingStimuli;
        protected int reportedIndex;
        protected UXF.Trial _uxf;
        protected float responseStartTime;

        bool firstTrial;
        bool sessionStart;

        protected string _stageName;
        protected List<ResponseShapeMetadata> _reportedStimuli;

        protected int _n_correct;
        protected FaceDirection _start_face_direction;

        protected GameObject _fixationSphere;

        

        void Awake()
        {
            settings.shapeSettings.Init();
            codeMap = settings.codes.ToDictionary(e => e.key, e => e.value);
            
        }


        // Start is called before the first frame update
        void Start()
        {

            Application.targetFrameRate = 90;

            // Find eyes depending on simulation or not
            SetEyes();

            _fixationSphere = InstantiateObject(settings.fixationSettings.fixationSphere,
                settings.fixationSettings.fixationSphere.transform.position,
                new Vector3(0.0f, 0.0f, 0.0f),
                "fixation"
            );

            FixationTf = _fixationSphere.transform;

            settings.fixationSettings.textMeshPro = _fixationSphere.GetComponentInChildren<TextMeshPro>();
            settings.fixationSettings.textMeshPro.enabled = false;
            _stageName = "none";

            faceDirection = FaceDirection.Front;
            
        }

        void SetEyes()
        {
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
            if (_randomSimulationDebug)
            {
                pause = false;
            }
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
            debug = _uxf.settings.GetBool("debug");
            _randomSimulationDebug = _uxf.settings.GetBool("randomSimulationDebug");
            simulating = _uxf.settings.GetBool("simulating");


            settings.shapeSettings.shapePositions = _uxf.settings.GetIntList("shapePositions");
            settings.shapeSettings.colorPositions = _uxf.settings.GetIntList("colorPositions");
            settings.shapeSettings.colorCols = _uxf.settings.GetIntList("colorCols");
            settings.shapeSettings.shapeRows = _uxf.settings.GetIntList("shapeRows");
            
            _turnDirection = (TurnDirection)_uxf.settings.GetObject("turnDirection");

            // TODO: write results 

            // _uxf.result["ConditionCode"] = _turnDirection == TurnDirection.Left ? 1 : 2;
            _uxf.result["ConditionTurn"] = _turnDirection.ToString().ToLower();
            _uxf.result["HeightOffset"] = eyes.position.y;
            _uxf.result["Facing"] = faceDirection.ToString().ToLower();

            ExtractFurtherSettings();

            settings.fixationSettings.turnTime = _uxf.settings.GetFloat("turnTime");
            fixationRotator = new FixationRotator(settings.fixationSettings.turnTime);

        }

        protected virtual void ExtractFurtherSettings()
        {}

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
            
            //actionsDone = false;
            //showingTarget = false;
            pause = false;
            firstTrial = false;
            sessionStart = false;

            // Change which way we are facing
            faceDirection = _uxf.number % 2 == 0 ? FaceDirection.Back : FaceDirection.Front;
            _start_face_direction = faceDirection;

            // If we are facing the front, negative z is further from us, so go from biggest to smallest 
            // Otherwise, go from smallest (negative) to biggest
            if (faceDirection == FaceDirection.Front)
            {
                Array.Sort(settings.shapeSettings.encodingShapePositions, (a, b) => b.z.CompareTo(a.z));
            }
            else
            {
                Array.Sort(settings.shapeSettings.encodingShapePositions, (a, b) => a.z.CompareTo(b.z));
            }

            textControllerWall.ChangeWall(faceDirection);

            _reportedStimuli = new List<ResponseShapeMetadata>();

            reportedIndex = 1;

            _n_correct = 0;
        }


        public void setTrigger(int code = 0)
        {
            int turnCondition = (_turnDirection == TurnDirection.Left) ? 0 : 1;
            int faceCondition = (_start_face_direction == FaceDirection.Front) ? 0 : 1;
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
        public virtual void BuildAndRunTrial(UXF.Trial trial)
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

        protected virtual IEnumerator RunTrial()
        {
            Debug.Log("Starting Trial");
            if (_uxf.numberInBlock == 1)
            {
                
                _fixationSphere.SetActive(false);
                Debug.Log("First in block");
                firstTrial = true;
                if (_uxf.number == 1)
                {
                    Debug.Log("Starting Session");
                    yield return RunStage(StartSession);

                }
                yield return RunStage(CalibrateEyes);
            }
            
            yield return RunStage(WaitToStart);  // preparation + jitter 
            
            yield return RunStage(Turn);  // Turn  

            yield return RunStage(Report);  // answer

            LogResults();

            yield return RunStage(Feedback);
            
            if (_uxf == session.LastTrial)
            {
                yield return RunStage(EndSession);
            }

            if (_uxf == session.CurrentBlock.lastTrial)
            {
                yield return RunStage(Break);
            }

            session.CurrentTrial.End();
            session.NextTrial.Begin();

        }



        IEnumerator RunStage(Func<IEnumerator> stage)
        {
            _stageName = stage.Method.Name;
            UnityEngine.Debug.Log(_stageName);
            textControllerWall.Debug(_stageName);

            // Invoke the method for any subscriber listening for stage changes
            StageChanged?.Invoke(_stageName);
            // code = stageName.GetHashCode();
            StageChanged?.Invoke(_stageName);
            yield return StartCoroutine(stage());
            textControllerWall.Clear();
        }

        /*
        * Trial Stages
        */

        IEnumerator CalibrateEyes() {
            textControllerWall.Write($"Pull the trigger to start calibration.");
            
            pause = true;
            yield return new WaitUntil(() => !pause);

            if (!debug)
            {
                bool calibrated = ViveSR.anipal.Eye.SRanipal_Eye.LaunchEyeCalibration();
                while (!calibrated)
                {
                    calibrated = ViveSR.anipal.Eye.SRanipal_Eye.LaunchEyeCalibration();
                }
                yield return new WaitUntil(() => calibrated);
            }

            
            _fixationSphere.transform.position = eyes.position + new Vector3(0.0f, settings.fixationSettings.downOffset, settings.fixationSettings.FixationDepth * (int)faceDirection);
            _fixationSphere.SetActive(true);

            textControllerWall.Write($"Pull the trigger to start the block.");
            
            pause = true;
            yield return new WaitUntil(() => !pause);
        }

        IEnumerator StartSession()
        {
            // condition = session.CurrentBlock.settings.GetInt("condition");
            textControllerWall.Write($"Welcome! We hope you enjoy the experiment."
                                    + "\n\nMake sure this text is clear (move the headset up and down to clarify and then tighten it at the back)." 
                                    + "\n\n Pull the trigger to continue.");
            // If first trial of session, wait for trigger to start

            sessionStart = true;

            SetEyes();            

            pause = true;
            yield return new WaitUntil(() => !pause);
            textControllerWall.Debug("Unpaused");


        }

        IEnumerator Instructions()
        {
            UnityEngine.Debug.Log("Running Instructions");
            textControllerWall.Write("Always follow the fixation ball!" 
                                     + "\n\nTry remember the blue shape and select the shape after your turn by pointing the laser"
                                     +"\n\nPull the trigger to continue");
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
            _fixationSphere.SetActive(false);
            textControllerWall.Write("Well done! Time to take a well deserved break"
                            + " \n\n Are you standing on the line?"
                            + " \n\n Pull the trigger when you're ready to start the next block (calibration first again ;) )");

            yield return new WaitForSeconds(3f);
            pause = true;
            yield return new WaitUntil(() => !pause);

            textControllerWall.Clear();
            yield return new WaitForSeconds(1f);

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

            GameObject[] stimuli = InstantiateEncodingStimuli(false);

            while (!fixationRotator.isFinished())
            {
                float dt = Time.deltaTime;
                elapsed += dt;
                fixationRotator.Step(dt); 
                _fixationSphere.transform.position = fixationRotator.GetCurrentPosition(
                    settings.fixationSettings.FixationDepth, _fixationSphere.transform.position, faceDirection);
                
                if (!triggerSent && elapsed >= halfwayTime)
                {
                    
                    // Debug.Log(fixationRotator.LogString());
                    setTrigger(codeMap["halfway_turn"]);
                    triggerSent = true;
                    StartCoroutine(ShowEncodingShapesForOneFrame(stimuli));
                }

                yield return null;
            }

            foreach (GameObject gameObject in stimuli)
            {
                Destroy(gameObject);
            }
            
            faceDirection = textControllerWall.ChangeWall(faceDirection, true);
        }

        IEnumerator ShowEncodingShapesForOneFrame(GameObject[] stimuli)
        {
            // Debug.Log($"{faceDirection}, {_turnDirection}");
            // Debug.Log(settings.fixationSettings.fixationSphere.transform.position);
            for (int i = 0; i != stimuli.Length; ++i)
            {
                // Debug.Log($"{faceDirection}, {_turnDirection}");
                // Debug.Log(settings.shapeSettings.encodingShapePositions[i]);

                Vector3 position = settings.shapeSettings.encodingShapePositions[i]
                                   + _fixationSphere.transform.position;
                GameObject gameObject = stimuli[i];
                gameObject.transform.position = position;
                // Debug.Log(position);

                gameObject.SetActive(true);
            }

            yield return new WaitForEndOfFrame();
            
            foreach (GameObject gameObject in stimuli)
            {
                gameObject.SetActive(false);
            }
            setTrigger(codeMap["remove_shapes"]);

        }


        IEnumerator Report()
        {

            // pause = true;
            // yield return new WaitUntil(() => !pause);

            setTrigger(codeMap["start_answer"]);

            _fixationSphere.SetActive(false);

            this.reportingStimuli = InstantiateReportStimuli();



            float startTime = Time.time;
            this.responseStartTime = startTime;

            // yield return new WaitForSeconds((session.CurrentTrial.settings.GetFloat("reportTime")));

            // pause = true;

            if (!_randomSimulationDebug)
            {
                yield return new WaitUntil(() => reportedIndex == settings.expectedResponses + 1);
            }
            else
            {
                SimulateReporting();
            }

            ReportHook();

            if (this.reportingStimuli == null)
            {
                yield break;
            }

            setTrigger(codeMap["end_answer"]);

            float endTime = Time.time;

            yield return null;
            
            
            _fixationSphere.SetActive(true);

            _uxf.result["Score"] = _n_correct;
            _uxf.result["RT"] = endTime - startTime;

        }

        protected virtual void ReportHook()
        {}

        protected virtual void SimulateReporting()
        {
            foreach (Color color in settings.shapeSettings.shapeColours)
            {
                List<GameObject> coloredObjs = reportingStimuli.Values.Where(obj =>
                {
                    Renderer renderer = obj.GetComponent<Renderer>();
                    return renderer != null && renderer.material.color == color;
                }).ToList();
                GameObject randomReportedObj = coloredObjs[UnityEngine.Random.Range(0, coloredObjs.Count)];
                ReportShapeSelected(randomReportedObj, false);
            }
        }

        IEnumerator Feedback()
        {

            settings.fixationSettings.textMeshPro.text = _n_correct.ToString();

            float feedbackRotation = (faceDirection == FaceDirection.Front) ? 0f : 180f;
            settings.fixationSettings.textMeshPro.transform.rotation = Quaternion.Euler(new Vector3(0f, feedbackRotation, 0f));

            settings.fixationSettings.textMeshPro.enabled = true;

            yield return new WaitForSeconds(session.CurrentTrial.settings.GetFloat("feedbackTime"));
            
            settings.fixationSettings.textMeshPro.enabled = false;

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

        /// Called once per encoding item during instantiation.
        /// Children can override this to capture special codes, adjust data, etc.
        protected virtual void OnEncodingItemCreated(int i, GameObject[] stimuli)
        {}

        protected virtual GameObject[] InstantiateEncodingStimuli(bool active)
        {

            GameObject[] stimuli = new GameObject[settings.shapeSettings.count];
            // Debug.Log($"shapeSettings.count: {settings.shapeSettings.count}");
            // Debug.Log($"colorPositions.Length: {settings.shapeSettings.colorPositions.Count}");
            // Debug.Log($"shapeColours.Length: {settings.shapeSettings.shapeColours.Length}");
            // Debug.Log($"shapePositions.Length: {settings.shapeSettings.shapePositions.Count}");
            // Debug.Log($"shapeMeshes.Length: {settings.shapeSettings.shapeMeshes.Length}");
            // Debug.Log($"encodingShapePositions.Length: {settings.shapeSettings.encodingShapePositions.Length}");

            for (int i = 0; i < settings.shapeSettings.count; i++)
            {
                // Debug.Log($"i: {i}, colorIndex: {settings.shapeSettings.colorPositions[i]}, meshIndex: {settings.shapeSettings.shapePositions[i]}");

                if (settings.shapeSettings.colorPositions[i] >= settings.shapeSettings.shapeColours.Length)
                {
                    Debug.Log($"Invalid color index {settings.shapeSettings.colorPositions[i]} at i={i}");
                }

                if (settings.shapeSettings.shapePositions[i] >= settings.shapeSettings.shapeMeshes.Length)
                {
                    Debug.Log($"Invalid mesh index {settings.shapeSettings.shapePositions[i]} at i={i}");
                }

                Color color = settings.shapeSettings.shapeColours[settings.shapeSettings.colorPositions[i]];
                Mesh mesh = settings.shapeSettings.shapeMeshes[settings.shapeSettings.shapePositions[i]];
                Vector3 rotation = settings.shapeSettings.encodingShapeRotationMap.TryGetValue(mesh.name, out var rot) ? rot : Vector3.zero;
                Vector3 scale = settings.shapeSettings.shapeScaleMap.TryGetValue(mesh.name, out var scal) ? scal : Vector3.one;

                // Change to x rotation since we're sideways now
                // Debug.Log(faceDirection.ToString() + " " + _turnDirection.ToString());
                // rotation = new Vector3(rotation.z, rotation.y, rotation.x);
                Vector3 position = settings.shapeSettings.encodingShapePositions[i]
                                   + _fixationSphere.transform.position;
                stimuli[i] = InstantiateObjectWithMeshAndColor(settings.shapeSettings.encodingShape,
                    mesh,
                    position,
                    rotation,
                    color,
                    active,
                    scale
                );

                OnEncodingItemCreated(i, stimuli);

                Debug.Log($"Encoding object {i} world bounds size: {stimuli[i].GetComponent<Renderer>().bounds.size}, center: {stimuli[i].GetComponent<Renderer>().bounds.center}");

            }

            return stimuli;
        }

        
        protected virtual Color SetColor(int i)
        {
            Color color = settings.shapeSettings.shapeColours[i];
            return color;
        }
        

        Dictionary<(int, int), GameObject> InstantiateReportStimuli()
        {
            Dictionary<(int, int), GameObject> stimuli = new Dictionary<(int, int), GameObject>();
            
            Dictionary<(int, int), int> pairIndex = new Dictionary<(int, int), int>();
            for (int i = 0; i < settings.shapeSettings.count; i++)
            {
                pairIndex[(settings.shapeSettings.colorPositions[i], settings.shapeSettings.shapePositions[i])] = i;
            }
            
            for (int colorRow = 0; colorRow != settings.shapeSettings.count; ++colorRow)
            {
                for (int shapeCol = 0; shapeCol != settings.shapeSettings.count; ++shapeCol)
                {
                    int posIndex = colorRow + shapeCol + (colorRow * (settings.shapeSettings.count - 1));
                    int color_i = settings.shapeSettings.shapeRows[colorRow];
                    int shape_i = settings.shapeSettings.colorCols[shapeCol];
                    // Debug.Log($"Shape pos: {posIndex} colorI: {color_i}, shape_i: {shape_i}");

                    Color color = SetColor(color_i);

                    Mesh mesh = settings.shapeSettings.shapeMeshes[shape_i];
                    Vector3 rotation = settings.shapeSettings.reportingShapeRotationMap.TryGetValue(mesh.name, out var rot) ? rot : Vector3.zero;
                    Vector3 position = settings.shapeSettings.reportingShapePositions[posIndex]
                                       + _fixationSphere.transform.position;
                    Vector3 scale = settings.shapeSettings.shapeScaleMap.TryGetValue(mesh.name, out var scal) ? scal : Vector3.one;


                    GameObject shape = InstantiateObjectWithMeshAndColor(settings.shapeSettings.reportingShape,
                        mesh,
                        position,
                        rotation,
                        color,
                        true,
                        scale * settings.shapeSettings.reportingShapeScale
                    ); ;
                    
                    // Store shapeData object for later processing
                    
                    // Find position of this color/shape in encoding array
                    int colorEncIndex = settings.shapeSettings.colorPositions.IndexOf(color_i);
                    int shapeEncIndex = settings.shapeSettings.shapePositions.IndexOf(shape_i);
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

        protected virtual string ReportCorrectEnc(int i, bool notReported)
        {
            // Check what it is otherwise
            // Single color specific. Only the specific chosen color can be reported on
            bool canBeReported = settings.shapeSettings.colorPositions[i] == 0;
            // Debug.Log($"Can be reported :{canBeReported}, code: {settings.shapeSettings.colorPositions[i]}");
            string correct = "nan";
            if (canBeReported && notReported)
            {
                correct = "0";
            } else if (canBeReported) {
                correct = "1";
            }
            
            return correct;
        }


       
        protected virtual (string, ResponseShapeMetadata, bool) ReportCorrectColor(int i, ResponseShapeMetadata encodingShape)
        {
            // //TODO See what it is normally
            bool canBeReported = i == 0;
            // Single color specific. Only the specific chosen color can be reported on 
            ResponseShapeMetadata reportedShape = _reportedStimuli
                .FirstOrDefault(item => item.ColorIndex == i && item.RT != -1);
            if (reportedShape == null && canBeReported)
            {
                throw new UnityException($"Could not find reported stimuli with color index {i}");
            }
            bool notReported = reportedShape == encodingShape;
            
            string correct = "nan";
            if (canBeReported && notReported)
            {
                correct = "0";
            } else if (canBeReported) {
                correct = "1";
            }
            
            return (correct, reportedShape, notReported);
        }

        protected virtual string ReportCorrectShape(int i, bool notReported)
        {
                
            // Single color specific. Only the specific chosen color can be reported on 
                
            bool canBeReported = i == 0;
            string correct = "nan";
            if (canBeReported && notReported)
            {
                correct = "0";
            } else if (canBeReported) {
                correct = "1";
            }
            
            return correct;
        }


        void LogResults()
        {
            
            for (int i = 0; i != settings.shapeSettings.count; ++i)
            {
                // Get the response stimulus corresponding to this encoding stimulus
                ResponseShapeMetadata response = _reportedStimuli
                    .FirstOrDefault(item => item.EncodingIndex == i);
                if (response == null)
                {
                    throw new UnityException($"Could not find reported stimuli with encoding index {i}");
                }
                bool notReported = response.RT == -1;

                _uxf.result[$"Enc{i + 1}_rank"] = notReported ? "nan" : response.ReportedIndex.ToString();
                _uxf.result[$"Enc{i + 1}_correct"] = ReportCorrectEnc(i, notReported);
                _uxf.result[$"Enc{i + 1}_colour"] = settings.shapeSettings.colorPositions[i];
                _uxf.result[$"Enc{i + 1}_shape"] = settings.shapeSettings.shapePositions[i];
                _uxf.result[$"Enc{i + 1}_respLoc"] = response.Loc;
                _uxf.result[$"Enc{i + 1}_rt"] = notReported ? "nan" : response.RT.ToString();
            }
            
            for (int i = 0; i != settings.shapeSettings.count; ++i)
            {
                ResponseShapeMetadata encodingShape = _reportedStimuli
                    .FirstOrDefault(item => item.ColorIndex == i && item.EncodingIndex != -1);
                if (encodingShape == null)
                {
                    throw new UnityException($"Could not find encoding stimuli with color index {i}");
                }

                (string correct, ResponseShapeMetadata reportedShape, bool notReported) = ReportCorrectColor(i, encodingShape);

                _uxf.result[$"Colour{i+1}_correct"] = correct;
                _uxf.result[$"Colour{i+1}_encLoc"] = encodingShape.EncodingIndex;
                _uxf.result[$"Colour{i+1}_encShape"] = encodingShape.ShapeIndex;
                _uxf.result[$"Colour{i+1}_respLoc"] = (reportedShape == null) ? "nan" : reportedShape.Loc.ToString();
                _uxf.result[$"Colour{i+1}_respShape"] = (reportedShape == null) ? "nan" : reportedShape.ShapeIndex.ToString();
                _uxf.result[$"Colour{i+1}_respRank"] = (reportedShape == null) ? "nan" : reportedShape.ReportedIndex.ToString();
                _uxf.result[$"Colour{i+1}_rt"] = (reportedShape == null) ? "nan" : reportedShape.RT.ToString();
            }
            
            for (int i = 0; i != settings.shapeSettings.count; ++i)
            {
                ResponseShapeMetadata encodingShape = _reportedStimuli
                    .FirstOrDefault(item => item.ShapeIndex == i && item.EncodingIndex != -1);
                if (encodingShape == null)
                {
                    throw new UnityException($"Could not find encoding stimuli with shape index {i}");
                }
                
                
                ResponseShapeMetadata reportedShape = _reportedStimuli
                    .FirstOrDefault(item => item.ShapeIndex == i && item.RT != -1);

                bool notReported = reportedShape != null && reportedShape == encodingShape;
                
                _uxf.result[$"Shape{i+1}_correct"] = ReportCorrectShape(i, notReported);
                _uxf.result[$"Shape{i+1}_encLoc"] = encodingShape.EncodingIndex;
                _uxf.result[$"Shape{i+1}_encColour"] = encodingShape.ColorIndex;
                
                _uxf.result[$"Shape{i+1}_respLoc"] = (reportedShape == null) ? "nan" : reportedShape.Loc.ToString();
                _uxf.result[$"Shape{i+1}_respColour"] = (reportedShape == null) ? "nan" : reportedShape.ColorIndex.ToString();
                _uxf.result[$"Shape{i+1}_respRank"] = (reportedShape == null) ? "nan" : reportedShape.ReportedIndex.ToString();
                _uxf.result[$"Shape{i+1}_rt"] = (reportedShape == null) ? "nan" : reportedShape.RT.ToString();
            }
            
        }
        
         
         GameObject InstantiateObjectWithMeshAndColor(GameObject prefab,
             Mesh mesh,
             Vector3 position,
             Vector3 rotation,
             Color color,
             bool active = true,
             Vector3? scale = null
            )
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
             gameObject.SetActive(active);
             
             gameObject.transform.localScale = localScale;
             FitBoxToMesh(gameObject);
             return gameObject;
         }
         
         public static void FitBoxToMesh(GameObject go)
         {
             var mf  = go.GetComponent<MeshFilter>();
             var boxes = go.GetComponentsInChildren<BoxCollider>(true);
             foreach (var box in boxes)
             {
                 if (!mf || !box || mf.sharedMesh == null) return;

                 // Mesh bounds are in the mesh's LOCAL space, perfect for the collider's local size/center
                 var mb = mf.sharedMesh.bounds;
                 box.center = mb.center;
                 box.size = mb.size;
             }
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
                Debug.Log($"encodingIndex = {shapeMetadata.EncodingIndex}");
                Debug.Log($"Adding {shapeMetadata.Correct} to _n_correct = {_n_correct}");

                setTrigger(codeMap["answer_i"] + ((reportedIndex-1) * 100));

                _n_correct += shapeMetadata.Correct;

                // Log everything to the results dictionary
                foreach (KeyValuePair<string, string> kvp in shapeMetadata.ToDictionary(reportedIndex))
                {
                    _uxf.result[kvp.Key] = kvp.Value;
                    //Debug.Log($"{kvp.Key}: {kvp.Value}");
                }

                // Update the shape we're reporting on for the next time
                reportedIndex++;
            }
            else
            {
                // Log everything to the results dictionary
                foreach (KeyValuePair<string, string> kvp in shapeMetadata.ToDictionary(reportedIndex))
                {
                    _uxf.result[kvp.Key] = "nan";
                }
                reportedIndex++;
            }
             
             Tuple<int, int> shapePair = shapeMetadata.GetShapePair();
             // Debug.Log($"shape pos: {shapeMetadata.}");
             
             // Destroy same colors and shapes
             for (int i = 0; i != settings.shapeSettings.count; ++i)
             {
                if (this.reportingStimuli[(shapePair.Item1, i)] != null)
                {
                    //  Debug.Log($"Destroying ({shapePair.Item1}, {i}) and ({i}, {shapePair.Item2})");
                    Destroy(this.reportingStimuli[(shapePair.Item1, i)]); // 0,0, 0,1
                }
                if (this.reportingStimuli[(i, shapePair.Item2)] != null)
                {
                    Destroy(this.reportingStimuli[(i, shapePair.Item2)]); // 0,0, 1,0
                }
             }
             
         }
         
    }
}