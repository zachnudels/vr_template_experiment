using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;

public class ResponseShapeMetadata
{
    private int correct = -1;
    private int encodingIndex;
    private int shapeIndex;
    private int colorIndex;
    private int loc;
    private int rt = -1;

    public int RT
    {
        get => rt;
        set => rt = value;
    }

    private int colorEncIndex;
    private int shapeEncIndex;
    
    public ResponseShapeMetadata(
        int colorIndex, 
        int shapeIndex, 
        int loc,
        int colorEncIndex,
        int shapeEncIndex,
        int encodingIndex = -1)
    {
        this.colorIndex = colorIndex;
        this.shapeIndex = shapeIndex;
        this.encodingIndex = encodingIndex;
        this.loc = loc;
        this.colorEncIndex = colorEncIndex;
        this.shapeEncIndex = shapeEncIndex;
        this.correct = this.encodingIndex == -1 ? 0 : 1;
    }
    
    public Dictionary<string, int> ToDictionary(int responseIndex)
    {
        return new Dictionary<string, int> {
            [$"Resp{responseIndex}_correct"] = correct,
            [$"Resp{responseIndex}_encLoc"] = encodingIndex,
            [$"Resp{responseIndex}_shape"] = shapeIndex,
            [$"Resp{responseIndex}_colour"] = colorIndex,
            [$"Resp{responseIndex}_loc"] = loc,
            [$"Resp{responseIndex}_rt"] = rt, 
            [$"Resp{responseIndex}_colourEncLoc"] = colorEncIndex,
            [$"Resp{responseIndex}_shapeEncLoc"] = shapeEncIndex
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