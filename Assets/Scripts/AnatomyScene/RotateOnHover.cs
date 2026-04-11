using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RotateOnHover : MonoBehaviour
{

    public float rotationSpeed = 30f;
    private bool isHovered = false;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        interactable.hoverEntered.AddListener(OnHoverEnter);
        interactable.hoverExited.AddListener(OnHoverExit);
    }

    private void OnDestroy()
    {
        interactable.hoverEntered.RemoveListener(OnHoverEnter);
        interactable.hoverExited.RemoveListener(OnHoverExit);
    }

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        isHovered = true;
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        isHovered = false;
    }

    void Update()
    {
        if (isHovered)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}
