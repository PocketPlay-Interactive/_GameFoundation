using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[ExtensionModuleNotice("Drag this MusicService object into Manager -> Extension Modules.")]
public class MusicService : SingletonGlobal<MusicService>, IGameFoundationModule
{
    [System.Serializable]
    public class MusicTable
    {
        public string key;
        public AudioClip clip;
    }

    [SerializeField] private MusicTable[] musics;
    [SerializeField] private AudioSource audioSource;
    [Range(0f, 1f)]
    [SerializeField] private float maxVolume = 1f;

    private readonly Dictionary<string, AudioClip> musicKeyDics = new Dictionary<string, AudioClip>(StringComparer.Ordinal);
    private const string SettingKeyMusic = "MUSIC";
    private bool isInitialized;

    public int Order => -50;

    public IEnumerator Initialize()
    {
        EnsureInitialized();
        SetMute();
        yield break;
    }

    public void PlayMusic(string key, float volume = 1.0f, bool loop = true)
    {
        EnsureInitialized();
        if (string.IsNullOrWhiteSpace(key) || !musicKeyDics.TryGetValue(key, out var clip) || clip == null)
            return;

        if (audioSource.isPlaying && audioSource.clip == clip)
            return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.volume = Mathf.Clamp(volume, 0f, maxVolume);
        audioSource.Play();
    }

    public void StopMusic()
    {
        EnsureInitialized();
        audioSource.Stop();
    }

    public void PauseMusic()
    {
        EnsureInitialized();
        audioSource.Pause();
    }

    public void ResumeMusic()
    {
        EnsureInitialized();
        audioSource.UnPause();
    }

    public void SetMute(bool isMute)
    {
        EnsureInitialized();
        audioSource.mute = isMute;
    }

    public void SetMute()
    {
        SetMute(!RuntimeStorageData.SETTING.Get(SettingKeyMusic, true));
    }

    public void SetVolume(float toVolume, float duration = 1f)
    {
        EnsureInitialized();
        DOTween.Kill(audioSource);
        DOTween.To(() => audioSource.volume, x => audioSource.volume = x, Mathf.Clamp(toVolume, 0f, maxVolume), duration).SetTarget(audioSource);
    }

    public bool IsPlaying
    {
        get
        {
            EnsureInitialized();
            return audioSource.isPlaying;
        }
    }

    public float CurrentVolume
    {
        get
        {
            EnsureInitialized();
            return audioSource.volume;
        }
    }

    public AudioClip CurrentClip
    {
        get
        {
            EnsureInitialized();
            return audioSource.clip;
        }
    }

    private void EnsureInitialized()
    {
        if (isInitialized) return;
        isInitialized = true;

        if (!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = true;
        }

        musicKeyDics.Clear();
        if (musics == null) return;

        foreach (var m in musics)
        {
            string key = m.key;
            if (m.clip && !string.IsNullOrWhiteSpace(key) && !musicKeyDics.ContainsKey(key))
                musicKeyDics.Add(key, m.clip);
        }
    }
}
