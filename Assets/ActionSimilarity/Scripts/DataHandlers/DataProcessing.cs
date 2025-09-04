using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using UXF;

public class DataProcessing
{

    List<float> reactionTimes;
    int correctChoices;
    Dictionary<int, int> straightChoiceMap;
    Dictionary<int, int> curvedChoiceMap;
    int numberOfTrials;

    int actionPos;

    public DataProcessing(int actionPos, int numberOfTrials) {
        reactionTimes = new List<float>();
        straightChoiceMap = new Dictionary<int, int>();
        curvedChoiceMap = new Dictionary<int, int>();
        correctChoices = 0;
        this.actionPos = actionPos;
        this.numberOfTrials = numberOfTrials;
    }

    public void reset(int numberOfTrials) {
        reactionTimes = new List<float>();
        straightChoiceMap = new Dictionary<int, int>();
        curvedChoiceMap = new Dictionary<int, int>();
        correctChoices = 0;
        this.numberOfTrials = numberOfTrials;
    }


    public void LogChoice(float displayTime,
        float actionTime,
        string shapeCategory,
        int comparisonShape,
        int choice, // 0 = rounder, 1 = cubier
        Dictionary<string, int> triggerMap,
        Trial trial
        ) {

        reactionTimes.Add(actionTime - displayTime);
        trial.result["PerceptionReactionTime"] = (actionTime - displayTime);
        AddChoice(comparisonShape, straightChoiceMap, curvedChoiceMap, choice, triggerMap, trial, shapeCategory);
    }


    public float AverageRT() => reactionTimes.Average();

    public float Accuracy() {
        return (float) correctChoices / numberOfTrials;
    }

    void AddChoice(int compPos,
                   Dictionary<int, int> straightChoiceMap,
                   Dictionary<int, int> curvedChoiceMap,
                   int choice,
                   Dictionary<string, int> triggerMap,
                   Trial trial,
                   string shapeCategory) {

        Dictionary<int, int> choiceMap;

        if (shapeCategory == "Straight") {
            choiceMap = straightChoiceMap;
        } else {
            choiceMap = curvedChoiceMap;
        }
        // Empty entry catch
        if (choiceMap.ContainsKey(compPos)) {
            choiceMap[compPos] += choice;
        } else {
            choiceMap[compPos] = choice;
        }

        int curvedChoice = triggerMap["Curved"];
        int straightChoice = triggerMap["Straight"];


        if (shapeCategory == "straight") {
            // If shape is straight and
            // comp is more curved (comp > action) then -ve since away from straight
            // but if comp is more straight (comp < action) then +ve incorrect since towards straight  
            trial.result["PerceptionResult"] = actionPos - compPos;
        } else if (shapeCategory=="curved") {
            // If shape is curved and
            // comp is more curved (comp > action) than they are +vely incorrect since towards curved
            // but if comp is more straight (comp < action) than they are -vely incorrect since away from shape  
            trial.result["PerceptionResult"] = compPos - actionPos;
        } else {
            Debug.LogError("Didn't save perceptionstep");
        }

        // If rounder 
        if (compPos > actionPos) {
            if (choice == curvedChoice) {
                correctChoices++;
                trial.result["PerceptionResult_Bin"] = 1;
             } else {
                trial.result["PerceptionResult_Bin"] = 0;
             }
        } else if (compPos < actionPos) { // if squarer
            if (choice == straightChoice) {
                correctChoices++;
                trial.result["PerceptionResult_Bin"] = 1;
             } else {
                trial.result["PerceptionResult_Bin"] = 0;
             }
        } else {
            if (shapeCategory == "straight" && choice == straightChoice
                || shapeCategory == "curved" && choice == curvedChoice) {
                trial.result["PerceptionResult"] = 0.001;
                trial.result["PerceptionResult_Bin"] = 1;

            } else if (shapeCategory == "straight" && choice == curvedChoice
                || shapeCategory == "curved" && choice == straightChoice) {
                trial.result["PerceptionResult"] = -0.001;
                trial.result["PerceptionResult_Bin"] = 1;
            }else {
                Debug.LogError("Didn't save perceptionstep");
            }

        }



    }
}
