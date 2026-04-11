using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DrillHelper : MonoBehaviour
{
    [Header("References")]
    public Transform drillTip;
    public ToolActionController toolController;
    public XRGrabInteractable grab;

    [Header("Settings")]
    public float snapDistance = 0.05f;
    public float moveSpeed = 0.25f;
    public float verticalTipOffset = 0.0f;

    [Header("Drill Completion Objects")]
    public List<GameObject> objectsToRemove = new();
    public List<GameObject> objectsToSpawn = new();

    private Transform activeGuide;
    private HashSet<Transform> completedGuides = new();

    private bool snapped;
    private bool drilling;
    private float guideProgress;

    private Vector3 guideStartPos;
    private Vector3 guideEndPos;

    private void LateUpdate()
    {
        if (!snapped || !drilling || activeGuide == null)
            return;

        AdvanceAlongGuide();
        AnchorTipToGuide();
    }

    public void TryBeginDrilling()
    {
        if (!snapped)
        {
            activeGuide = FindClosestGuide();
            if (!activeGuide) return;

            LockToGuide(activeGuide);
        }

        drilling = true;
    }

    public void StopDrilling()
    {
        drilling = false;
        Unlock();
    }

    private Transform FindClosestGuide()
    {
        float bestDist = snapDistance;
        Transform closest = null;

        foreach (GameObject guideObj in toolController.associatedGuides)
        {
            if (!guideObj || !guideObj.activeInHierarchy)
                continue;

            Transform guide = guideObj.transform;
            if (completedGuides.Contains(guide))
                continue;

            float d = Vector3.Distance(drillTip.position, guide.position);
            if (d <= bestDist)
            {
                bestDist = d;
                closest = guide;
            }
        }

        return closest;
    }

    private void LockToGuide(Transform guide)
    {
        snapped = true;

        ComputeGuideEndpoints(guide, out guideStartPos, out guideEndPos);
        guideProgress = 0f;

        grab.trackPosition = false;
        grab.trackRotation = true;

        AnchorTipToGuide();
    }

    private void Unlock()
    {
        snapped = false;
        drilling = false;

        grab.trackPosition = true;
        grab.trackRotation = true;
    }

    private void AdvanceAlongGuide()
    {
        guideProgress += moveSpeed * Time.deltaTime;

        if (guideProgress >= 1f)
        {
            guideProgress = 1f;
            CompleteDrilling();
        }
    }

    private void AnchorTipToGuide()
    {
        Vector3 guidePos = Vector3.Lerp(guideStartPos, guideEndPos, guideProgress);

        // Keep tip fixed
        Vector3 correction = guidePos - drillTip.position;
        transform.position += correction;

        // Optional vertical offset
        if (verticalTipOffset != 0f)
            transform.position += Vector3.up * verticalTipOffset;
    }

    private void CompleteDrilling()
    {
        drilling = false;

        if (activeGuide)
        {
            completedGuides.Add(activeGuide);

            foreach (var obj in objectsToRemove)
                if (obj) Destroy(obj);

            foreach (var obj in objectsToSpawn)
                if (obj) obj.SetActive(true);

            if (completedGuides.Count >= GetTotalValidGuides())
                toolController.MarkActionCompleted();
        }

        activeGuide = null;
        Unlock();
    }

    private void ComputeGuideEndpoints(Transform guide, out Vector3 start, out Vector3 end)
    {
        Renderer r = guide.GetComponent<Renderer>();
        if (!r)
        {
            start = end = guide.position;
            return;
        }

        Bounds b = r.bounds;

        Vector3 right = guide.right;
        Vector3 forward = guide.forward;

        float xExtent = Vector3.Project(b.size, right).magnitude * 0.5f;
        float zExtent = Vector3.Project(b.size, forward).magnitude * 0.5f;

        Vector3 axis = xExtent >= zExtent ? right : forward;
        float halfLength = Mathf.Max(xExtent, zExtent);

        start = b.center - axis * halfLength;
        end = b.center + axis * halfLength;
    }

    private int GetTotalValidGuides()
    {
        int count = 0;
        foreach (var g in toolController.associatedGuides)
            if (g) count++;
        return count;
    }
}