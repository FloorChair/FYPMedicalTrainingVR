using UnityEngine;

public class InjectionMarker : MonoBehaviour, IInjectable
{
    [Header("Tooth & Injection Settings")]
    public ToothBehaviour parentTooth;
    public float targetDose = 1.2f; // ideal injection amount
    public float maxDose = 2f;      // auto-complete if exceeded

    private float holdProgress = 0f;
    private bool completed = false;
    private Renderer rend;
    private float score = 0f;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
            rend.material.color = Color.red;
    }

    public bool IsCompleted => completed;
    public float Score => score;
    public float InjectedAmount => holdProgress;

    public void ResetProgress()
    {
        holdProgress = 0f;
        completed = false;
        score = 0f;
        if (rend != null)
            rend.material.color = Color.red;
    }

    public void Inject(float deltaTime)
    {
        if (completed) return;

        holdProgress += deltaTime;
        if (holdProgress > maxDose) holdProgress = maxDose; // cap injection

        // Update color based on % of maxDose
        if (rend != null)
        {
            float t = Mathf.Clamp01(holdProgress / maxDose);
            rend.material.color = Color.Lerp(Color.red, Color.green, t);
        }
    }

    public void CompleteInjection()
    {
        if (completed) return;

        completed = true;
        CalculateScore();
        gameObject.SetActive(false);
        parentTooth?.CheckAnesthesiaCompletion();
    }

    private void CalculateScore()
    {
        float difference = Mathf.Abs(holdProgress - targetDose);
        float tolerance = targetDose * 0.5f; // 50% tolerance
        score = Mathf.Clamp01(1f - (difference / tolerance)) * 100f;
        if (score < 0f) score = 0f;
    }
}
