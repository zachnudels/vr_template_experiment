using System.Collections.Generic;
using System.Linq;
using PBL.DataHandler;
using UnityEngine;
using UXF;

namespace PBL.Experiments
{
     

    public class PBLShiftedTrial : PBLTrialBase
    {

        [SerializeField] protected List<float> shifts;

        private float _shift;

        protected override void ExtractFurtherSettings()
        {
            int shiftI = _uxf.settings.GetInt("cond.Shift");
            _shift = shifts[shiftI];
            // _colorCode = settings.colorCode; 
            // Debug.Log(_colorCode);
        }


        /// Called once per encoding item during instantiation.
        /// Children can override this to capture special codes, adjust data, etc.
        protected override void OnEncodingItemCreated(int i, GameObject[] stimuli)
        {
            GameObject gameObject = stimuli[i];
            Vector3 position = gameObject.transform.position;
            position.z += _shift;
            gameObject.transform.position = position;
        }


    }
}