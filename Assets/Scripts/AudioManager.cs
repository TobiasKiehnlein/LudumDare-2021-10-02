using System;
using System.Linq;
using Enums;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private GameSettings gameSettings;

    private float _localSfxVolume = 1;
    private float _localMusicVolume = 1;


    public static AudioManager Instance { get; private set; }

    private AudioSource[] _musicSources;
    private AudioSource[] _sfxSources;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        _musicSources = GetComponents<AudioSource>().Where(x => x.loop).ToArray();
        _sfxSources = GetComponents<AudioSource>().Where(x => !x.loop).ToArray();
        DontDestroyOnLoad(this.gameObject);
    }

    public void SetMusicMute(bool muted)
    {
        foreach (var musicSource in _musicSources)
        {
            musicSource.mute = muted;
        }
    }

    public void SetSfxMute(bool muted)
    {
        foreach (var source in _sfxSources)
        {
            source.mute = muted;
        }
    }

    public void StartSound(Sfx sfx)
    {
        try
        {
            SfxHandler.Instance.TriggerSfx(sfx);
        }
        catch (Exception)
        {
            Debug.LogWarning($"The audio No. ${sfx} couldn't be played");
        }
    }

    public void StartSound(Music music, float time = 5)
    {
        mixer.FindSnapshot(music.ToString()).TransitionTo(time);
    }

    public void StopSound(Sfx sfx)
    {
    }

    private void Update()
    {
        const double tolerance = .01;
        if (Math.Abs(gameSettings.MusicVolume - _localMusicVolume) > tolerance)
        {
            mixer.SetFloat("MusicVol", gameSettings.MusicVolume * 80 - 80);
            _localMusicVolume = gameSettings.MusicVolume;
        }

        if (Math.Abs(gameSettings.SfxVolume - _localSfxVolume) > tolerance)
        {
            mixer.SetFloat("SfxVol", gameSettings.SfxVolume * 80 - 80);
            _localSfxVolume = gameSettings.SfxVolume;
        }
    }
}