using UnityEngine;

public class SutureToolHelper : MonoBehaviour
{
    [Header("References")]
    public ToolActionController toolController;

    [Header("Suture Settings")]
    public GameObject sutureSegmentPrefab;

    [Header("Cuttable Mesh Reference")]
    public SkinnedCuttableMesh cuttableMesh;

    private GameObject currentSuture;

    public void PerformSutureFromDrawnLine()
    {
        if (toolController == null || sutureSegmentPrefab == null)
            return;

        Vector3 startPoint = toolController.LastDrawStartPoint;
        Vector3 endPoint = toolController.LastDrawEndPoint;

        if (startPoint == endPoint)
            return;

        // Calculate midpoint
        Vector3 midpoint = (startPoint + endPoint) / 2f;

        // Instantiate prefab at midpoint
        currentSuture = Instantiate(sutureSegmentPrefab, midpoint, Quaternion.identity);

        // Calculate direction and distance
        Vector3 dir = endPoint - startPoint;
        float distance = dir.magnitude;

        // Rotate so prefab points along line
        Vector3 up = dir.normalized;
        Vector3 forward = Vector3.Cross(Vector3.up, up).normalized;
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        currentSuture.transform.rotation = Quaternion.LookRotation(forward, up);

        // Scale prefab to match distance
        Vector3 scale = currentSuture.transform.localScale;
        scale.y = distance; // assuming prefab length is along local Y
        currentSuture.transform.localScale = scale;

        toolController.MarkActionCompleted();

        // Reset skinned mesh cuts
        if (cuttableMesh != null)
            cuttableMesh.ResetMeshCuts();
    }
}