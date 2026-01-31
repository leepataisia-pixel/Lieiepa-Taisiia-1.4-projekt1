using UnityEngine;
using UnityEngine.Video;

public class CutsceneTriggerPlay : MonoBehaviour
{
    [Header("Refs")]
    public VideoPlayer videoPlayer;
    public GameObject cutsceneUI;
    public MonoBehaviour playerController;

    [Header("UI to hide")]
    public GameObject playerHealthBarUI; // ← сюда перетащишь хелсбар

    [Header("Settings")]
    public string playerTag = "Player";
    public bool playOnce = true;

    bool played = false;

    private void Awake()
    {
        if (cutsceneUI != null) cutsceneUI.SetActive(false);

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (playOnce && played) return;

        played = true;
        StartCutscene();
    }

    void StartCutscene()
    {
        // спрятать хелсбар
        if (playerHealthBarUI != null) playerHealthBarUI.SetActive(false);

        // выключить управление
        if (playerController != null) playerController.enabled = false;

        // показать видео UI
        if (cutsceneUI != null) cutsceneUI.SetActive(true);

        if (videoPlayer != null) videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        EndCutscene();
    }

    void EndCutscene()
    {
        // скрыть видео UI
        if (cutsceneUI != null) cutsceneUI.SetActive(false);

        // вернуть управление
        if (playerController != null) playerController.enabled = true;

        // вернуть хелсбар
        if (playerHealthBarUI != null) playerHealthBarUI.SetActive(true);
    }
}