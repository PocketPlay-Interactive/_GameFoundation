using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[ExtensionModuleNotice("Drag this SoundService object into Manager -> Extension Modules.")]
public class SoundService : SingletonGlobal<SoundService>, IGameFoundationModule
{
    [Serializable]
    public class SoundTable
    {
        public string key;
        public AudioClip[] clips;
    }

    [SerializeField] private SoundTable[] sounds;
    [SerializeField] private AudioSource audioSourceSpecial;
    [SerializeField] private int audioSourceCount = 20;
    [SerializeField] private float minIntervalSameSound = 0.05f;
    [SerializeField] private bool overrideAudioVoiceConfig = false;

    private readonly List<AudioSource> audioSources = new List<AudioSource>();
    private readonly Dictionary<string, AudioClip[]> soundKeyDics = new Dictionary<string, AudioClip[]>(StringComparer.Ordinal);
    private readonly Dictionary<string, float> lastPlayKeyTime = new Dictionary<string, float>(StringComparer.Ordinal);
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

    public void PlayOnShot(string key, float volume = 1f)
    {
        EnsureInitialized();
        if (string.IsNullOrWhiteSpace(key) || !soundKeyDics.TryGetValue(key, out var clips) || clips.Length == 0) return;

        float now = Time.unscaledTime;
        if (lastPlayKeyTime.TryGetValue(key, out float last) && now - last < minIntervalSameSound)
            return;

        lastPlayKeyTime[key] = now;
        PlayClipDirect(clips[UnityEngine.Random.Range(0, clips.Length)], volume);
    }

    public void PlayOnShot(AudioClip clip, float volume = 1f)
    {
        PlayClipDirect(clip, volume);
    }

    public void PlaySpecialSound(string key, float volume = 1.0f)
    {
        EnsureInitialized();
        if (string.IsNullOrWhiteSpace(key) || !soundKeyDics.TryGetValue(key, out var clips) || clips.Length == 0) return;

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

    public AudioClip GetRandomClip(string key)
    {
        EnsureInitialized();

        if (!string.IsNullOrWhiteSpace(key) && soundKeyDics.TryGetValue(key, out var clips) && clips.Length > 0)
            return clips[UnityEngine.Random.Range(0, clips.Length)];

        return null;
    }

    public AudioClip ConvertToClip(string key) => GetRandomClip(key);

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

        soundKeyDics.Clear();
        if (sounds != null)
        {
            foreach (var s in sounds)
            {
                string key = s.key;
                if (s.clips != null && s.clips.Length > 0 && !string.IsNullOrWhiteSpace(key) && !soundKeyDics.ContainsKey(key))
                    soundKeyDics.Add(key, s.clips);
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

        Debug.Log($"[SoundService] Initialized with {count} AudioSources");
    }
}
