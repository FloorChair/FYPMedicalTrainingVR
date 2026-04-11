using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ForcepsAdvancedHaptics : MonoBehaviour
{
    [Header("References")]
    public ForcepsPickup forcepsPickup;

    [Header("Haptic Settings")]
    public float minAmplitude = 0.1f;
    public float maxAmplitude = 1f;
    public float pulseInterval = 0.05f;
    public float pulseDuration = 0.02f;
    public float rotationSpeedSmoothTime = 0.1f;

    private float lastAngle;
    private float smoothedSpeed;
    private float smoothVelocity;
    private float pulseTimer;
    private bool isWiggling;

    private void OnEnable()
    {
        if (forcepsPickup != null)
            forcepsPickup.OnWriggleProgress += OnWriggleProgress;
    }

    private void OnDisable()
    {
        if (forcepsPickup != null)
            forcepsPickup.OnWriggleProgress -= OnWriggleProgress;
    }

    private void OnWriggleProgress(float progress)
    {
        isWiggling = progress > 0f && progress < 1f;
    }

    private void Update()
    {
        if (!isWiggling) return;

        float currentAngle = transform.eulerAngles.magnitude;
        float rawSpeed = Mathf.Abs(Mathf.DeltaAngle(lastAngle, currentAngle)) / Time.deltaTime;
        lastAngle = currentAngle;

        smoothedSpeed = Mathf.SmoothDamp(smoothedSpeed, rawSpeed, ref smoothVelocity, rotationSpeedSmoothTime);
        pulseTimer -= Time.deltaTime;
    }

    // This gets called by the onAdvancedHaptics event via the Inspector
    public void SendWiggleHaptics(XRBaseInputInteractor interactor)
    {
        if (!isWiggling) return;
        if (pulseTimer > 0f) return;

        float amplitude = Mathf.Lerp(minAmplitude, maxAmplitude, Mathf.Clamp01(smoothedSpeed / 180f));
        interactor.SendHapticImpulse(amplitude, pulseDuration);

        pulseTimer = pulseInterval;
    }
}