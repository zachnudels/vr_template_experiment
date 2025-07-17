using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Python.Runtime;
//using UnityEditor.Scripting.Python;

using System;
using UXF;
using Random = UnityEngine.Random;

namespace Oddball {

    public class OddBall : MonoBehaviour {

        public Session session;

        public GameObject[] letters;
        public GameObject[] digits;




        //public float ISI;
        //public float ISIasync;
        //public float stimulusDuration;


        GameObject currentStimulus;
        ////Renderer m_renderer;

        //float currentStimulusDuration;
        //float currentISI;
        float currentISIasync;

        Trial trial;

        //public int numTrials; // number of deviants PER BLOCK (x2 for entire session)
        //public int trialLength;
        //public int numBlocks;

        //private bool letterFrquents;

        ////int blockNum;
        ////int trialInd;

        //bool presentation;

        //public float IBI;





        // Start is called before the first frame update
        void Start() {
        }

        private void Awake() {
            
        }


        void DestroyStimulus() {
            foreach (Transform child in this.transform) {
                Destroy(child.gameObject);
            }
            

        }

        IEnumerator DisplayStimulus(GameObject stimulus) {
            currentStimulus = Instantiate(
                                stimulus,
                                new Vector3(2.59f, 1.029f, -26.53f),
                                Quaternion.Euler(0.0f, -86.0f, 0.0f),
                                this.transform
                                );


            yield return new WaitForSeconds(session.settings.GetFloat("duration"));
            DestroyStimulus();


            // get input
            Debug.Log(currentISIasync);
            yield return new WaitForSeconds(currentISIasync);
            session.BeginNextTrial();


        }

        // assign this method to the Session OnTrialBegin UnityEvent in its inspector
        public void BuildAndRunTrial(Trial trial) {

            //Debug.Log("Running Trial");
            this.trial = trial;

            float ISIasync = session.settings.GetFloat("ISIAsync");

            currentISIasync = session.settings.GetFloat("ISI") + Random.Range(-ISIasync, ISIasync);
            RareStimulus rare = (RareStimulus)trial.settings.GetObject("rareStim");
            GameObject deviant;
            GameObject frequent;

            if (rare == RareStimulus.Digit) {
                deviant = digits[Random.Range(0, digits.Length)];
                frequent = letters[Random.Range(0, letters.Length)];
            } else {
                deviant = letters[Random.Range(0, letters.Length)];
                frequent = digits[Random.Range(0, digits.Length)];
            }
            float prob = Random.Range(0.0f, 1.0f);

            if (prob > 0.8) {
                StartCoroutine(DisplayStimulus(deviant));
            } else {
                StartCoroutine(DisplayStimulus(frequent));
            }

            
            
            
            

            
        }

        // this could trigger on some user behaviour (e.g. button response), collecting their score in a task
        public void RecordResultsAndEnd(int score) {
            // store their score
            session.CurrentTrial.result["score"] = score;
            // end this trial
            session.CurrentTrial.End();
        }


        // Update is called once per frame
        void Update() {
            if (Input.GetKeyDown("z")) {
                
            }

        }


        //IEnumerator PresentStimulus(GameObject stimulus) {

        //    yield return new WaitForSeconds(stimulusDuration);
        //}

        //void updateGameObject()
        //{
        //    DestroyImmediate(currentStimulus);
        //    instatiateStimulus();
        //}

        //GameObject[] buildTrial() {
        //    GameObject[] deviants;
        //    GameObject[] frequents;

        //    RareStimulus deviant = (RareStimulus)trial.settings.GetObject("rareStim");
        //    int trialLength = trial.settings.GetInt("trialLength");

        //    if (deviant == RareStimulus.Digit) {
        //        deviants = digits;
        //        frequents = letters;
        //    } else {
        //        deviants = letters;
        //        frequents = digits;
        //    }

        //    // Randomize deviant spot
        //    int deviantSpot = Random.Range(0, trialLength);

        //    GameObject[] stimuli = new GameObject[trialLength];

        //    for (int i = 0; i != trialLength; i++) {
        //        if (i == deviantSpot) {
        //            stimuli[i] = deviants[Random.Range(0, deviants.Length)];
        //        } else {
        //            stimuli[i] = frequents[Random.Range(0, frequents.Length)];
        //        }
        //    }

        //    return stimuli;
        //}


    }
}
