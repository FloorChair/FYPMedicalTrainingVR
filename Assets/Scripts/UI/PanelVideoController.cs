using UnityEngine;
using UnityEngine.Video;

public class PanelVideoController : MonoBehaviour
{
    [System.Serializable]
    public class PanelVideoEntry
    {
        public GameObject panel;
        public VideoClip videoClip;
    }

    [Header("Video Player")]
    public VideoPlayer videoPlayer;

    [Header("Panel to Video Mappings")]
    public PanelVideoEntry[] entries;

    public void OnPanelChanged(GameObject activePanel)
    {
        if (videoPlayer == null) return;
        foreach (var entry in entries)
        {
            if (entry.panel == activePanel)
            {
                if (entry.videoClip != null)
                {
                    videoPlayer.Stop();
                    videoPlayer.clip = entry.videoClip;
                    videoPlayer.Play();
                }
                else
                {
                    videoPlayer.Stop();
                    videoPlayer.clip = null;
                }
                return;
            }
        }
        videoPlayer.Stop();
        videoPlayer.clip = null;
    }
}