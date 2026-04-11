using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class ProcedureGenerator : MonoBehaviour
{
    [System.Serializable]
    public class ProcedureStep
    {
        public GameObject tool;
        public Image stepTabImage;
        public GameObject stepPanel;
    }

    [Header("Procedure Steps")]
    public List<ProcedureStep> steps = new List<ProcedureStep>();

    [Header("Results")]
    public string resultFileName = "procedureResult";
    public TMP_Text endPanelScoreText;

    [Header("Global Panels")]
    public GameObject startPanel;
    public GameObject endPanel;
    public GameObject errorPanel;
    public GameObject intermediaryPanel;

    [Header("Highlight Settings")]
    public string highlightLayerName = "Outline";
    public string defaultLayerName = "Default";

    [Header("Tab Colors")]
    public Color defaultTabColor = Color.white;
    public Color activeTabColor = new Color(1f, 0.5f, 0f);

    private int currentIndex = 0;
    private List<float> stepScores = new List<float>();

    private void Start()
    {
        ShowGlobalPanel(startPanel);
        HideAllStepPanels();
        ResetAllTabs();
        HighlightCurrentStep();
    }

    public void BeginProcedure()
    {
        currentIndex = 0;
        stepScores.Clear();
        ProcedureResults.ClearPending();
        ShowGlobalPanel(startPanel);
        HideAllStepPanels();
        ResetAllTabs();
        RemoveAllHighlights();
        HighlightCurrentStep();
    }

    public void ReportToolPickedUp(GameObject tool)
    {
        if (currentIndex >= steps.Count)
            return;
        if (tool != steps[currentIndex].tool)
        {
            ShowGlobalPanel(errorPanel);
            return;
        }
        HideGlobalPanel(errorPanel);
        HideGlobalPanel(startPanel);
        HideGlobalPanel(intermediaryPanel);
        SetToolHighlight(currentIndex, defaultLayerName);
        ShowCurrentStepPanel();
    }

    public void ReportToolActionCompleted(GameObject tool)
    {
        if (currentIndex >= steps.Count)
            return;
        if (tool != steps[currentIndex].tool)
            return;
        HideAllStepPanels();
        SetTabColor(currentIndex, defaultTabColor);
        SetToolHighlight(currentIndex, defaultLayerName);
        currentIndex++;
        if (currentIndex >= steps.Count)
        {
            float finalScore = GetOverallScore();
            ProcedureResults.fileName = resultFileName;
            ProcedureResults.Save(finalScore, ScoreUtility.GetRank(finalScore));
            Debug.Log($"Final Score: {finalScore:F1}% - {ScoreUtility.GetRank(finalScore)}");
            DisplayFinalScore(finalScore);
            ShowGlobalPanel(endPanel);
            return;
        }
        HighlightCurrentStep();
        ShowGlobalPanel(intermediaryPanel);
    }

    public void ReportStepScore(float score)
    {
        stepScores.Add(score);
    }

    private float GetOverallScore()
    {
        if (stepScores.Count == 0) return 0f;
        float total = 0f;
        foreach (float s in stepScores) total += s;
        return total / stepScores.Count;
    }

    private void DisplayFinalScore(float score)
    {
        if (endPanelScoreText == null)
            return;

        endPanelScoreText.text = $"Final Score: {ScoreUtility.GetRank(score)} ({score:F1}%)";
    }

    private void ShowCurrentStepPanel()
    {
        HideAllStepPanels();
        GameObject panel = steps[currentIndex].stepPanel;
        if (panel != null)
            panel.SetActive(true);
    }

    private void HideAllStepPanels()
    {
        foreach (var step in steps)
        {
            if (step.stepPanel != null)
                step.stepPanel.SetActive(false);
        }
    }

    private void ShowGlobalPanel(GameObject panel)
    {
        HideAllGlobalPanels();
        if (panel != null)
            panel.SetActive(true);
    }

    private void HideGlobalPanel(GameObject panel)
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void HideAllGlobalPanels()
    {
        if (startPanel) startPanel.SetActive(false);
        if (endPanel) endPanel.SetActive(false);
        if (errorPanel) errorPanel.SetActive(false);
    }

    private void ResetAllTabs()
    {
        for (int i = 0; i < steps.Count; i++)
            SetTabColor(i, defaultTabColor);
    }

    private void HighlightCurrentStep()
    {
        if (currentIndex < steps.Count)
        {
            SetTabColor(currentIndex, activeTabColor);
            SetToolHighlight(currentIndex, highlightLayerName);
        }
    }

    private void SetToolHighlight(int index, string layerName)
    {
        if (index < 0 || index >= steps.Count) return;
        GameObject tool = steps[index].tool;
        if (tool == null) return;

        int layer = LayerMask.NameToLayer(layerName);
        foreach (var renderer in tool.GetComponentsInChildren<Renderer>())
            renderer.gameObject.layer = layer;
    }

    private void RemoveAllHighlights()
    {
        for (int i = 0; i < steps.Count; i++)
            SetToolHighlight(i, defaultLayerName);
    }

    private void SetTabColor(int index, Color color)
    {
        if (index < 0 || index >= steps.Count)
            return;

        Image img = steps[index].stepTabImage;
        if (img != null)
            img.color = color;
    }
}