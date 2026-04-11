using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class ToothBehaviour : MonoBehaviour, ICuttable, IExtractable
{
    [HideInInspector] public bool isAnesthetized = false;
    [HideInInspector] public bool isCut = false;
    [HideInInspector] public bool isExtracted = false;

    private Renderer rend;

    // Reference to prefab for creating halves
    public GameObject toothPrefab;

    public bool IsCut => isCut;
    public bool IsExtracted => isExtracted;

    public InjectionMarker[] markers;

    // Whether tooth can be grabbed
    public bool IsGrabbable => isCut;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    public void CheckAnesthesiaCompletion()
    {
        foreach (var marker in markers)
        {
            if (!marker.IsCompleted) return;
        }

        isAnesthetized = true;

        // Change colour when anesthesia complete
        if (rend != null)
            rend.material.color = Color.cyan;
    }

    public void Cut()
    {
        if (!isAnesthetized || isCut) return;

        isCut = true;

        // Split the tooth into halves
        SplitVertically();
    }

    private void SplitVertically()
    {
        if (toothPrefab == null)
        {
            Debug.LogError("Tooth prefab not assigned!");
            return;
        }

        Vector3 originalPos = transform.position;
        Quaternion originalRot = transform.rotation;
        Vector3 originalScale = transform.localScale;
        Transform parent = transform.parent;

        float driftCorrection = 0.043f;  // compensate pivot drift
        float visualGap = 0.01f;        // small visual gap

        // Corrected base position
        Vector3 correctedPos = originalPos - transform.right * driftCorrection;

        // Left half
        GameObject left = Instantiate(toothPrefab, correctedPos - transform.right * visualGap, originalRot, parent);
        left.name = name + "_Left";
        left.transform.localScale = new Vector3(originalScale.x * 0.5f, originalScale.y, originalScale.z);
        InitHalf(left);

        // Right half
        GameObject right = Instantiate(toothPrefab, correctedPos + transform.right * visualGap, originalRot, parent);
        right.name = name + "_Right";
        right.transform.localScale = new Vector3(originalScale.x * 0.5f, originalScale.y, originalScale.z);
        InitHalf(right);

        // Destroy original tooth
        Destroy(gameObject);
    }

    // Initialize each half independently
    private void InitHalf(GameObject half)
    {
        ToothBehaviour tb = half.GetComponent<ToothBehaviour>();
        if (tb != null)
        {
            tb.isCut = true;       // already cut
            tb.isExtracted = false; // not yet extracted
            tb.isAnesthetized = true;
        }
    }

    public void Extract()
    {
        if (!isCut || isExtracted) return;

        isExtracted = true;

        if (rend != null)
            rend.material.color = Color.white;
    }

}
