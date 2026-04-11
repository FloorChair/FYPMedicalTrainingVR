using UnityEngine;

public class PanelActivationNotifier : MonoBehaviour
{
    private PanelVideoController videoController;

    void Awake()
    {
        videoController = FindFirstObjectByType<PanelVideoController>();
    }

    void OnEnable()
    {
        videoController?.OnPanelChanged(gameObject);
    }
}