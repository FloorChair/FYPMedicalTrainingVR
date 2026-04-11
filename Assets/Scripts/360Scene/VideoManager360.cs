using UnityEngine;
using System.Collections;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class VideoManager360 : MonoBehaviour
{
    [Header("UI Fade Overlay")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1.0f;

    [Header("Skyboxes")]
    public Material defaultSkybox;
    public Material videoSkybox;

    [Header("Video")]
    public VideoPlayer videoPlayer;

    [Header("Play/Pause Button")]
    public Button playPauseButton;
    public Sprite playIcon;
    public Sprite pauseIcon;

    [Header("Status Text")]
    public TMP_Text statusText;

    [Header("Menu")]
    public GameObject menuPanel;
    public InputActionReference openMenuAction;

    [Header("Selection")]
    public GameObject selectionPanel;

    private bool isPlaying = false;
    private System.Action<InputAction.CallbackContext> menuActionHandler;

    private void Start()
    {
        if (defaultSkybox == null)
            defaultSkybox = RenderSettings.skybox;

        RenderSettings.skybox = defaultSkybox;
        DynamicGI.UpdateEnvironment();

        videoPlayer.Stop();
        videoPlayer.prepareCompleted += OnVideoPrepared;

        if (menuPanel != null)
            menuPanel.SetActive(false);
        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 0f;
        if (selectionPanel != null)
            selectionPanel.SetActive(true);

        if (openMenuAction != null)
        {
            menuActionHandler = _ => ToggleMenu();
            openMenuAction.action.performed += menuActionHandler;
            openMenuAction.action.Enable();
        }

        UpdateButtonIcon();
        UpdateStatusText();
    }

    private void OnDestroy()
    {
        videoPlayer.prepareCompleted -= OnVideoPrepared;

        if (openMenuAction != null && menuActionHandler != null)
            openMenuAction.action.performed -= menuActionHandler;
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        videoPlayer.targetTexture.Release();
        videoPlayer.targetTexture.width = (int)vp.width;
        videoPlayer.targetTexture.height = (int)vp.height;
        videoPlayer.targetTexture.Create();
    }

    public void SelectVideo(VideoClip clip)
    {
        videoPlayer.Stop();
        videoPlayer.clip = clip;

        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        videoPlayer.Prepare();
        TogglePlayPause();
    }

    public void ToggleMenu()
    {
        if (menuPanel == null) return;
        if (selectionPanel != null && selectionPanel.activeSelf) return;

        if (menuPanel.activeSelf)
            StartCoroutine(FadeMenuThenDeactivate());
        else
        {
            menuPanel.SetActive(true);
            StartCoroutine(Fade(1f, fadeDuration));
        }
    }

    public void TogglePlayPause()
    {
        if (isPlaying)
        {
            isPlaying = false;
            videoPlayer.Pause();
        }
        else
        {
            isPlaying = true;
            PlayVideoSkybox();
            StartCoroutine(FadeMenuThenDeactivate());
        }

        UpdateButtonIcon();
        UpdateStatusText();
    }

    public void ExitVideo()
    {
        isPlaying = false;
        StartCoroutine(FadeAndSwitchSkybox(defaultSkybox, videoPlayer.Stop));

        if (selectionPanel != null)
            selectionPanel.SetActive(true);
        if (menuPanel != null)
            menuPanel.SetActive(false);

        UpdateButtonIcon();
        UpdateStatusText();
    }

    public void PlayVideoSkybox()
    {
        StartCoroutine(FadeAndSwitchSkybox(videoSkybox, videoPlayer.Play));
    }

    public void ReturnToDefaultSkybox()
    {
        StartCoroutine(FadeAndSwitchSkybox(defaultSkybox, videoPlayer.Pause));
    }

    private void UpdateButtonIcon()
    {
        if (playPauseButton == null) return;
        Image btnImage = playPauseButton.GetComponent<Image>();
        if (btnImage == null) return;
        btnImage.sprite = isPlaying ? pauseIcon : playIcon;
    }

    private void UpdateStatusText()
    {
        if (statusText == null) return;
        statusText.text = isPlaying ? "Playing" : "Paused";
    }

    private IEnumerator FadeMenuThenDeactivate()
    {
        yield return Fade(0f, fadeDuration);
        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    private IEnumerator FadeAndSwitchSkybox(Material targetMaterial, System.Action onCompleteAction)
    {
        yield return Fade(1f, fadeDuration);
        RenderSettings.skybox = targetMaterial;
        DynamicGI.UpdateEnvironment();
        onCompleteAction?.Invoke();
        yield return Fade(0f, fadeDuration);
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        if (fadeCanvasGroup == null) yield break;
        float startAlpha = fadeCanvasGroup.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
            yield return null;
        }
        fadeCanvasGroup.alpha = targetAlpha;
    }
}