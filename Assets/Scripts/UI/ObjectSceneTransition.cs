using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;


public class ObjectSceneTransition : MonoBehaviour
{
    public string sceneName; // Set in Inspector
    public Material highlightMaterial; // Drag a glow/bright material in Inspector

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;
    private Material originalMaterial;
    private Renderer rend;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        rend = GetComponent<Renderer>();
        originalMaterial = rend.material;

        interactable.hoverEntered.AddListener(OnHoverEnter);
        interactable.hoverExited.AddListener(OnHoverExit);
        interactable.selectEntered.AddListener(OnSelect);
    }

    void OnHoverEnter(HoverEnterEventArgs args)
    {
        rend.material = highlightMaterial; // swap to highlight
    }

    void OnHoverExit(HoverExitEventArgs args)
    {
        rend.material = originalMaterial; // revert
    }

    void OnSelect(SelectEnterEventArgs args)
    {
        SceneManager.LoadScene(sceneName);
    }

    void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.hoverEntered.RemoveListener(OnHoverEnter);
            interactable.hoverExited.RemoveListener(OnHoverExit);
            interactable.selectEntered.RemoveListener(OnSelect);
        }
    }
}
