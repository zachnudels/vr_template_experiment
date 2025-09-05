using System.Collections.Generic;
using System.Linq;

using ActionSimilarity;
using PBL.Experiments;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


namespace PBL.TrialComponents
{
    [RequireComponent(typeof(XRRayInteractor))]
    [RequireComponent(typeof(XRRayInteractor))]
    [RequireComponent(typeof(XRInteractorLineVisual))]
    public class RayLineSelector : MonoBehaviour
    {
        public Gradient hoverGradient; // white
        public Gradient defaultGradient; // red

        private XRRayInteractor rayInteractor;
        private XRInteractorLineVisual lineVisual;

        public float hoverThreshold = 0.5f;
        public float rayLength = 10f;
        public LayerMask shapeLayer;
        public PBLTrialBase trial;

        private GameObject currentTarget;
        private float hoverTime = 0f;

        void Awake()
        {
            rayInteractor = GetComponent<XRRayInteractor>();
            lineVisual = GetComponent<XRInteractorLineVisual>();
        }

        private void OnEnable()
        {
            if (trial != null)
            {
                trial.StageChanged += OnStageChanged;
            }
        }

        private void OnDisable()
        {
            if (trial != null)
            {
                trial.StageChanged -= OnStageChanged;
            }
        }

        private void OnStageChanged(string stageName)
        {
            lineVisual.enabled = stageName == "Report";
        }

        void Update()
        {
            if (!(Physics.Raycast(rayInteractor.transform.position, rayInteractor.transform.forward, out RaycastHit hit,
                    10f)))
            {
                return;
            }

            GameObject hitObj = hit.collider.gameObject;

            if (hit.collider.CompareTag("ReportingShape"))
            {
                lineVisual.invalidColorGradient = hoverGradient;
                if (hitObj == currentTarget)
                {
                    hoverTime += Time.deltaTime;
                    if (!(hoverTime >= hoverThreshold))
                    {
                        return;
                    }

                    trial.ReportShapeSelected(hitObj, false);
                    hoverTime = -999f; // prevent re-triggering
                }
                else
                {
                    currentTarget = hitObj;
                    hoverTime = 0f;
                }
            }
            else
            {
                lineVisual.invalidColorGradient = defaultGradient;
                currentTarget = null;
                hoverTime = 0f;
            }
        }
    }

    //
    // void Update()
    // {
    //     if (Physics.Raycast(rayInteractor.transform.position, rayInteractor.transform.forward, out RaycastHit hitInfo, 10f)) {
    //         
    //         
    //         // Debug.Log("Physics.Raycast hit: " + hitInfo.collider.name);
    //     }
    //     //
    //     // if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
    //     // {
    //     //     Debug.Log($"[XRRay] Hit: {hit.collider.gameObject.name}");
    //     //
    //     //     if (hit.collider.CompareTag("ReportingShape"))
    //     //     {
    //     //         lineVisual.validColorGradient = hoverGradient;
    //     //     }
    //     //     else
    //     //     {
    //     //         lineVisual.validColorGradient = defaultGradient;
    //     //     }
    //     // }
    //     // else
    //     // {
    //     //     Debug.Log("[XRRay] No hit");
    //     //     lineVisual.validColorGradient = defaultGradient;
    //     // }
    //     
    //     // List<IXRInteractable> targets = new List<IXRInteractable>();
    //     // rayInteractor.GetValidTargets(targets);
    //     //
    //     // Debug.Log("XR targets: " + string.Join(", ", targets.Select(t => t.transform.name)));
    //     // //
    //     // bool isHovering = rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit)
    //     //                   && hit.collider.CompareTag("ReportShape");
    //     //
    //     // lineVisual.invalidColorGradient = isHovering ? hoverGradient : defaultGradient;
    // }
// }
}
