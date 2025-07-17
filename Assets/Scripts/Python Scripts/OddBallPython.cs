using System.Collections;
using System.Collections.Generic;
//using UnityEngine;
//using Python.Runtime;
//using UnityEditor.Scripting.Python;

//using System;


public class OddBall : UnityEngine.MonoBehaviour {

    public UnityEngine.GameObject[] letters;
    public UnityEngine.GameObject[] digits;

    public float ISI;
    public float ISIasync;
    public float stimulusDuration;


    UnityEngine.GameObject currentStimulus;
    //Renderer m_renderer;

    float currentStimulusDuration;
    float currentISI;
    float currentISIasync;

    public int numTrials; // number of deviants PER BLOCK (x2 for entire session)
    public int trialLength;

    private bool letterFrquents;
                        


    // Start is called before the first frame update
    void Start() {
        currentStimulusDuration = 0.0f;
        currentISI = 0.0f;
        UnityEngine.Random.InitState(System.DateTime.Now.GetHashCode());
        UnityEngine.Debug.Log(stimulusDuration);
        UnityEngine.Debug.Log(ISI);
        letterFrquents = true;
        //numTrials = 150; // should be 150 to get 300 in one session

        //PythonRunner.EnsureInitialized();
        //using (Py.GIL()) {
        //    try {
        //        dynamic sys = PythonEngine.ImportModule("sys");
        //        dynamic psychopy = PythonEngine.ImportModule("psychopy");
        //        UnityEngine.Debug.Log($"python version: {sys.version}"); 
        //    }
        //    catch (PythonException e) {
        //        UnityEngine.Debug.LogException(e);
        //    }
        //}
    }

    // Update is called once per frame
    void Update() {
        for (int blockNum = 0; blockNum < 2; blockNum++) {
            UnityEngine.GameObject[][] block = GetBlock();
            UnityEngine.Debug.Log("Block: " + blockNum);

            for (int trialInd = 0; trialInd < numTrials; trialInd++) {
                UnityEngine.GameObject[] trial = block[trialInd];
                int stim = 0;
                UnityEngine.Debug.Log("Trial: " + trialInd);

                foreach (UnityEngine.GameObject stimulus in trial) {
                    UnityEngine.Debug.Log("Trial: " + trialInd + ", Stim: " + stim++);

                    if (currentStimulusDuration > 0
                        || (currentStimulusDuration == 0.0f && currentISI == 0.0f)) {
                        currentStimulusDuration += UnityEngine.Time.deltaTime;
                        if (currentStimulusDuration >= stimulusDuration) {
                            UnityEngine.Debug.Log("Duration: " + currentStimulusDuration);

                            foreach (UnityEngine.Transform child in this.transform) {
                                UnityEngine.GameObject.Destroy(child.gameObject);
                            }
                            currentStimulusDuration = 0.0f;
                            currentISI += UnityEngine.Time.deltaTime;
                            currentISIasync = UnityEngine.Random.Range(-ISIasync, ISIasync);
                            //Debug.Log("current ISI after destroy: " + currentISI);
                        }
                    } else {
                        currentISI += UnityEngine.Time.deltaTime;
                        if (currentISI >= (ISI + currentISIasync)) {
                            UnityEngine.Debug.Log("ISI: " + currentISI);
                            currentISI = 0.0f;
                            currentStimulus = Instantiate(
                            stimulus,
                            new UnityEngine.Vector3(2.59f, 1.029f, -26.53f),
                            UnityEngine.Quaternion.Euler(0.0f, -86.0f, 0.0f),
                            this.transform
                            );

                            currentStimulus.transform.localScale = new UnityEngine.Vector3(60, 60, 60);
                        }
                    }
                }
            }


            letterFrquents = false;
            if (blockNum == 2) {
                UnityEngine.Application.Quit();
            }
            
        }

        


    }

    //void updateGameObject()
    //{
    //    DestroyImmediate(currentStimulus);
    //    instatiateStimulus();
    //}

    void ExecuteTrial(UnityEngine.GameObject[] trial) {
        

    }

    UnityEngine.GameObject[][] GetBlock() {
        UnityEngine.GameObject[] deviants;
        UnityEngine.GameObject[] frequents;

        UnityEngine.GameObject[][] trialList = new UnityEngine.GameObject[numTrials][];

        for (int trialInd = 0; trialInd != numTrials; trialInd++) {
            if (letterFrquents) {
                deviants = digits;
                frequents = letters;
            } else {
                deviants = letters;
                frequents = digits;
            }

            // Randomize deviant spot
            int deviantSpot = UnityEngine.Random.Range(0, trialLength);

            UnityEngine.GameObject[] trial = new UnityEngine.GameObject[trialLength];

            for (int i = 0; i != trialLength; i++) {
                if (i == deviantSpot) {
                    trial[i] = deviants[UnityEngine.Random.Range(0, deviants.Length)];
                } else {
                    trial[i] = frequents[UnityEngine.Random.Range(0, frequents.Length)];
                }
            }

            trialList[trialInd] = trial;

        }

        return trialList;

    }
}
