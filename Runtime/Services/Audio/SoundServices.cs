using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SoundServices : SingletonGlobal<SoundServices>, IGameFoundationModule
{
    public enum Sound
    {
        NONE,
        BUTTON,
        LOSE,
        WIN,
    }

    [Serializable]
    public class SoundTable
    {
        public Sound sound;
        public AudioClip[] clips;
    }

    [SerializeField] private SoundTable[] sounds;
    [SerializeField] private AudioSource audioSourceSpecial;
    [SerializeField] private int audioSourceCount = 20;
    [SerializeField] private float minIntervalSameSound = 0.05f;
    [SerializeField] private bool overrideAudioVoiceConfig = false;

    private readonly List<AudioSource> audioSources = new List<AudioSource>();
    private readonly Dictionary<Sound, AudioClip[]> soundDics = new Dictionary<Sound, AudioClip[]>();
    private readonly Dictionary<Sound, float> lastPlayTime = new Dictionary<Sound, float>();
    private const string SettingKeySound = "SOUND";
    private int currentAudioSourceIndex;
    private bool isInitialized;

    public int Order => -50;

    public IEnumerator Initialize()
    {
        EnsureInitialized();
        SetMute();
        yield break;
    }

    public void SetMute()
    {
        EnsureInitialized();
        bool mute = !RuntimeStorageData.SETTING.Get(SettingKeySound, true);

        foreach (var src in audioSources)
            if (src) src.mute = mute;

        if (audioSourceSpecial) audioSourceSpecial.mute = mute;
    }

    public void VolumeChange(float toVolume)
    {
        EnsureInitialized();

        foreach (var src in audioSources)
            if (src)
            {
                DOTween.Kill(src);
                DOTween.To(() => src.volume, x => src.volume = x, toVolume, 1f).SetTarget(src);
            }

        if (audioSourceSpecial)
        {
            DOTween.Kill(audioSourceSpecial);
            DOTween.To(() => audioSourceSpecial.volume, x => audioSourceSpecial.volume = x, toVolume, 1f).SetTarget(audioSourceSpecial);
        }
    }

    public void PlayOnShot(Sound sound, float volume = 1f)
    {
        EnsureInitialized();
        if (!soundDics.TryGetValue(sound, out var clips) || clips.Length == 0) return;

        float now = Time.unscaledTime;
        if (lastPlayTime.TryGetValue(sound, out float last) && now - last < minIntervalSameSound)
            return;

        lastPlayTime[sound] = now;
        PlayClipDirect(clips[UnityEngine.Random.Range(0, clips.Length)], volume);
    }

    public void PlayOnShot(AudioClip clip, float volume = 1f)
    {
        PlayClipDirect(clip, volume);
    }

    public void PlaySpecialSound(Sound sound, float volume = 1.0f)
    {
        EnsureInitialized();
        if (!soundDics.TryGetValue(sound, out var clips) || clips.Length == 0) return;

        var clip = clips[UnityEngine.Random.Range(0, clips.Length)];
        if (!audioSourceSpecial || !clip) return;

        audioSourceSpecial.volume = volume;
        audioSourceSpecial.clip = clip;
        audioSourceSpecial.Play();
    }

    public void StopSpecialSound()
    {
        if (audioSourceSpecial) audioSourceSpecial.Stop();
    }

    public AudioClip GetRandomClip(Sound sound)
    {
        EnsureInitialized();

        if (soundDics.TryGetValue(sound, out var clips) && clips.Length > 0)
            return clips[UnityEngine.Random.Range(0, clips.Length)];

        return null;
    }

    public AudioClip ConvertToClip(Sound sound) => GetRandomClip(sound);

    public void PlayClipDirect(AudioClip clip, float volume = 1f)
    {
        EnsureInitialized();
        if (clip == null || audioSources.Count == 0) return;

        AudioSource availableSrc = null;
        foreach (var src in audioSources)
        {
            if (!src.isPlaying)
            {
                availableSrc = src;
                break;
            }
        }

        if (availableSrc == null)
        {
            availableSrc = audioSources[currentAudioSourceIndex];
            currentAudioSourceIndex = (currentAudioSourceIndex + 1) % audioSources.Count;
        }

        if (availableSrc == null) return;

        availableSrc.clip = clip;
        availableSrc.volume = volume;
        availableSrc.Play();
    }

    private void EnsureInitialized()
    {
        if (isInitialized) return;
        isInitialized = true;

        if (overrideAudioVoiceConfig)
        {
            var config = AudioSettings.GetConfiguration();
            config.numVirtualVoices = 512;
            config.numRealVoices = 32;
            AudioSettings.Reset(config);
        }

        soundDics.Clear();
        if (sounds != null)
        {
            foreach (var s in sounds)
            {
                if (s.clips != null && s.clips.Length > 0 && !soundDics.ContainsKey(s.sound))
                    soundDics.Add(s.sound, s.clips);
            }
        }

        audioSources.Clear();
        int count = Mathf.Max(1, audioSourceCount);
        for (int i = 0; i < count; i++)
        {
            GameObject go = new GameObject($"AudioSource_{i}");
            go.transform.SetParent(transform);

            AudioSource src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.priority = 128;
            audioSources.Add(src);
        }

        Debug.Log($"[SoundManager] Initialized with {count} AudioSources");
    }
}
