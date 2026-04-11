using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ScalpelCuttingHaptics : MonoBehaviour
{
    [Header("Cutting Feel")]
    [Range(0f, 1f)] public float dragAmplitude = 0.2f;
    [Range(0f, 1f)] public float fiberBumpAmplitude = 0.8f;
    public float distancePerBump = 0.005f;

    private Vector3 lastPos;
    private float distanceTravelled;
    private bool isInsideMesh = false;

    private void OnTriggerEnter(Collider other)
    {
        isInsideMesh = true;
        lastPos = transform.position;
        distanceTravelled = 0f;
    }

    private void OnTriggerExit(Collider other)
    {
        isInsideMesh = false;
    }

    public void PlayCuttingHaptics(XRBaseInputInteractor interactor)
    {
        if (!isInsideMesh) return;

        distanceTravelled += Vector3.Distance(transform.position, lastPos);
        lastPos = transform.position;

        if (distanceTravelled >= distancePerBump)
        {
            interactor.SendHapticImpulse(fiberBumpAmplitude * Random.Range(0.6f, 1f), 0.01f);
            distanceTravelled = 0f;
            return;
        }

        interactor.SendHapticImpulse(dragAmplitude, 0.02f);
    }
}