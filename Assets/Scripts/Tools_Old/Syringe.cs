using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class Syringe : Tool
{
    [Header("VR Input")]
    public InputActionProperty injectAction;

    [Header("Markers")]
    public InjectionMarker[] markers;

    [Header("Snap Settings")]
    public Vector3 snapOffset = new Vector3(0f, 0.05f, -0.02f);
    public float triggerHeightOffset = 0.1f;
    public Vector3 snapEulerOffset = new Vector3(45f, 0f, 0f);

    [Header("UI (Syringe Panel)")]
    public TextMeshProUGUI syringeText; // assign in Inspector
    public GameObject syringePanel;     // assign the full panel here

    private IInjectable currentTarget;
    private InjectionMarker currentMarker;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private bool isSnapped = false;
    private bool isInjecting = false;

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnPickup);
            grabInteractable.selectExited.AddListener(OnDrop);
        }

        foreach (var marker in markers)
            marker.gameObject.SetActive(false);

        // Hide the panel initially
        if (syringePanel != null)
            syringePanel.SetActive(false);

        UpdatePanelText("Ready");
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnPickup);
            grabInteractable.selectExited.RemoveListener(OnDrop);
        }
    }

    private void OnPickup(SelectEnterEventArgs args)
    {
        foreach (var marker in markers)
            marker.gameObject.SetActive(true);

        if (syringePanel != null)
            syringePanel.SetActive(true); // show panel on pickup

        UpdatePanelText("Syringe picked up");
    }

    private void OnDrop(SelectExitEventArgs args)
    {
        foreach (var marker in markers)
            marker.gameObject.SetActive(false);

        if (currentMarker != null && isInjecting)
        {
            currentMarker.CompleteInjection();
            UpdatePanelText($"Injection complete!\nScore: {currentMarker.Score:F1}%");
        }

        ReleaseSnap();

        if (syringePanel != null)
            syringePanel.SetActive(false); // hide panel on drop
    }

    private void Update()
    {
        if (currentTarget != null && injectAction.action.IsPressed())
        {
            if (!isSnapped && currentMarker != null)
                SnapToMarker(currentMarker);

            if (isSnapped)
            {
                isInjecting = true;
                currentTarget.Inject(Time.deltaTime);

                if (currentMarker != null)
                    UpdatePanelText($"Injecting...\nAmount: {currentMarker.InjectedAmount:F2} ml");
            }
        }
        else if (isSnapped && currentMarker != null && isInjecting)
        {
            // finalize injection when button released
            currentMarker.CompleteInjection();
            int intScore = Mathf.RoundToInt(currentMarker.Score);
            ScoreManager.Instance.AddScore(intScore);
            UpdatePanelText($"Injection complete!\nScore: {currentMarker.Score:F1}%");
            ReleaseSnap();
        }
    }

    protected override void OnAction<T>(T target)
    {
        currentTarget = target as IInjectable;
        currentMarker = target as InjectionMarker;
    }

    private void SnapToMarker(InjectionMarker marker)
    {
        if (marker == null) return;

        isSnapped = true;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        if (grabInteractable != null)
        {
            grabInteractable.trackPosition = false;
            grabInteractable.trackRotation = false;
        }

        Vector3 targetPos = marker.transform.position
                            - transform.up * triggerHeightOffset
                            + marker.transform.TransformDirection(snapOffset);
        Quaternion targetRot = marker.transform.rotation * Quaternion.Euler(snapEulerOffset);

        transform.position = targetPos;
        transform.rotation = targetRot;

        UpdatePanelText("Ready to inject");
    }

    private void ReleaseSnap()
    {
        if (!isSnapped) return;

        isSnapped = false;
        isInjecting = false;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        if (grabInteractable != null)
        {
            grabInteractable.trackPosition = true;
            grabInteractable.trackRotation = true;
        }

        currentMarker = null;
        currentTarget = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryApply<IInjectable>(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (currentTarget != null && !isInjecting)
        {
            if (other.GetComponent<IInjectable>() == currentTarget)
            {
                currentTarget.ResetProgress();
                currentTarget = null;
                currentMarker = null;
            }
        }
    }

    private void UpdatePanelText(string text)
    {
        if (syringeText != null)
            syringeText.text = text;
    }
}
