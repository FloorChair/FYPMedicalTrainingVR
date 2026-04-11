using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit; // for XRGrabInteractable

public class Tweezers : Tool
{
    [Header("VR Input")]
    public InputActionProperty selectAction;

    [Header("Tool Tip")]
    public Transform tip;

    [Header("Feedback Panel")]
    public GameObject tweezersPanel;
    public TextMeshProUGUI tweezersText;

    [Header("Pick & Place Feedback")]
    public GameObject placementSurface;   // the plane or target object

    private GameObject heldObject = null;
    private Collider candidate = null;

    private readonly HashSet<string> placedHalves = new HashSet<string>();
    private readonly HashSet<string> completedObjects = new HashSet<string>();
    private int runningScore = 0;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnToolPickup);
            grabInteractable.selectExited.AddListener(OnToolDrop);
        }

        if (tweezersPanel != null)
            tweezersPanel.SetActive(false);

        UpdatePanelText("Ready");
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnToolPickup);
            grabInteractable.selectExited.RemoveListener(OnToolDrop);
        }
    }

    // --- Panel logic for the tool itself ---
    private void OnToolPickup(SelectEnterEventArgs args)
    {
        if (tweezersPanel != null)
            tweezersPanel.SetActive(true);

        UpdatePanelText("Tweezers picked up");
    }

    private void OnToolDrop(SelectExitEventArgs args)
    {
        if (tweezersPanel != null)
            tweezersPanel.SetActive(false);
    }

    private void OnEnable() => selectAction.action.Enable();
    private void OnDisable() => selectAction.action.Disable();

    private void Update()
    {
        // --- Pick up tooth ---
        if (heldObject == null)
        {
            if (candidate != null && selectAction.action.WasPressedThisFrame())
            {
                TryApply<Collider>(candidate);
                heldObject = candidate.gameObject;
            }
            return;
        }

        // --- Follow tip ---
        heldObject.transform.position = tip.position;
        heldObject.transform.rotation = tip.rotation;

        // --- Release tooth ---
        if (selectAction.action.WasReleasedThisFrame())
            ReleaseObject();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (heldObject != null) return;

        var tooth = other.GetComponent<ToothBehaviour>();
        if (tooth != null && !tooth.IsGrabbable) return;

        candidate = other;
    }

    private void OnTriggerExit(Collider other)
    {
        if (candidate == other)
            candidate = null;
    }

    protected override void OnAction<T>(T target)
    {
        var obj = target as Component;
        if (obj == null) return;

        heldObject = obj.gameObject;

        var rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    private void ReleaseObject()
    {
        if (heldObject == null) return;

        var rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        heldObject.transform.SetParent(null);

        var pd = heldObject.GetComponent<PlacementDetector>();
        if (pd == null)
            pd = heldObject.AddComponent<PlacementDetector>();

        pd.Init(this, placementSurface);

        heldObject = null;
        candidate = null;
    }

    // Called by PlacementDetector
    public void RegisterPlacement(GameObject placedObj)
    {
        string baseName = GetBaseName(placedObj.name);
        string leftName = baseName + "_left";
        string rightName = baseName + "_right";

        placedHalves.Add(placedObj.name);
        runningScore++;

        bool bothPlaced = placedHalves.Contains(leftName) && placedHalves.Contains(rightName);

        if (bothPlaced && !completedObjects.Contains(baseName))
        {
            completedObjects.Add(baseName);
            ScoreManager.Instance.AddScore(runningScore * 100);
            UpdatePanelText($"Both segments placed\nTotal Segments Placed: {runningScore}");
        }
        else
        {
            UpdatePanelText($"Placed segment {baseName}\nTotal Segments Placed: {runningScore}");
        }
    }

    private string GetBaseName(string name)
    {
        if (name.EndsWith("_left"))
            return name.Substring(0, name.Length - 5);
        if (name.EndsWith("_right"))
            return name.Substring(0, name.Length - 6);
        return name;
    }

    private void UpdatePanelText(string text)
    {
        if (tweezersText != null)
            tweezersText.text = text;
    }
}

// --- Helper Script ---
public class PlacementDetector : MonoBehaviour
{
    private Tweezers tweezers;
    private GameObject placementSurface;
    private bool placed = false;

    public void Init(Tweezers t, GameObject surface)
    {
        tweezers = t;
        placementSurface = surface;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (placed) return;

        if (collision.gameObject == placementSurface)
        {
            placed = true;
            tweezers.RegisterPlacement(gameObject);
        }
    }
}
