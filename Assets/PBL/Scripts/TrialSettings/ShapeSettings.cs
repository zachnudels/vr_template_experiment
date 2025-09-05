using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PBL.Types;


namespace PBL.TrialComponents
{

    [Serializable]
    public class ShapeSettings
    {
        [HideInInspector] public int count;
        public GameObject encodingShape;
        public GameObject reportingShape;
        public Mesh[] shapeMeshes;
        public Color[] shapeColours;
        public List<LabelledVector3> reportingShapeRotations;
        public List<LabelledVector3> encodingShapeRotations;
        public List<LabelledVector3> shapeScales;
        public float[] shapeSpacing;
        public float reportingShapeScale;

        [HideInInspector] public Vector3[] encodingShapePositions;
        [HideInInspector] public List<int> shapePositions; // which mesh goes on which encoding shape
        [HideInInspector] public List<int> colorPositions; // which color goes on which encoding shape
        [HideInInspector] public Vector3[] reportingShapePositions;

        [HideInInspector] public List<int> shapeRows;
        [HideInInspector] public List<int> colorCols;
        [HideInInspector] public Dictionary<string, Vector3> reportingShapeRotationMap;
        [HideInInspector] public Dictionary<string, Vector3> encodingShapeRotationMap;
        [HideInInspector] public Dictionary<string, Vector3> shapeScaleMap;

        public void Init()
        {

            InitEncodingPositions();
            InitReportingPositions();

            if (this.shapeMeshes.Length != this.shapeColours.Length || this.shapeColours.Length != this.encodingShapePositions.Length)
            {
                throw new UnityException("Mesh, Color, Position Length must all be equal. See Trial object.");
            }

            this.count = this.shapeMeshes.Length;
            if (count * count != this.reportingShapePositions.Length)
            {
                throw new UnityException("reportingShapes must be square of other values!");
            }

            reportingShapeRotationMap = reportingShapeRotations.ToDictionary(e => e.key, e => e.value);
            encodingShapeRotationMap = encodingShapeRotations.ToDictionary(e => e.key, e => e.value);
            shapeScaleMap = shapeScales.ToDictionary(e => e.key, e => e.value);

        }

        private void InitEncodingPositions()
        {
            encodingShapePositions = new Vector3[2 * shapeSpacing.Length];

            for (int i = 0; i != shapeSpacing.Length; ++i)
            {
                encodingShapePositions[i * 2] = new Vector3(0f, 0f, shapeSpacing[i]);
                encodingShapePositions[i * 2 + 1] = new Vector3(0f, 0f, -shapeSpacing[i]);
            }
            Array.Sort(encodingShapePositions, (a, b) => a.z.CompareTo(b.z));

        }

        private void InitReportingPositions()
        {
            reportingShapePositions = new Vector3[4 * shapeSpacing.Length * shapeSpacing.Length];

            List<float> directedShapePositions = new List<float>();
            foreach (float s in shapeSpacing)
            {
                directedShapePositions.Add(-s);
                directedShapePositions.Add(s);
            }

            directedShapePositions.Sort();

            int index = 0;
            foreach (float y in directedShapePositions)
            {
                foreach (float x in directedShapePositions)
                {
                    reportingShapePositions[index] = new Vector3(x, y, 0f);
                    index += 1;
                }
            }
        }
    }
}