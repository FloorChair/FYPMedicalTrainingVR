using UnityEngine;
using System.Collections.Generic;


public class BoundaryFader : MonoBehaviour
{
    public Transform boundaryCenter;   // static boundary reference
    public Transform itemContainer;    // moving carousel
    public float boundarySize = 3f;    // width of visible area
    public float fadeDistance = 0.5f;  // distance to fully shrink
    public float minScale = 0.01f;     // scale when fully “gone”
    public float maxScale = 0.8f;
    public float shrinkSpeed = 5f;     // smoothing speed

    public List<Transform> activeItems = new List<Transform>();

    private Transform[] items;

    void Start()
    {
        int childCount = itemContainer.childCount;
        items = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            items[i] = itemContainer.GetChild(i);
            items[i].localScale = Vector3.one; // ensure starts at full size
            activeItems.Add(items[i]);
        }
    }

    void Update()
    {
        float halfSize = boundarySize / 2f;

        foreach (Transform item in activeItems)
        {
            // Position relative to boundary center
            Vector3 localPos = boundaryCenter.InverseTransformPoint(item.position);
            float dist = Mathf.Max(0f, Mathf.Abs(localPos.x) - halfSize);

            // Target scale (1 inside, minScale outside)
            float t = Mathf.Clamp01(1f - (dist / fadeDistance));
            float targetScale = Mathf.Lerp(minScale, maxScale, t);

            // Smoothly interpolate top-level scale
            item.localScale = Vector3.Lerp(item.localScale, Vector3.one * targetScale, Time.deltaTime * shrinkSpeed);

            // Disable grab interactable when fully “gone”
            UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab = item.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grab != null)
                grab.enabled = t > 0.05f;
        }
    }

    public void RemoveFromFade(Transform item)
    {
        activeItems.Remove(item);
    }

    public void AddToFade(Transform item)
    {
        if (!activeItems.Contains(item))
        {
            activeItems.Add(item);
        }
    }
}
