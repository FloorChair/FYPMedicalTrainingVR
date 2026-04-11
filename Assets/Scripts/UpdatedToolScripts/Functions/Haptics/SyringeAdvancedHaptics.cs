using UnityEngine;
public class SyringeAdvancedHaptics : MonoBehaviour
{
    [Header("Haptics")]
    [Range(0f, 1f)] public float maxAmplitude = 1f;
    public float pulseDuration = 0.02f;
    public float maxAngle = 30f;

    private Quaternion insertionRotation;

    public void OnInsertionStart()
    {
        insertionRotation = transform.rotation;
    }

    public void PlayAlignmentHaptics(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor interactor)
    {
        float angle = Quaternion.Angle(transform.rotation, insertionRotation);
        float normalized = Mathf.Clamp01(angle / maxAngle);
        interactor.SendHapticImpulse(normalized * maxAmplitude, pulseDuration);
    }
}