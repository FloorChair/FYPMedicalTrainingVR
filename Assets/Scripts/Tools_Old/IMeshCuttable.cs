using UnityEngine;

// This interface allows the Scalpel (or any Tool) to cut the gum
public interface IMeshCuttable
{
    // Called by the Scalpel to cut at a position
    void CutAt(Vector3 worldPosition, Vector3 direction, float radius, float pushDistance);
}
