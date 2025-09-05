using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;

namespace PBL.DataHandler
{
    public class ResponseShapeMetadata
    {
        private int correct = -1;
        private int encodingIndex;
        private int shapeIndex;
        private int colorIndex;
        private int loc;
        private int colorEncIndex;
        private int shapeEncIndex;
        private int rt = -1;
        private int reportedIndex = -1;

        private bool processed;

        public int Correct => correct;

        public int EncodingIndex => encodingIndex;

        public int ShapeIndex => shapeIndex;

        public int ColorIndex => colorIndex;

        public int Loc => loc;

        public int ColorEncIndex => colorEncIndex;

        public int ShapeEncIndex => shapeEncIndex;


        public int RT
        {
            get => rt;
            set => rt = value;
        }

        public bool Processed
        {
            get => processed;
            set => processed = value;
        }

        public int ReportedIndex
        {
            get => reportedIndex;
            set => reportedIndex = value;
        }

        public ResponseShapeMetadata(
            int colorIndex,
            int shapeIndex,
            int loc,
            int colorEncIndex,
            int shapeEncIndex,
            int encodingIndex = -1)
        {
            this.processed = false;
            this.colorIndex = colorIndex;
            this.shapeIndex = shapeIndex;
            this.encodingIndex = encodingIndex;
            this.loc = loc;
            this.colorEncIndex = colorEncIndex;
            this.shapeEncIndex = shapeEncIndex;
            this.correct = this.encodingIndex == -1 ? 0 : 1;
        }

        public Dictionary<string, string> ToDictionary(int responseIndex)
        {
            return new Dictionary<string, string>
            {
                [$"Resp{responseIndex}_correct"] = correct.ToString(),
                [$"Resp{responseIndex}_encLoc"] = (encodingIndex == -1) ? "nan" : (encodingIndex + 1).ToString(),
                [$"Resp{responseIndex}_shape"] = shapeIndex.ToString(),
                [$"Resp{responseIndex}_colour"] = colorIndex.ToString(),
                [$"Resp{responseIndex}_loc"] = loc.ToString(),
                [$"Resp{responseIndex}_rt"] = rt.ToString(),
                [$"Resp{responseIndex}_colourEncLoc"] = colorEncIndex.ToString(),
                [$"Resp{responseIndex}_shapeEncLoc"] = shapeEncIndex.ToString()
            };
        }

        public Tuple<int, int> GetShapePair()
        {
            Debug.Log($"{this.colorIndex}, {this.shapeIndex}");
            return new Tuple<int, int>(this.colorIndex, this.shapeIndex);
        }

    }

    public class ResponseShapeMetadataObject : MonoBehaviour
    {
        public ResponseShapeMetadata Data;

    }
}