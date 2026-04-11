using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;

[System.Serializable]
public class ToolPanel
{
    public XRGrabInteractable tool; // tool to watch
    public GameObject panel;        // panel to show when this tool is picked up
}

public class ToolPanelManager : MonoBehaviour
{
    [Header("Tool Panels")]
    public ToolPanel[] toolPanels;

    [Header("Default Panel")]
    public GameObject defaultPanel; // panel to show when no tool is held

    [Header("Finish Panel")]
    public GameObject FinalResultsPanel;

    [Header("Final Results UI")]
    public TextMeshProUGUI finalScoreText;

    private GameObject activePanel = null;
    private bool hasStarted = false; // to prevent showing default panel on startup

    private void OnEnable()
    {
        foreach (var tp in toolPanels)
        {
            if (tp.tool != null)
            {
                tp.tool.selectEntered.AddListener((args) => OnToolPickedUp(tp));
                tp.tool.selectExited.AddListener((args) => OnToolDropped(tp));
            }
        }
    }

    private void Start()
    {
        hasStarted = true;

        // Hide all tool panels initially
        foreach (var tp in toolPanels)
        {
            if (tp.panel != null)
                tp.panel.SetActive(false);
        }

        // Keep default panel as configured in the editor
        if (defaultPanel != null)
            defaultPanel.SetActive(defaultPanel.activeSelf);
    }

    private void OnDisable()
    {
        foreach (var tp in toolPanels)
        {
            if (tp.tool != null)
            {
                tp.tool.selectEntered.RemoveAllListeners();
                tp.tool.selectExited.RemoveAllListeners();
            }
        }
    }

    private void OnToolPickedUp(ToolPanel tp)
    {
        // Hide currently active panel
        if (activePanel != null)
            activePanel.SetActive(false);

        // Hide default panel
        if (defaultPanel != null)
            defaultPanel.SetActive(false);

        // Show the picked-up tool’s panel
        if (tp.panel != null)
        {
            tp.panel.SetActive(true);
            activePanel = tp.panel;
        }
    }

    private void OnToolDropped(ToolPanel tp)
    {
        // Hide this tool’s panel
        if (tp.panel != null)
        {
            tp.panel.SetActive(false);
            activePanel = null;
        }

        // Only show default panel after first drop
        if (hasStarted)
            ShowDefaultPanel();
    }

    private void ShowDefaultPanel()
    {
        if (defaultPanel != null)
            defaultPanel.SetActive(true);
    }

    public void ShowFinalResults()
    {
        // Hide and lock all tool panels
        foreach (var tp in toolPanels)
        {
            if (tp.panel != null)
                tp.panel.SetActive(false);
        }

        // Disable all tool interactions permanently
        foreach (var tp in toolPanels)
        {
            if (tp.tool != null)
                tp.tool.enabled = false;
        }

        // Hide the default panel if it exists
        if (defaultPanel != null)
            defaultPanel.SetActive(false);

        // Hide any active tool panel reference
        activePanel = null;

        // Show final results panel
        if (FinalResultsPanel != null)
        {
            FinalResultsPanel.SetActive(true);

            int totalScore = ScoreManager.Instance.GetTotalScore();
            if (finalScoreText != null)
                finalScoreText.text = $"Final Score: {totalScore}";
        }
    }

}
