using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SyringeInjectionHelper : MonoBehaviour
{
    [Header("References")]
    public ToolActionController toolController;

    [Header("Injection Settings")]
    public float injectionDuration = 2.5f;

    private bool injecting;
    private bool paused;
    private float progress;

    private void Update()
    {
        if (!injecting || paused)
            return;

        progress += Time.deltaTime;

        // Update controller display
        if (toolController.controllerDisplay != null)
        {
            float percent = Mathf.Clamp01(progress / injectionDuration) * 100f;
            toolController.controllerDisplay.UpdateFunctionText($"Injected: {percent:F0}%");
        }

        if (progress >= injectionDuration)
            CompleteInjection();
    }

    public void BeginInjection()
    {
        if (injecting && !paused) return; // Already running
        injecting = true;
        paused = false;
    }

    public void PauseInjection()
    {
        if (!injecting || paused) return;
        paused = true;

        if (toolController.controllerDisplay != null)
            toolController.controllerDisplay.UpdateFunctionText("Injection Paused");
    }

    private void CompleteInjection()
    {
        injecting = false;
        paused = false;
        progress = 0f;

        toolController?.MarkActionCompleted();

        if (toolController?.controllerDisplay != null)
            toolController.controllerDisplay.UpdateFunctionText("");
    }
}
