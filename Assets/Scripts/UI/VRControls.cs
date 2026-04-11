using UnityEngine;
using UnityEngine.InputSystem;

public class VRControls : MonoBehaviour
{
    [Header("Zoom Panels")]
    public GameObject panel1;
    public GameObject panel2;
    public Camera zoomCamera1;
    public Camera zoomCamera2;
    public float defaultFOV = 60f;
    public float zoomedFOV = 30f;
    public float zoomSpeed = 5f;

    [Header("Light")]
    public Light spotlight;

    [Header("Input")]
    public InputActionReference panelAction;
    public InputActionReference lightAction;

    private bool panelsActive = false;
    private bool lightActive = false;

    void Start()
    {
        panelsActive = false;
        lightActive = false;
        panel1.SetActive(false);
        panel2.SetActive(false);
        spotlight.enabled = false;
        zoomCamera1.fieldOfView = defaultFOV;
        zoomCamera2.fieldOfView = defaultFOV;
    }

    void OnEnable()
    {
        panelAction.action.Enable();
        lightAction.action.Enable();

        panelAction.action.performed += TogglePanels;
        lightAction.action.performed += ToggleLight;
    }

    void OnDisable()
    {
        panelAction.action.performed -= TogglePanels;
        lightAction.action.performed -= ToggleLight;

        panelAction.action.Disable();
        lightAction.action.Disable();
    }

    void Update()
    {
        float targetFOV = panelsActive ? zoomedFOV : defaultFOV;

        zoomCamera1.fieldOfView = Mathf.Lerp(zoomCamera1.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
        zoomCamera2.fieldOfView = Mathf.Lerp(zoomCamera2.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
    }

    void TogglePanels(InputAction.CallbackContext ctx)
    {
        panelsActive = !panelsActive;
        panel1.SetActive(panelsActive);
        panel2.SetActive(panelsActive);
    }

    void ToggleLight(InputAction.CallbackContext ctx)
    {
        lightActive = !lightActive;
        spotlight.enabled = lightActive;
    }
}