using UnityEngine;
using UnityEngine.EventSystems;

public class HoverGrow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float scaleFactor = 1.1f;    // how much bigger on hover
    public float duration = 0.2f;       // animation speed

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;   // cache original size
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, originalScale * scaleFactor, duration).setEaseOutCubic();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, originalScale, duration).setEaseOutCubic();
    }
}
