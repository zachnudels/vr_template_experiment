// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// public class NewBehaviourScript : MonoBehaviour
// {
//     private GameObject expobj;
//     private Quaternion rotR;
//
//     //circle stuff
//     public float ThetaScale = 0.01f;
//     public float radius = 0.2f;
//     private int Size;
//     private LineRenderer LineDrawer;
//     private float Theta = 0f;
//
//     // Start is called before the first frame update
//     void Start()
//     {
//         if (expobj == null)
//         {
//             expobj = GameObject.FindGameObjectWithTag("experimentobj");
//         }
//
//         //circle stuff
//         //LineDrawer = GetComponent<LineRenderer>();
//         //LineDrawer.SetWidth(0.02f, 0.02f);
//     }
//
//     // Update is called once per frame
//     void Update()
//     {
//         if (expobj != null)
//         {
//             expobj = GameObject.FindGameObjectWithTag("experimentobj");
//         }
//
//         if (!expobj.GetComponent<Experimentscript>().stage.Equals("answer"))
//         {
//             return;
//         }
//
//         if (GameObject.FindGameObjectWithTag("fixationtarget"))
//         {
//             transform.position = GameObject.FindGameObjectWithTag("fixationtarget").transform.position;
//         }
//
//         //UnityEngine.XR.InputDevice rh = expobj.GetComponent<Experimentscript>().getHand();
//         //rh.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out Quaternion rotR);
//         //float factor = (float)expobj.GetComponent<Experimentscript>().getFront();
//
//         /*
//         float handRot = expobj.GetComponent<Experimentscript>().getHandRot();
//         float newRot = handRot; //* expobj.GetComponent<Experimentscript>().getFront();
//         string facing = expobj.GetComponent<Experimentscript>().getFacing();
//
//         
//         if (facing.Equals("back"))
//         {
//             newRot = 360f - newRot;
//         }
//         
//
//         gameObject.transform.eulerAngles = new Vector3(
//             0f,
//             0f,
//             newRot
//             );
//         */
//
//         //circle stuff
//         /*
//         Theta = 0f;
//         Size = (int)((1f / ThetaScale) +1f);
//         LineDrawer.SetVertexCount(Size);
//         for (int i = 0; i < Size; i++)
//         {
//             Theta += (2.0f* Mathf.PI* ThetaScale);
//         float x = radius * Mathf.Cos(Theta);
//         float y = radius * Mathf.Sin(Theta);
//         LineDrawer.SetPosition(i, new Vector3(x, y, 0f));
//         
//         }
//         */
//     }
// }
