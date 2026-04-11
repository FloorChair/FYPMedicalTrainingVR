using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class PathDrawer : MonoBehaviour
{
    [Header("Input")]
    public InputActionProperty actionButton;  // generic activate button

    [Header("Settings")]
    public LayerMask markerLayer;            // markers should be on this layer
    public Transform toolTip;                 // tip or handle of the tool
    public float rayDistance = 0.05f;        // how far to detect markers

    [Header("Line Visuals")]
    public Color lineColor = Color.white;
    public float lineWidth = 0.01f;

    private LineRenderer lineRenderer;
    private bool isActive = false;
    private Vector3 startMarkerPos;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // basic line renderer setup
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
    }

    private void Update()
    {
        // detect nearby markers
        Collider[] hits = Physics.OverlapSphere(toolTip.position, rayDistance, markerLayer);

        if (hits.Length > 0 && actionButton.action.IsPressed())
        {
            if (!isActive)
            {
                // find the closest marker to the tool tip
                Collider closestMarker = hits[0];
                float minDist = Vector3.Distance(toolTip.position, closestMarker.transform.position);

                for (int i = 1; i < hits.Length; i++)
                {
                    float dist = Vector3.Distance(toolTip.position, hits[i].transform.position);
                    if (dist < minDist)
                    {
                        closestMarker = hits[i];
                        minDist = dist;
                    }
                }

                // start interaction at the closest marker
                isActive = true;
                startMarkerPos = closestMarker.transform.position;

                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, startMarkerPos);
                lineRenderer.SetPosition(1, toolTip.position);

                // optionally highlight marker
                Renderer r = closestMarker.GetComponent<Renderer>();
                if (r != null) r.material.color = Color.green;
            }
            else
            {
                // update second point while holding
                lineRenderer.SetPosition(1, toolTip.position);
            }
        }
        else if (isActive && actionButton.action.WasReleasedThisFrame())
        {
            EndInteraction();
        }
    }

    private void TryStartInteraction()
    {
        Ray ray = new Ray(toolTip.position, toolTip.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, markerLayer))
        {
            isActive = true;
            startMarkerPos = hit.collider.transform.position;

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startMarkerPos);
            lineRenderer.SetPosition(1, toolTip.position);

            // optionally change marker color for feedback
            Renderer r = hit.collider.GetComponent<Renderer>();
            if (r != null) r.material.color = Color.green;
        }
    }

    private void EndInteraction()
    {
        isActive = false;

        // here you could trigger any logic between startMarkerPos and current toolTip.position
        // e.g., activate an effect, trigger an event, etc.

        // clear the line
        lineRenderer.positionCount = 0;
    }
}
