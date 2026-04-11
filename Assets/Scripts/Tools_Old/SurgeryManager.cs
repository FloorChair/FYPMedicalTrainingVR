using UnityEngine;


public class SurgeryManager : MonoBehaviour
{
    public static SurgeryManager Instance { get; private set; }
    public bool SurgeryStarted { get; private set; } = false;

    [Header("UI Panels")]
    public GameObject startPanel;   // the panel with the Start button
    public GameObject surgeryPanel; // the panel to show after starting

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        // Disable all XRGrabInteractables initially (modern API)
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable[] tools =
            Object.FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>(FindObjectsSortMode.None);
        foreach (var tool in tools)
        {
            tool.enabled = false;
        }

        // Make sure panels are in the correct initial state
        if (startPanel != null) startPanel.SetActive(true);
        if (surgeryPanel != null) surgeryPanel.SetActive(false);
    }

    public void StartSurgery()
    {
        SurgeryStarted = true;

        // Enable all tools (modern API)
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable[] tools =
            Object.FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>(FindObjectsSortMode.None);
        foreach (var tool in tools)
        {
            tool.enabled = true;
        }

        // Toggle panels
        if (startPanel != null) startPanel.SetActive(false);
        if (surgeryPanel != null) surgeryPanel.SetActive(true);
    }
}
