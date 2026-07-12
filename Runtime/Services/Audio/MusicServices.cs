using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MusicServices : SingletonGlobal<MusicServices>, IGameFoundationModule
{
    public enum Music
    {
        NONE,
    }

    [System.Serializable]
    public class MusicTable
    {
        public Music music;
        public AudioClip clip;
    }

    [SerializeField] private MusicTable[] musics;
    [SerializeField] private AudioSource audioSource;

    private readonly Dictionary<Music, AudioClip> musicDics = new Dictionary<Music, AudioClip>();
    private const string SettingKeyMusic = "MUSIC";
    private bool isInitialized;

    public int Order => -50;

    public IEnumerator Initialize()
    {
        EnsureInitialized();
        SetMute();
        yield break;
    }

    public void PlayMusic(Music music, float volume = 1.0f, bool loop = true)
    {
        EnsureInitialized();
        if (!musicDics.TryGetValue(music, out var clip) || clip == null)
            return;

        if (audioSource.isPlaying && audioSource.clip == clip)
            return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.volume = volume;
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
        DOTween.To(() => audioSource.volume, x => audioSource.volume = x, toVolume, duration).SetTarget(audioSource);
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

        musicDics.Clear();
        if (musics == null) return;

        foreach (var m in musics)
        {
            if (m.clip && !musicDics.ContainsKey(m.music))
                musicDics.Add(m.music, m.clip);
        }
    }
}
