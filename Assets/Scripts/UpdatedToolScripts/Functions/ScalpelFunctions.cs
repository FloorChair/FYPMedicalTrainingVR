using UnityEngine;

public class ScalpelFunction : MonoBehaviour
{
    [Header("References")]
    public ToolActionController toolController;
    public SkinnedCuttableMesh cuttableMesh;
    public Transform toolTip;

    [Header("Cut Settings")]
    public float cutRadius = 0.01f;

    private bool isCutting = false;
    private Vector3 lastCutPoint;

    private void Update()
    {
        if (isCutting)
            ContinueCut();
    }

    public void BeginCut()
    {
        if (toolController == null || toolTip == null) return;

        isCutting = true;
        lastCutPoint = toolTip.position;
    }

    public void EndCut()
    {
        if (!isCutting) return;

        isCutting = false;
        toolController.MarkActionCompleted();
    }

    private void ContinueCut()
    {
        if (cuttableMesh == null || toolTip == null) return;

        Vector3 currentPoint = toolTip.position;
        float distance = Vector3.Distance(lastCutPoint, currentPoint);

        if (distance < cutRadius * 0.5f) return;

        int steps = Mathf.CeilToInt(distance / cutRadius);
        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector3 point = Vector3.Lerp(lastCutPoint, currentPoint, t);
            cuttableMesh.ProcessCutAtWorldPoint(point, cutRadius);
        }

        lastCutPoint = currentPoint;
    }
}