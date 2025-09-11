using System.Collections.Generic;
using System.Linq;
using PBL.DataHandler;
using UnityEngine;
using UXF;

namespace PBL.Experiments
{
    public class PBLOriginalTrial : PBLTrialBase
    {
        
        protected override void SimulateReporting()
        {
            foreach (Color color in settings.shapeSettings.shapeColours)
            {
                List<GameObject> coloredObjs = reportingStimuli.Values.Where(obj =>
                {
                    Renderer renderer = obj.GetComponent<Renderer>();
                    return renderer != null && renderer.material.color == color;
                }).ToList();
                GameObject randomReportedObj = coloredObjs[UnityEngine.Random.Range(0, coloredObjs.Count)];
                ReportShapeSelected(randomReportedObj, false);
            }
        }
        
        protected override Color SetColor(int i)
        {
            Color color = settings.shapeSettings.shapeColours[i];
            return color;
        }
        
        protected override string ReportCorrectEnc(int i, bool notReported)
        {
            // Check what it is otherwise
            // Single color specific. Only the specific chosen color can be reported on
            bool canBeReported = settings.shapeSettings.colorPositions[i] == 0;
            // Debug.Log($"Can be reported :{canBeReported}, code: {settings.shapeSettings.colorPositions[i]}");
            string correct = "nan";
            if (canBeReported && notReported)
            {
                correct = "0";
            } else if (canBeReported) {
                correct = "1";
            }
            
            return correct;
        }

        protected override (string, ResponseShapeMetadata, bool) ReportCorrectColor(int i, ResponseShapeMetadata encodingShape)
        {
            // //TODO See what it is normally
            bool canBeReported = i == 0;
            // Single color specific. Only the specific chosen color can be reported on 
            ResponseShapeMetadata reportedShape = _reportedStimuli
                .FirstOrDefault(item => item.ColorIndex == i && item.RT != -1);
            if (reportedShape == null && canBeReported)
            {
                throw new UnityException($"Could not find reported stimuli with color index {i}");
            }
            bool notReported = reportedShape == encodingShape;
            
            string correct = "nan";
            if (canBeReported && notReported)
            {
                correct = "0";
            } else if (canBeReported) {
                correct = "1";
            }
            
            return (correct, reportedShape, notReported);
        }

        protected override string ReportCorrectShape(int i, bool notReported)
        {
                
            // Single color specific. Only the specific chosen color can be reported on 
                
            bool canBeReported = i == 0;
            string correct = "nan";
            if (canBeReported && notReported)
            {
                correct = "0";
            } else if (canBeReported) {
                correct = "1";
            }
            
            return correct;
        }
    }
}