using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class ToolActionController : MonoBehaviour
{
    // Input
    public InputActionProperty activateTriggerAction;

    // Tool Belt
    [Header("Tool Belt")]
    public bool useToolBelt = false;
    [HideInInspector] public bool IsDocked { get; set; }

    public enum HapticMode
    {
        Simple,
        Advanced
    }

    // Haptics
    public bool enableHaptics = true;
    public HapticMode hapticMode = HapticMode.Simple;
    [Range(0f, 1f)] public float hapticAmplitude = 0.5f;
    public UnityEvent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor> onAdvancedHaptics;

    // Position Guides
    public bool enableGuides = false;
    public List<GameObject> associatedGuides = new List<GameObject>();

    // Ghost Guide
    public bool enableGhostGuide = false;
    public float ghostGuideHideDistance = 0.1f;
    public GameObject ghostGuide;
    public GhostGuideAnimator ghostAnimator;

    // Highlight
    public bool enableHighlight = true;
    public string objectsTag = "Default";
    public string highlightLayerName = "Outline";
    public string originalLayerName = "Default";

    // Procedure
    public ProcedureGenerator procedureGenerator;

    // Controller Display
    public ControllerToolDisplay controllerDisplay;
    public string displayName;

    // Projection Settings
    public bool enableProjection = false;
    public Transform toolTip;
    public Vector3 localForward = Vector3.forward;
    public float maxProjectionDistance = 0.3f;
    public LayerMask projectionLayer;
    public GameObject projectionMarkerPrefab;

    public enum ProjectionMode { Locked, DrawLine }
    public ProjectionMode projectionMode = ProjectionMode.Locked;
    public Vector3 LastDrawStartPoint { get; private set; }
    public Vector3 LastDrawEndPoint { get; private set; }

    // Draw Line Settings
    public Material lineMaterial;
    public float lineWidth = 0.01f;

    // Events
    public UnityEvent onTriggerPressed;
    public UnityEvent onTriggerReleased;
    public UnityEvent onPickup;
    public UnityEvent onDrop;
    public UnityEvent onProjectionActionComplete;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor currentInteractor;

    private bool isHeld;
    private bool triggerHeld;
    private bool actionCompleted;

    private GameObject projectionMarker;
    private Renderer projectionRenderer;
    private bool projectionLocked = false;

    // Draw line
    private LineRenderer currentLine;
    private Vector3 drawStartPoint;
    private bool drawInProgress = false;

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);

        HideGuides();

        if (enableGhostGuide && ghostGuide != null)
            ghostGuide.SetActive(false);

        if (enableProjection && projectionMarkerPrefab != null)
        {
            projectionMarker = Instantiate(projectionMarkerPrefab);
            projectionMarker.SetActive(false);
            projectionRenderer = projectionMarker.GetComponent<Renderer>();
        }
    }

    private void OnEnable() => activateTriggerAction.action?.Enable();
    private void OnDisable() => activateTriggerAction.action?.Disable();

    private void Update()
    {
        if (!isHeld) return;

        // Show/hide ghost guide based on proximity
        if (enableGhostGuide && ghostGuide != null)
        {
            float distance = Vector3.Distance(transform.position, ghostGuide.transform.position);
            ghostGuide.SetActive(distance > ghostGuideHideDistance);
        }

        // Haptics
        if (enableHaptics && activateTriggerAction.action.IsPressed() && currentInteractor != null)
        {
            switch (hapticMode)
            {
                case HapticMode.Simple:
                    currentInteractor.SendHapticImpulse(hapticAmplitude, Time.deltaTime);
                    break;

                case HapticMode.Advanced:
                    onAdvancedHaptics?.Invoke(currentInteractor);
                    break;
            }
        }

        // Trigger events
        if (activateTriggerAction.action.WasPressedThisFrame() && !triggerHeld)
        {
            triggerHeld = true;
            onTriggerPressed?.Invoke();
            HandleProjectionModeOnTriggerPressed();
        }

        if (activateTriggerAction.action.WasReleasedThisFrame() && triggerHeld)
        {
            triggerHeld = false;
            onTriggerReleased?.Invoke();
            HandleProjectionModeOnTriggerReleased();
        }
    }

    private void LateUpdate()
    {
        if (enableProjection)
            UpdateProjectionMarker();

        if (projectionMode == ProjectionMode.DrawLine && drawInProgress && currentLine != null && projectionMarker != null)
            currentLine.SetPosition(1, projectionMarker.transform.position);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        isHeld = true;
        actionCompleted = false;

        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor xr)
            currentInteractor = xr;

        if (enableGuides) ShowGuides();
        if (enableHighlight) SetObjectsToHighlightLayer();

        if (controllerDisplay != null)
        {
            string nameToShow = string.IsNullOrEmpty(displayName) ? gameObject.name : displayName;
            controllerDisplay.ShowTool(nameToShow);
        }

        if (enableGhostGuide && ghostGuide != null)
        {
            ghostGuide.SetActive(true);

            ghostAnimator?.PlayAnimation(ghostGuide);
        }

        procedureGenerator?.ReportToolPickedUp(gameObject);

        onPickup?.Invoke();
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isHeld = false;
        triggerHeld = false;
        currentInteractor = null;

        if (enableGuides) HideGuides();
        if (enableHighlight) RestoreObjectsLayer();

        if (controllerDisplay != null)
            controllerDisplay.HideTool();

        if (actionCompleted)
            procedureGenerator?.ReportToolActionCompleted(gameObject);
        
        if (enableGhostGuide && ghostGuide != null)
            ghostGuide.SetActive(false);

        onDrop?.Invoke();
    }

    public void MarkActionCompleted()
    {
        actionCompleted = true;

        ToolScoring scoring = GetComponent<ToolScoring>();
        scoring?.OnActionCompleted();

        Debug.Log($"{name} action completed");
    }

    private void ShowGuides()
    {
        foreach (var g in associatedGuides)
            if (g) g.SetActive(true);
    }

    private void HideGuides()
    {
        foreach (var g in associatedGuides)
            if (g) g.SetActive(false);
    }

    private void SetObjectsToHighlightLayer()
    {
        int layer = LayerMask.NameToLayer(highlightLayerName);
        foreach (var obj in GameObject.FindGameObjectsWithTag(objectsTag))
            obj.layer = layer;
    }

    private void RestoreObjectsLayer()
    {
        int layer = LayerMask.NameToLayer(originalLayerName);
        foreach (var obj in GameObject.FindGameObjectsWithTag(objectsTag))
            obj.layer = layer;
    }

    #region Projection

    private void UpdateProjectionMarker()
    {
        if (projectionLocked || projectionMarker == null || toolTip == null)
            return;

        Vector3 worldDir = toolTip.TransformDirection(localForward).normalized;
        Ray ray = new Ray(toolTip.position, worldDir);

#if UNITY_EDITOR
        Debug.DrawRay(toolTip.position, worldDir * maxProjectionDistance, Color.cyan);
#endif

        if (Physics.Raycast(ray, out RaycastHit hit, maxProjectionDistance, projectionLayer))
        {
            projectionMarker.SetActive(true);
            projectionMarker.transform.position = hit.point + hit.normal * 0.001f;
            projectionMarker.transform.rotation = Quaternion.LookRotation(hit.normal);
        }
        else
        {
            projectionMarker.SetActive(false);
        }
    }

    private void HandleProjectionModeOnTriggerPressed()
    {
        if (!enableProjection || projectionMarker == null || !projectionMarker.activeSelf) return;

        switch (projectionMode)
        {
            case ProjectionMode.Locked:
                LockProjection();

                // Snap tool to marker but DO NOT disable grab permanently
                if (toolTip != null)
                    transform.position += projectionMarker.transform.position - toolTip.position;

                grabInteractable.trackPosition = false;
                grabInteractable.trackRotation = true;

                onProjectionActionComplete?.Invoke();
                break;

            case ProjectionMode.DrawLine:
                drawStartPoint = projectionMarker.transform.position;

                if (currentLine == null)
                {
                    GameObject lineObj = new GameObject($"{name}_ProjectionLine");
                    currentLine = lineObj.AddComponent<LineRenderer>();
                    currentLine.material = lineMaterial ?? new Material(Shader.Find("Sprites/Default"));
                    currentLine.startWidth = lineWidth;
                    currentLine.endWidth = lineWidth;
                    currentLine.useWorldSpace = true;
                }

                currentLine.positionCount = 2;
                currentLine.SetPosition(0, drawStartPoint);
                currentLine.SetPosition(1, drawStartPoint);
                currentLine.enabled = true;
                drawInProgress = true;
                break;
        }
    }

    private void HandleProjectionModeOnTriggerReleased()
    {
        if (!enableProjection) return;

        if (projectionMode == ProjectionMode.DrawLine && drawInProgress && projectionMarker != null)
        {
            Vector3 endPoint = projectionMarker.transform.position;

            currentLine.SetPosition(1, endPoint);
            drawInProgress = false;

            if (currentLine != null)
            {
                currentLine.enabled = false;
            }

            // Store final line positions
            LastDrawStartPoint = drawStartPoint;
            LastDrawEndPoint = endPoint;

            onProjectionActionComplete?.Invoke();
        }

        if (projectionMode == ProjectionMode.Locked)
        {
            UnlockProjection();

            // Restore grab tracking so other tools can be picked up
            grabInteractable.trackPosition = true;
            grabInteractable.trackRotation = true;
        }
    }

    public void LockProjection() => projectionLocked = true;
    public void UnlockProjection() => projectionLocked = false;

    #endregion
}
