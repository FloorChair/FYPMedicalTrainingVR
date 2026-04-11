using UnityEngine;
using UnityEngine.UI;

public class SidePanelController : MonoBehaviour
{
    public RectTransform sidePanel;
    public CanvasGroup sidePanelCanvasGroup;
    public CanvasGroup mainPanelCanvasGroup;
    public Image overlay;
    public float animDuration = 0.4f;
    public float overlayMaxAlpha = 0.8f;

    public Vector2 hiddenPos;
    public Vector2 shownPos;

    void Start()
    {

        sidePanel.anchoredPosition = hiddenPos;

        sidePanelCanvasGroup.alpha = 0f;
        sidePanelCanvasGroup.interactable = false;
        sidePanelCanvasGroup.blocksRaycasts = false;

        overlay.color = new Color(1f, 1f, 1f, 0f);
        overlay.raycastTarget = false;
    }

    public void OpenPanel()
    {
        mainPanelCanvasGroup.interactable = false;
        mainPanelCanvasGroup.blocksRaycasts = false;

        overlay.raycastTarget = true;
        LeanTween.alpha(overlay.rectTransform, overlayMaxAlpha, animDuration);

        sidePanelCanvasGroup.interactable = true;
        sidePanelCanvasGroup.blocksRaycasts = true;
        LeanTween.moveLocal(sidePanel.gameObject, shownPos, animDuration).setEase(LeanTweenType.easeOutCubic);
        LeanTween.alphaCanvas(sidePanelCanvasGroup, 1f, animDuration);
    }

    public void ClosePanel()
    {
        mainPanelCanvasGroup.interactable = true;
        mainPanelCanvasGroup.blocksRaycasts = true;

        overlay.raycastTarget = false;
        LeanTween.alpha(overlay.rectTransform, 0f, animDuration);

        sidePanelCanvasGroup.interactable = false;
        sidePanelCanvasGroup.blocksRaycasts = false;
        LeanTween.moveLocal(sidePanel.gameObject, hiddenPos, animDuration).setEase(LeanTweenType.easeInCubic);
        LeanTween.alphaCanvas(sidePanelCanvasGroup, 0f, animDuration);
    }
}