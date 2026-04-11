using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StitchMarker : MonoBehaviour
{
    public bool IsStitched { get; private set; } = false;

    public void MarkStitched()
    {
        if (IsStitched) return;
        IsStitched = true;

    }
}
