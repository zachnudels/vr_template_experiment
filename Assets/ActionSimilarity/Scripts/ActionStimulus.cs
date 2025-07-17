using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UXF;

namespace ActionSimilarity {
    public class ActionStimulus : Stimulus {

        public bool actedOn;
        public int incorrectTries;
        public ProgressBar progressBar;
        public float incorrectActionFactor;
        bool logged;

        private void Start() {
            actedOn = false;
            incorrectTries = 0;
            logged = false;
        }

        void Update() {
            // For debugging
            if (Input.GetKeyDown("z")) {
                if (tag == "LeftHand") {
                    Conseqeunce();
                    LogResult(true);
                }
                else {
                    WrongAction();
                }
            } else if (Input.GetKeyDown("/")) {
                if (tag == "RightHand") {
                    Conseqeunce();
                    LogResult(true);
                } else {
                    WrongAction();
                }
            }
        }


        private void OnTriggerEnter(Collider other) {
            if (other.tag.Equals(tag)) {
                LogResult(true);
                Conseqeunce();
            } else {
                WrongAction();
            }

        }

        void WrongAction() {
            incorrectTries += 1;
            progressBar.UpdateLevel(-incorrectActionFactor);
        }

        void LogResult(bool success) {
            logged = true;
            session.CurrentTrial.result["Action" + location + "IncorrectTries"] = incorrectTries;
            session.CurrentTrial.result["Action" + location + "Success"] = success;
            session.CurrentTrial.result["Action" + location + "FinalTime"] = Time.time;
        }

        private void OnDestroy() {
            if (!logged) LogResult(false);
        }


        private void Conseqeunce() {
            if (!actedOn) {
                TriangleExplosion explosion = gameObject.AddComponent<TriangleExplosion>();
                StartCoroutine(explosion.SplitMesh(true, session.settings.GetFloat("animationTime")));
                actedOn = true;
            }
        }

    }
}