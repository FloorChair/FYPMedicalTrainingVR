using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Drill : Tool
{
    [Header("VR Input")]
    public InputActionProperty cutAction; // trigger/grip for cutting

    [Header("Markers")]
    public List<DrillGuide> markers;      // use DrillGuide as markers
    public List<Vector2Int> cutPairs;     // valid start/end marker index pairs

    [Header("Tooth Reference")]
    public ToothBehaviour targetTooth;    // the tooth to cut directly

    [Header("UI (Drill Panel)")]
    public TextMeshProUGUI drillText;     // assign in Inspector
    public GameObject drillPanel;         // assign the panel GameObject

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private bool isHeld = false;

    private DrillGuide currentMarker = null;
    private DrillGuide startMarker = null;

    private int score = 0;

    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor currentInteractor = null;

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnPickup);
            grabInteractable.selectExited.AddListener(OnDrop);
        }

        if (drillPanel != null)
            drillPanel.SetActive(false);

        UpdatePanelText("Ready");
    }

    private void OnPickup(SelectEnterEventArgs args)
    {
        isHeld = true;
        SetMarkersActive(true);

        if (drillPanel != null)
            drillPanel.SetActive(true);

        UpdatePanelText("Drill picked up");

        // Get the interactor for haptic feedback
        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor xrInteractor)
        {
            currentInteractor = xrInteractor;
        }
    }

    private void OnDrop(SelectExitEventArgs args)
    {
        isHeld = false;
        SetMarkersActive(false);
        startMarker = null;
        currentMarker = null;

        if (drillPanel != null)
            drillPanel.SetActive(false);

        UpdatePanelText("Ready");

        currentInteractor = null; // remove reference when dropped
    }

    private void OnTriggerEnter(Collider other) => TryApply<DrillGuide>(other);

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<DrillGuide>() == currentMarker)
            currentMarker = null;
    }

    private void Update()
    {
        if (!isHeld) return;

        // Continuous haptic while activate button is held
        if (cutAction.action.IsPressed() && currentInteractor != null)
        {
            // amplitude (0-1), duration per frame (seconds)
            currentInteractor.SendHapticImpulse(0.5f, Time.deltaTime);
        }

        // Only handle cutting if touching a marker
        if (currentMarker == null) return;

        // Start cut when trigger pressed
        if (cutAction.action.WasPressedThisFrame() && startMarker == null)
        {
            startMarker = currentMarker;
            startMarker.MarkCut();
            UpdatePanelText("Cut started");
        }

        // End cut when trigger released
        if (cutAction.action.WasReleasedThisFrame() && startMarker != null)
        {
            if (currentMarker != null && currentMarker != startMarker)
            {
                int startIndex = markers.IndexOf(startMarker);
                int endIndex = markers.IndexOf(currentMarker);

                foreach (var pair in cutPairs)
                {
                    if ((pair.x == startIndex && pair.y == endIndex) ||
                        (pair.y == startIndex && pair.x == endIndex))
                    {
                        targetTooth?.Cut();
                        currentMarker.MarkCut();
                        score += 1;
                        ScoreManager.Instance.AddScore(score * 100);
                        UpdatePanelText($"Cut complete! Score: {score * 100}");
                        break;
                    }
                }
            }

            startMarker = null;
        }
    }

    private void SetMarkersActive(bool active)
    {
        if (markers == null) return;
        foreach (var marker in markers)
            marker.gameObject.SetActive(active);
    }

    protected override void OnAction<T>(T target)
    {
        currentMarker = target as DrillGuide;
    }

    private void UpdatePanelText(string text)
    {
        if (drillText != null)
            drillText.text = text;
    }
}
