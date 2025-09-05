using UnityEngine;

namespace PBL.Types
{
    public enum TurnDirection
    {
        Left = -1,
        Right = 1,
    }
    
    public enum FaceDirection
    {
        Front = 1,
        Back = -1,
    }
    
    [System.Serializable]
    public class LabelledInt
    {
        public string key;
        public int value;
    }
    
    [System.Serializable]
    public class LabelledVector3
    {
        public string key;
        public Vector3 value;
    }

    
}