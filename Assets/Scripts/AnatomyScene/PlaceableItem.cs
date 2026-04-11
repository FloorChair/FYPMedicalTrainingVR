using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlaceableItem : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Transform currentPodiumSnapPoint;
    private bool onPodium = false;

    [Header("UI Feedback")]
    public GameObject placedUIPanel;

    [Header("Rotation Settings")]
    public float rotationSpeed = 30f;

    [Header("Highlight Settings")]
    public GameObject highlightCube;
    public Transform highlightTarget;

    void Awake()
    {
        if (grabInteractable == null)
            grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grabInteractable.selectExited.AddListener(OnReleased);
        if (placedUIPanel != null)
            placedUIPanel.SetActive(false);
    }

    void OnDestroy()
    {
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.CompareTag("Podium"))
        {
            Transform snapPoint = other.transform.parent.Find("SnapPoint");
            currentPodiumSnapPoint = snapPoint != null ? snapPoint : other.transform.parent;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.CompareTag("Podium"))
        {
            currentPodiumSnapPoint = null;
        }
    }

    void OnReleased(SelectExitEventArgs args)
    {
        if (currentPodiumSnapPoint != null)
        {
            transform.position = currentPodiumSnapPoint.position;
            transform.rotation = currentPodiumSnapPoint.rotation;
            transform.SetParent(currentPodiumSnapPoint);
            onPodium = true;

            if (placedUIPanel != null)
                placedUIPanel.SetActive(true);

            // Show highlight
            if (highlightCube != null && highlightTarget != null)
            {
                highlightCube.transform.SetParent(highlightTarget);
                highlightCube.transform.localPosition = Vector3.zero;
                highlightCube.transform.localRotation = Quaternion.identity;
                highlightCube.SetActive(true);
            }

            return;
        }

        transform.SetParent(null);
        onPodium = false;

        if (placedUIPanel != null)
            placedUIPanel.SetActive(false);

        // Hide highlight
        if (highlightCube != null)
            highlightCube.SetActive(false);
    }

    void Update()
    {
        if (onPodium && currentPodiumSnapPoint != null)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}