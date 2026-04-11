using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CutMarker : MonoBehaviour
{
    public bool IsCut { get; private set; } = false;

    [Header("Visuals")]
    public Color touchedColor = Color.green;
    public Color defaultColor = Color.red;
    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
            rend.material.color = defaultColor;
    }

    public void MarkCut()
    {
        if (IsCut) return;
        IsCut = true;

        if (rend != null)
            rend.material.color = touchedColor;
    }
}
