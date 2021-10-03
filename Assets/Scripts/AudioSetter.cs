using System;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

public class AudioSetter : MonoBehaviour
{
    private Slider _slider;

    private void Start()
    {
        _slider = GetComponent<Slider>();
    }

    enum Type
    {
        Music,
        Sfx
    }

    [SerializeField] private Type type;
    [SerializeField] private GameSettings gameSettings;

    public void OnValueChanged(float value)
    {
        if (type == Type.Music)
        {
            gameSettings.MusicVolume = value;
        }
        else if (type == Type.Sfx)
        {
            gameSettings.SfxVolume = value;
        }
    }

    private void Update()
    {
        if (_slider != null)
        {
            if (type == Type.Music)
            {
                _slider.value = gameSettings.MusicVolume;
            }
            else if (type == Type.Sfx)
            {
                _slider.value = gameSettings.SfxVolume;
            }
        }
    }
}