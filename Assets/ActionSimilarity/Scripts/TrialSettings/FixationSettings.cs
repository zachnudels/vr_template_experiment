using System;
using UnityEngine;
using TMPro;



namespace ActionSimilarity
{

    [Serializable]
    public class FixationSettings
    {
        public GameObject fixationSphere;
        [SerializeField] private int fixationDepth;
        public int FixationDepth => fixationDepth;

        [HideInInspector] public TextMeshPro textMeshPro;

        [HideInInspector] public float turnTime;

        public float leftOffset;
        public float downOffset;

    }
}
