using UnityEngine;
using TMPro;

public class ControllerToolDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI toolNameText;    
    public TextMeshProUGUI toolFunctionText; 
    public GameObject panel;                 

    private void Awake()
    {
        if (panel) panel.SetActive(false);
    }

    public void ShowTool(string toolName)
    {
        if (toolNameText) toolNameText.text = toolName;
        if (panel) panel.SetActive(true);
    }

    public void UpdateFunctionText(string text)
    {
        if (toolFunctionText)
            toolFunctionText.text = text;
    }

    public void HideTool()
    {
        if (panel) panel.SetActive(false);
    }
}
