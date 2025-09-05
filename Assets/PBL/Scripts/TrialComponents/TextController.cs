
using UnityEngine;
using TMPro;
using PBL.Types;


namespace PBL.TrialComponents
{
    public class TextController : MonoBehaviour
    {
        
        public TextMeshPro textMesh;
        public PBLExperiment experiment;
        Vector3 _frontWallPosition = new Vector3(0f, 2f, 3.9f);
        Vector3 _frontWallRotation = new Vector3(0f, 0f, 0f);
        Vector3 _backWallPosition = new Vector3(0f, 2f, -3.9f);
        Vector3 _backWallRotation = new Vector3(0f, 180f, 0f);


        //private void Awake()
        //{
        //    textMesh = GetComponent<TextMeshPro>();
        //}

        public void Write(string text)
        {
            textMesh.text = text;    
        }

        public void Debug(string text)
        {
            if (experiment.debug)
            {
                textMesh.text = text;
            } else {
                // textMesh.text = "Debug is false";
            }
        }

        public void Clear()
        {
            textMesh.text = "";
        }

        public FaceDirection ChangeWall(FaceDirection faceDirection, bool swap = false)
        {
            if (faceDirection == FaceDirection.Front && !swap || 
                (faceDirection==FaceDirection.Back && swap))
            {
                // UnityEngine.Debug.Log("Changing text to front wall");
                this.textMesh.transform.position = _frontWallPosition;
                this.textMesh.transform.eulerAngles = _frontWallRotation;
                return FaceDirection.Front;
            }
            else
            {
                // UnityEngine.Debug.Log("Changing text to back wall");
                this.textMesh.transform.position = _backWallPosition;
                this.textMesh.transform.eulerAngles = _backWallRotation;
                return FaceDirection.Back;
            }
            
            
        }

    }
}