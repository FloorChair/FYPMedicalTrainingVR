using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenuManager : MonoBehaviour
{
    [Header("Menu References")]
    public GameObject optionsMenu; // The canvas
    public string previousSceneName; // Set in inspector for exit

    void Start()
    {
        // Ensure menu is hidden initially
        optionsMenu.SetActive(false);
    }

    // Called by the options button
    public void OpenOptionsMenu()
    {
        optionsMenu.SetActive(true);
    }

    // Called by Resume button
    public void Resume()
    {
        optionsMenu.SetActive(false);
    }

    // Called by Restart button
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Called by Exit button
    public void Exit()
    {
        if (!string.IsNullOrEmpty(previousSceneName))
            SceneManager.LoadScene(previousSceneName);
        else
            Debug.LogWarning("Previous scene name not set for Exit!");
    }
}
