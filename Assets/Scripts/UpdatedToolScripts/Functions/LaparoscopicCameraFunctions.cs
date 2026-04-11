using UnityEngine;
public class LaparoscopicCamera : MonoBehaviour
{
    public GameObject cameraObject;
    public Renderer monitorRenderer;
    public Material monitorOnMaterial;
    public Material monitorOffMaterial;

    public void EnableCamera()
    {
        cameraObject.SetActive(true);
        Material[] mats = monitorRenderer.materials;
        mats[2] = monitorOnMaterial;
        monitorRenderer.materials = mats;
    }

    public void DisableCamera()
    {
        cameraObject.SetActive(false);
        Material[] mats = monitorRenderer.materials;
        mats[2] = monitorOffMaterial;
        monitorRenderer.materials = mats;
    }
}