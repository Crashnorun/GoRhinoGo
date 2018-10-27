using UnityEngine;
using UnityEngine.Events;

public class VRRaycaster : MonoBehaviour
{

    [System.Serializable]
    public class Callback : UnityEvent<Ray, RaycastHit> { }
    
    public LineRenderer lineRenderer = null;
    public float maxRayDistance = 500.0f;
    public LayerMask excludeLayers;
    public VRRaycaster.Callback raycastHitCallback;

    void Awake()
    {
        if (lineRenderer == null)
        {
            Debug.LogWarning("Assign a line renderer in the inspector!");
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.widthMultiplier = 0.015f;
        }
    }
    
    void Update()
    {
            lineRenderer.widthMultiplier = 0.0f;

        //display when trigger is pulled but not when touchpad is touched
        if (transform.parent.GetComponent<PlayerControl>().lastTriggerState && !transform.parent.GetComponent<PlayerControl>().lastTouchState)
        {

            lineRenderer.widthMultiplier = 0.015f;
            lineRenderer.startColor = transform.parent.GetComponent<PlayerControl>().clr;
            lineRenderer.endColor = transform.parent.GetComponent<PlayerControl>().clr;

            Ray laserPointer = new Ray(transform.position, transform.forward);

            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, laserPointer.origin);
                lineRenderer.SetPosition(1, laserPointer.origin + laserPointer.direction * maxRayDistance);
            }

            RaycastHit hit;
            if (Physics.Raycast(laserPointer, out hit, maxRayDistance, ~excludeLayers))
            {
                if (lineRenderer != null)
                {
                    lineRenderer.SetPosition(1, hit.point);
                }

                if (raycastHitCallback != null)
                {
                    raycastHitCallback.Invoke(laserPointer, hit);
                }
            }

        }
    }
}