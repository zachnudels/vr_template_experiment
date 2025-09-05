using System.Collections;
using ActionSimilarity;
using PBL;
using PBL.Experiments;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PBl.Legacy
{
    public class Actions : MonoBehaviour
    {
        [Header("Input Actions")]
        public InputActionProperty yesAction;
        public InputActionProperty noAction;

        public PBLTrialBase trial;

        private ComparisonStimulus target;

        void Update()
        {
            GameObject[] targets = GameObject.FindGameObjectsWithTag("Target");
            if (targets.Length > 0)
            {
                target = targets[0].GetComponent<ComparisonStimulus>();
            }

            if (trial.pause)
            {
                if (GetYesPressed() || GetNoPressed())
                {
                    trial.pause = false;
                }
            }

            if (GetYesPressed() && target != null)
            {
                target.makeChoice(0);
                target = null;
            }

            if (GetNoPressed() && target != null)
            {
                target.makeChoice(1);
                target = null;
            }
        }

        bool GetYesPressed()
        {
            return yesAction.action != null && yesAction.action.WasPressedThisFrame();
        }

        bool GetNoPressed()
        {
            return noAction.action != null && noAction.action.WasPressedThisFrame();
        }
    }
}