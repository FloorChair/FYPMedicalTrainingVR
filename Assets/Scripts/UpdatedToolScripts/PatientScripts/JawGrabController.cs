using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class JawGrabController : MonoBehaviour
{
    [Header("References")]
    public Transform jawBone;
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    [Header("Jaw Settings")]
    public Vector3 hingeAxis = Vector3.right;
    public float movementToAngle = 120f;
    public float minAngle = -35f;   // fully open
    public float maxAngle = 0f;     // fully closed

    private UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor currentInteractor;
    private Vector3 lastHandPos;

    private Quaternion closedRotation;  // true closed pose
    private float currentAngle = 0f;    // persistent jaw state

    private void Start()
    {
        closedRotation = jawBone.localRotation;
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        currentInteractor = args.interactorObject;
        lastHandPos = currentInteractor.transform.position;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        currentInteractor = null;
    }

    private void Update()
    {
        if (currentInteractor == null)
            return;

        Vector3 currentHandPos = currentInteractor.transform.position;
        Vector3 delta = currentHandPos - lastHandPos;

        float movement = delta.y;

        // Increment angle instead of redefining baseline
        currentAngle += movement * movementToAngle;

        // Absolute clamp
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        // Apply relative to true closed pose
        jawBone.localRotation =
            closedRotation *
            Quaternion.AngleAxis(currentAngle, hingeAxis.normalized);

        lastHandPos = currentHandPos;
    }
}