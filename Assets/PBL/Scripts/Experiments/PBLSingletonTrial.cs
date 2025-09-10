using System.Collections.Generic;
using System.Linq;
using PBL.DataHandler;
using UnityEngine;
using UXF;

namespace PBL.Experiments
{
    public class PBLSingletonTrial : PBLTrialBase
    {
        
        protected int _colorCode;
        protected int _shapeCode;
        
        protected override void ExtractFurtherSettings(Trial _uxf)
        {
            _colorCode = _uxf.settings.GetInt("cond.colorCode");
        }

        private List<GameObject> FindColoredReportingStimuli()
        {
            Color color = settings.shapeSettings.shapeColours[_colorCode];
            List<GameObject> coloredObjs = reportingStimuli.Values.Where(obj =>
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                return renderer != null && renderer.material.color == color;
            }).ToList();

            return coloredObjs;
        }
        
        protected override void ReportHook()
        {
            foreach (GameObject stimulus in FindColoredReportingStimuli())
            {
                ReportShapeSelected(stimulus, true); // even though we cannot select these, report on them but set to ignore
            }
        }
        
        
        protected override void SimulateReporting()
        {
            List<GameObject> coloredObjs = FindColoredReportingStimuli();
            GameObject randomReportedObj = coloredObjs[UnityEngine.Random.Range(0, coloredObjs.Count)];
            ReportShapeSelected(randomReportedObj, false);
        }
        
        /// Called once per encoding item during instantiation.
        /// Children can override this to capture special codes, adjust data, etc.
        protected override void OnEncodingItemCreated(int i)
        {
            if (settings.shapeSettings.colorPositions[i] == _colorCode)
            {
                _shapeCode = settings.shapeSettings.shapePositions[i];
            }
        }
        
        protected override Color SetColor(int i)
        {
            //Singleton
            return i == _colorCode ? settings.shapeSettings.shapeColours[i] : Color.gray;
        }
        
        protected override string ReportCorrectEnc(int i, bool notReported)
        {
            // Check what it is otherwise
            // Single color specific. Only the specific chosen color can be reported on
            bool canBeReported = settings.shapeSettings.colorPositions[i] == _colorCode;
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
            //TODO See what it is normally
            bool canBeReported = i == _colorCode;
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
                
            bool canBeReported = i == _shapeCode;
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