using UnityEngine;

public class RespawnBoundary : MonoBehaviour
{
    private ToolBelt toolBelt;

    void Start()
    {
        toolBelt = FindObjectsByType<ToolBelt>(FindObjectsSortMode.None)[0];
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger hit by: {other.gameObject.name} tag: {other.tag}");
        if (!other.CompareTag("Tool")) return;
        toolBelt.RespawnTool(other.transform);
    }
}
