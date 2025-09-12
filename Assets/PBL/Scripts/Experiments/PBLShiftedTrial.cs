using System.Collections.Generic;
using System.Linq;
using PBL.DataHandler;
using UnityEngine;
using UXF;
using PBL.Types;

namespace PBL.Experiments
{
     

    public class PBLShiftedTrial : PBLTrialBase
    {

        [SerializeField] protected List<float> shifts;

        private float _shiftVal;

        protected override void ExtractFurtherSettings()
        {
            int shiftI = _uxf.settings.GetInt("cond.Shift");
            _shiftVal = shifts[shiftI];
            string logShift = "0";

            // If we are facing the front, negative z is further from us, so -shift is on far side, +ve shift is on close 
            // Otherwise, opposite
            if (faceDirection == FaceDirection.Front)
            {
                logShift = _shiftVal < 0 ? "far" : _shiftVal > 0 ? "close" : "0";
            }
            else
            {
                logShift = _shiftVal < 0 ? "close" : +_shiftVal > 0 ? "far": "0";
            }
            Debug.Log(logShift);

            _uxf.result["shift"] = logShift;
            _shift = new Vector3(0f, 0f, _shiftVal);
            // TODO: Do the encoding in the direction of motion
        }

    }
}