using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SuturingHaptics : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float minPulseInterval = 0.02f;
    public float maxPulseInterval = 0.2f;
    public float maxDistance = 0.2f;
    [Range(0f, 1f)] public float pulseAmplitude = 0.6f;
    public float pulseDuration = 0.02f;

    private Vector3 startPoint;
    private bool startPointSet = false;
    private float lastPulseTime;

    public void PlaySuturingHaptics(XRBaseInputInteractor interactor)
    {
        if (!startPointSet)
        {
            startPoint = transform.position;
            startPointSet = true;
        }

        float distance = Vector3.Distance(transform.position, startPoint);
        float t = Mathf.Clamp01(distance / maxDistance);
        float interval = Mathf.Lerp(minPulseInterval, maxPulseInterval, t);

        if (Time.time - lastPulseTime >= interval)
        {
            interactor.SendHapticImpulse(pulseAmplitude, pulseDuration);
            lastPulseTime = Time.time;
        }
    }

    private void OnDisable()
    {
        startPointSet = false;
    }
}