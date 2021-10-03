using ScriptableObjects;
using UnityEngine;

public class AudioSetter : MonoBehaviour
{
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
            gameSettings.musicVolume = value;
        } else if (type == Type.Sfx)
        {
            gameSettings.sfxVolume = value;
        }
    }
}
