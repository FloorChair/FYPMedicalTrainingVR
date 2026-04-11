using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class OptionsButtonHandler : MonoBehaviour
{
    public InputActionReference optionsButtonAction;
    public OptionsMenuManager menuManager;

    private void OnEnable()
    {
        optionsButtonAction.action.Enable();
        optionsButtonAction.action.performed += OnOptionsPressed;
    }

    private void OnDisable()
    {
        optionsButtonAction.action.performed -= OnOptionsPressed;
        optionsButtonAction.action.Disable();
    }

    private void OnOptionsPressed(InputAction.CallbackContext context)
    {
        // Toggle the menu
        if (menuManager.optionsMenu.activeSelf)
            menuManager.Resume();
        else
            menuManager.OpenOptionsMenu();
    }
}
