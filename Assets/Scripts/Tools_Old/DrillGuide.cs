using UnityEngine;

public class DrillGuide : MonoBehaviour
{
    [HideInInspector] public bool IsCompleted { get; private set; } = false;

    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
            rend.material.color = Color.red; // default uncut color
    }

    public void MarkCut()
    {
        IsCompleted = true;
        if (rend != null)
            rend.material.color = Color.green; // visually mark as complete
    }
}
