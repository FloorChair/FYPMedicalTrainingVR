using UnityEngine;
using System.Collections.Generic;

public class ForcepsPickup : MonoBehaviour
{
    [Header("Settings")]
    public float pickupRadius = 0.05f;

    [Header("Resistance Settings")]
    public float wriggleThreshold = 15f;
    public float wriggleAngleStep = 3f;

    [Header("References")]
    public ToolActionController toolController;
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    // Pickup state
    private GameObject heldObject;
    private Rigidbody heldRb;
    private HashSet<GameObject> pickedObjects = new HashSet<GameObject>();

    // Resistance state
    private bool inResistancePhase = false;
    private GameObject resistanceTarget;
    private Transform interactionPointRef;
    private Quaternion lastToolRotation;
    private float accumulatedWriggle = 0f;

    public System.Action<float> OnWriggleProgress;

    public void PickOrDrop(Transform interactionPoint)
    {
        if (inResistancePhase)
            return;

        if (heldObject != null)
            Drop();
        else
            TryPick(interactionPoint);
    }

    private void TryPick(Transform interactionPoint)
    {
        if (interactionPoint == null || toolController == null)
            return;

        string pickableTag = toolController.objectsTag;
        Collider[] hits = Physics.OverlapSphere(interactionPoint.position, pickupRadius);

        foreach (var hit in hits)
        {
            GameObject candidate = hit.attachedRigidbody != null
                ? hit.attachedRigidbody.gameObject
                : hit.transform.root.gameObject;

            if (!candidate.CompareTag(pickableTag))
                continue;

            // Only do resistance if this object hasn't been freed before
            if (pickedObjects.Contains(candidate))
                PickDirectly(candidate, interactionPoint);
            else
                BeginResistance(candidate, interactionPoint);

            break;
        }
    }

    private void PickDirectly(GameObject target, Transform interactionPoint)
    {
        heldObject = target;
        heldRb = heldObject.GetComponentInParent<Rigidbody>();

        if (heldRb != null)
        {
            heldRb.isKinematic = true;
            heldRb.useGravity = false;
        }

        heldObject.transform.SetParent(interactionPoint, true);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;
    }

    private void BeginResistance(GameObject target, Transform interactionPoint)
    {
        inResistancePhase = true;
        resistanceTarget = target;
        interactionPointRef = interactionPoint;
        lastToolRotation = transform.rotation;
        accumulatedWriggle = 0f;

        // Freeze target object
        Rigidbody targetRb = target.GetComponentInParent<Rigidbody>();
        if (targetRb != null)
        {
            targetRb.isKinematic = true;
            targetRb.useGravity = false;
        }

        // Lock position, allow rotation around the new origin (tip)
        if (grabInteractable != null)
        {
            grabInteractable.trackPosition = false;
            grabInteractable.trackRotation = true;
        }
    }

    private void Update()
    {
        if (!inResistancePhase) return;

        if (interactionPointRef == null || resistanceTarget == null)
        {
            CancelResistance();
            return;
        }

        Quaternion currentRotation = transform.rotation;
        float angleDelta = Quaternion.Angle(lastToolRotation, currentRotation);

        if (angleDelta >= wriggleAngleStep)
        {
            accumulatedWriggle += angleDelta;
            lastToolRotation = currentRotation;
        }

        float progress = Mathf.Clamp01(accumulatedWriggle / wriggleThreshold);
        OnWriggleProgress?.Invoke(progress);

        if (accumulatedWriggle >= wriggleThreshold)
            CompletePickup();
    }

    private void CompletePickup()
    {
        inResistancePhase = false;

        if (grabInteractable != null)
        {
            grabInteractable.trackPosition = true;
            grabInteractable.trackRotation = true;
        }

        heldObject = resistanceTarget;
        heldRb = heldObject.GetComponentInParent<Rigidbody>();

        if (heldRb != null)
        {
            heldRb.isKinematic = true;
            heldRb.useGravity = false;
        }

        heldObject.transform.SetParent(interactionPointRef, true);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;

        resistanceTarget = null;
        pickedObjects.Add(heldObject);
    }

    private void CancelResistance()
    {
        if (resistanceTarget != null)
        {
            Rigidbody targetRb = resistanceTarget.GetComponentInParent<Rigidbody>();
            if (targetRb != null)
            {
                targetRb.isKinematic = false;
                targetRb.useGravity = true;
            }
        }

        if (grabInteractable != null)
        {
            grabInteractable.trackPosition = true;
            grabInteractable.trackRotation = true;
        }

        inResistancePhase = false;
        resistanceTarget = null;
        accumulatedWriggle = 0f;
    }

    public void Drop()
    {
        if (inResistancePhase)
        {
            CancelResistance();
            return;
        }

        if (heldObject == null) return;

        if (heldRb != null)
        {
            heldRb.isKinematic = false;
            heldRb.useGravity = true;
        }

        heldObject.transform.SetParent(null);
        heldObject = null;
        heldRb = null;
    }
}