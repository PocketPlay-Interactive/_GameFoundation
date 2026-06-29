using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SoundManager : SingletonGlobal<SoundManager>
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
    [SerializeField] private int audioSourceCount = 20; // Tăng lên 20 AudioSource để xử lý súng bắn nhanh
    
    private List<AudioSource> audioSources = new List<AudioSource>();
    private int currentAudioSourceIndex = 0;
    
    private Dictionary<Sound, AudioClip[]> soundDics = new();
    private Dictionary<Sound, float> lastPlayTime = new();
    [SerializeField] private float minIntervalSameSound = 0.05f;

    protected override void Awake()
    {
        base.Awake();
        
        // Tăng số lượng virtual voices lên tối đa
        var config = AudioSettings.GetConfiguration();
        config.numVirtualVoices = 512;
        config.numRealVoices = 32; // Số voices thật sự được phát (hardware limit)
        AudioSettings.Reset(config);
        
        foreach (var s in sounds)
        {
            if (s.clips != null && s.clips.Length > 0)
            {
                if (!soundDics.ContainsKey(s.sound))
                    soundDics.Add(s.sound, s.clips);
            }
        }
        
        // Tạo nhiều AudioSource để phân tải PlayOneShot
        for (int i = 0; i < audioSourceCount; i++)
        {
            GameObject go = new GameObject($"AudioSource_{i}");
            go.transform.SetParent(transform);
            AudioSource src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.priority = 128; // Priority thấp nhất để không bị giới hạn
            audioSources.Add(src);
        }
        
        Debug.Log($"[SoundManager] Initialized with {audioSourceCount} AudioSources");
    }

    private void Start()
    {
        DOVirtual.DelayedCall(0.5f, () => SetMute());
    }

    public void SetMute()
    {
        bool mute = !RuntimeStorageData.Setting.Get("sound", true);
        foreach (var src in audioSources)
            if (src) src.mute = mute;
        if (audioSourceSpecial) audioSourceSpecial.mute = mute;
    }

    public void VolumeChange(float toVolume)
    {
        foreach (var src in audioSources)
        {
            if (src)
                DOTween.To(() => src.volume, x => src.volume = x, toVolume, 1f);
        }
        if (audioSourceSpecial)
            DOTween.To(() => audioSourceSpecial.volume, x => audioSourceSpecial.volume = x, toVolume, 1f);
    }

    public void PlayOnShot(Sound sound, float volume = 1f)
    {
        if (!soundDics.TryGetValue(sound, out var clips) || clips.Length == 0) return;

        // Giảm spam cho UI sounds
        float now = Time.unscaledTime;
        if (lastPlayTime.TryGetValue(sound, out float last) && now - last < minIntervalSameSound)
            return;
        lastPlayTime[sound] = now;

        var clip = clips[UnityEngine.Random.Range(0, clips.Length)];
        PlayClipDirect(clip, volume);
    }

    public void PlayOnShot(AudioClip clip, float volume = 1f)
    {
        PlayClipDirect(clip, volume);
    }

    public void PlaySpecialSound(Sound sound, float volume = 1.0f)
    {
        if (!soundDics.TryGetValue(sound, out var clips) || clips.Length == 0) return;
        var clip = clips[UnityEngine.Random.Range(0, clips.Length)];
        if (audioSourceSpecial && clip)
        {
            audioSourceSpecial.volume = volume;
            audioSourceSpecial.clip = clip;
            audioSourceSpecial.Play();
        }
    }

    public void StopSpecialSound()
    {
        if (audioSourceSpecial) audioSourceSpecial.Stop();
    }

    public AudioClip GetRandomClip(Sound sound)
    {
        if (soundDics.TryGetValue(sound, out var clips) && clips.Length > 0)
            return clips[UnityEngine.Random.Range(0, clips.Length)];
        return null;
    }

    public AudioClip ConvertToClip(Sound sound) => GetRandomClip(sound);

    // Thay vì dùng PlayOneShot (tạo voice mới mỗi lần)
    // Dùng Play() với AudioSource riêng biệt (1 AudioSource = 1 voice)
    public void PlayClipDirect(AudioClip clip, float volume = 1f)
    {
        if (clip == null || audioSources.Count == 0) return;
        
        // Tìm AudioSource đang KHÔNG phát (available)
        AudioSource availableSrc = null;
        foreach (var src in audioSources)
        {
            if (!src.isPlaying)
            {
                availableSrc = src;
                break;
            }
        }
        
        // Nếu không có AudioSource rảnh, dùng round-robin (ghi đè cũ)
        if (availableSrc == null)
        {
            availableSrc = audioSources[currentAudioSourceIndex];
            currentAudioSourceIndex = (currentAudioSourceIndex + 1) % audioSources.Count;
        }
        
        if (availableSrc != null)
        {
            // Dùng Play() thay vì PlayOneShot()
            // Play() chỉ tạo 1 voice cho mỗi AudioSource
            availableSrc.clip = clip;
            availableSrc.volume = volume;
            availableSrc.Play();
        }
    }
}