using System;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    AudioSource audioSource;
    string loadedPath = string.Empty;
    bool isPlaying;

    public event Action OnPlaybackCompleted;
    public event Action<bool> OnPlayingStateChanged;

    public bool IsPlaying
    {
        get { return isPlaying; }
    }

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    void Update()
    {
        if (!isPlaying || audioSource == null)
            return;

        if (!audioSource.isPlaying)
        {
            isPlaying = false;
            OnPlayingStateChanged?.Invoke(false);
            OnPlaybackCompleted?.Invoke();
        }
    }

    public bool TryPrepare(string resourcesPath)
    {
        StopAndClear();

        if (string.IsNullOrEmpty(resourcesPath))
            return false;

        var clip = Resources.Load<AudioClip>(resourcesPath);
        if (clip == null)
            return false;

        audioSource.clip = clip;
        loadedPath = resourcesPath;
        return true;
    }

    public bool TryPlay()
    {
        if (audioSource == null || audioSource.clip == null)
            return false;

        audioSource.Play();
        isPlaying = true;
        OnPlayingStateChanged?.Invoke(true);
        return true;
    }

    public void PauseOrStop()
    {
        if (audioSource == null)
            return;

        if (audioSource.isPlaying)
            audioSource.Pause();

        isPlaying = false;
        OnPlayingStateChanged?.Invoke(false);
    }

    public void StopAndClear()
    {
        if (audioSource == null)
            return;

        audioSource.Stop();
        audioSource.clip = null;
        loadedPath = string.Empty;
        isPlaying = false;
        OnPlayingStateChanged?.Invoke(false);
    }

    public string LoadedPath
    {
        get { return loadedPath; }
    }
}
