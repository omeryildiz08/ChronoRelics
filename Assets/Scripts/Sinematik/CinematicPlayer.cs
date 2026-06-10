using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class CinematicPlayer : MonoBehaviour
{
    public static CinematicPlayer Instance { get; private set; }

    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private VideoClip introClip;

    [Header("UI")]
    [SerializeField] private GameObject cinematicRoot;
    [SerializeField] private RawImage videoRawImage;

    [Header("Options")]
    [SerializeField] private bool allowSkipWithEscape = true;
    [SerializeField] private bool pauseGameWhilePlaying = true;

    private Action onCinematicFinished;
    private bool isPlaying;
    private float previousTimeScale = 1f;

    public bool IsPlaying => isPlaying;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (cinematicRoot != null)
        {
            cinematicRoot.SetActive(false);
        }

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.loopPointReached += HandleVideoFinished;
            videoPlayer.errorReceived += HandleVideoError;
        }
    }

    private void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        if (allowSkipWithEscape && Input.GetKeyDown(KeyCode.Escape))
        {
            StopCinematicAndFinish();
        }
    }

    public void PlayIntroThenLoadScene(string sceneName)
    {
        PlayCinematic(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        });
    }

    public void PlayCinematicInCurrentScene()
    {
        PlayCinematic(null);
    }

    public void PlayCinematic(Action onFinished)
    {
        if (videoPlayer == null || introClip == null)
        {
            Debug.LogWarning("CinematicPlayer: VideoPlayer veya introClip atanmadı.");

            onFinished?.Invoke();
            return;
        }

        if (isPlaying)
        {
            return;
        }

        isPlaying = true;
        onCinematicFinished = onFinished;

        if (pauseGameWhilePlaying)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        if (cinematicRoot != null)
        {
            cinematicRoot.SetActive(true);
        }

        videoPlayer.Stop();
        videoPlayer.clip = introClip;
        videoPlayer.time = 0;
        videoPlayer.Play();
    }

    private void HandleVideoFinished(VideoPlayer source)
    {
        StopCinematicAndFinish();
    }

    private void HandleVideoError(VideoPlayer source, string message)
    {
        Debug.LogWarning($"CinematicPlayer video error: {message}");
        StopCinematicAndFinish();
    }

    private void StopCinematicAndFinish()
    {
        if (!isPlaying)
        {
            return;
        }

        isPlaying = false;

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        if (cinematicRoot != null)
        {
            cinematicRoot.SetActive(false);
        }

        if (pauseGameWhilePlaying)
        {
            Time.timeScale = previousTimeScale;
        }

        Action callback = onCinematicFinished;
        onCinematicFinished = null;
        callback?.Invoke();
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= HandleVideoFinished;
            videoPlayer.errorReceived -= HandleVideoError;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}