using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using Valve.VR;

namespace ActionSimilarity {
    public class Actions : MonoBehaviour {

        public SteamVR_Input_Sources handType;
        public SteamVR_Action_Boolean yes;
        public SteamVR_Action_Boolean no;
        private XRDeviceControllerControls inputActions;
        ComparisonStimulus target;

        public Trial trial;
        
        void Awake() {
            inputActions = new XRDeviceControllerControls();
        }

        void OnEnable() {
            inputActions.Enable();
        }

        void OnDisable() {
            inputActions.Disable();
        }
        
        // Update is called once per frame
        void Update() {
            GameObject[] targets = GameObject.FindGameObjectsWithTag("Target");
            if (targets.Length > 0) {
                target = targets[0].GetComponent<ComparisonStimulus>();
            }

            if (trial.pause) {
                // Debug.Log("paused");
                if (GetLeftTrigger() || GetRightTrigger()) {
                    // Debug.Log("unpause");
                    trial.pause = false;
                }
            }

            if (GetLeftTrigger() && target != null) {
                target.makeChoice(0);
                target = null;
            }

            if (GetRightTrigger()&& target != null) {
                // Debug.Log();
                target.makeChoice(1);
                target = null;
            }

        }

        // public bool GetLeftTrigger() {
        //     if (!trial.simulating) {
        //         return yes.GetStateDown(handType);
        //     } 
        //     return Input.GetKeyDown("[");
        // }
        //
        // public bool GetRightTrigger() {
        //     if (!trial.simulating) {
        //         return no.GetStateDown(handType);
        //     }
        //     return Input.GetKeyDown("]");
        // }
        bool GetLeftTrigger() {
            return trial.simulating
                ? inputActions.Controller.Trigger.ReadValue<float>() > 0
                : yes.GetStateDown(handType);
        }

        bool GetRightTrigger() {
            return trial.simulating
                ? inputActions.Controller.Trigger.ReadValue<float>() > 0
                : no.GetStateDown(handType);
        }
    }
}