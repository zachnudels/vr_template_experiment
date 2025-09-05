using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UXF;
using PBL.Experiments;

namespace PBL{
    public class InputController : MonoBehaviour {

        //public Session session;
        public PBLTrialBase trial;

        // Start is called before the first frame update
        void Start() {

        }

        // Update is called once per frame
        void Update() {
            if (Input.GetKeyDown("z")) {
                print("z");
            }

        }
    }
}
