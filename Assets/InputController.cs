using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UXF;


namespace ActionSimilarity{
    public class InputController : MonoBehaviour {

        //public Session session;
        public Trial trial;

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
