using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : SingletonGlobal<MusicManager>
{
    public enum Music
    {
        NONE,
        // Thêm các loại nhạc khác ở đây
    }

    [System.Serializable]
    public class MusicTable
    {
        public Music music;
        public AudioClip clip;
    }

    [SerializeField] private MusicTable[] musics;
    [SerializeField] private AudioSource audioSource;
    private Dictionary<Music, AudioClip> musicDics = new();

    protected override void Awake()
    {
        base.Awake();
        musicDics.Clear();
        foreach (var m in musics)
            if (m.clip && !musicDics.ContainsKey(m.music))
                musicDics.Add(m.music, m.clip);
    }

    public void PlayMusic(Music music, float volume = 1.0f, bool loop = true)
    {
        if (!musicDics.TryGetValue(music, out var clip) || clip == null)
            return;

        if (audioSource.isPlaying && audioSource.clip == clip)
            return; // Đã phát đúng nhạc này rồi

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.volume = volume;
        audioSource.Play();
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }

    public void PauseMusic()
    {
        audioSource.Pause();
    }

    public void ResumeMusic()
    {
        audioSource.UnPause();
    }

    public void SetMute(bool isMute)
    {
        audioSource.mute = isMute;
    }

    public void SetVolume(float toVolume, float duration = 1f)
    {
        DOTween.To(() => audioSource.volume, x => audioSource.volume = x, toVolume, duration);
    }

    public bool IsPlaying => audioSource.isPlaying;
    public float CurrentVolume => audioSource.volume;
    public AudioClip CurrentClip => audioSource.clip;
}
