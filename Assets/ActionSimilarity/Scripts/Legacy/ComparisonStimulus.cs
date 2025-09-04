using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UXF;

namespace ActionSimilarity {
    public class ComparisonStimulus : Stimulus {

        // Start is called before the first frame update
        void Start() {
            StartCoroutine(Countdown());
        }

        // Update is called once per frame
        void Update() {
        }

        public void makeChoice(int choice) {
            StopCoroutine(Countdown());
            GetComponent<MeshFilter>().mesh = null;
            LogChoice(choice);
            NextTrial();
        }

        void LogChoice(int choice) {
            session.CurrentTrial.result["PerceptionChoice"] = choice;
            session.CurrentTrial.result["PerceptionChoiceTime"] = Time.time;
        }

        IEnumerator Countdown() {
            yield return new WaitForSeconds(session.CurrentTrial.settings.GetFloat("decisionTime"));
            NextTrial();
        }

        void NextTrial() {
            if (session.CurrentTrial == session.LastTrial) {
                session.EndCurrentTrial();
            } else {
                session.BeginNextTrial();
            }
        }
    }
}