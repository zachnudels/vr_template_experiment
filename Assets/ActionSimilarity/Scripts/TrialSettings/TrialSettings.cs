using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ActionSimilarity
{

    [CreateAssetMenu(menuName = "Trials/Trial Settings")]
    public class TrialSettings : ScriptableObject
    {
        [Header("Shapes")]
        public ShapeSettings shapeSettings;

        [Header("Fixation")]
        public FixationSettings fixationSettings;


        [Header("Trigger Codes")]
        // [Tooltip("""
        // code will be sent to the eye tracking recorder 
        // Please set the mapping from event to code 
        // Make sure to leave enough space between each event code 
        // This allows you to manipulate the code based on a condition at the start
        // of the stage
        // """)
        //  ]
        public List<CodeDictionary> codes;


    }
}