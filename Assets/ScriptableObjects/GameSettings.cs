using System;
using Enums;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "CustomGameSettings/GameSettings", order = 0)]
    public class GameSettings : ScriptableObject
    {
        [SerializeField] private Orientation gravityOrientation;
        [SerializeField] private float gravityStrength = 9.81f;
        public float gravityChangeInterval = 20;
        public float gravityChangeRandomness = 5;
        public float oxygenMax = 100f;
        public float oxygenCurrent = 0f;
        public float oxygenStart = 70f;
        public float oxygenTank = 25f;
        [SerializeField] private float musicVolume = 1;
        [SerializeField] private float sfxVolume = 1;

        public float MusicVolume
        {
            get => musicVolume;
            set
            {
                musicVolume = value;
                PlayerPrefs.SetFloat("MusicVolume", value);
                PlayerPrefs.Save();
            }
        }

        public float SfxVolume
        {
            get => sfxVolume;
            set
            {
                sfxVolume = value;
                PlayerPrefs.SetFloat("SfxVolume", value);
                PlayerPrefs.Save();
            }
        }


        public Orientation GravityOrientation
        {
            get => gravityOrientation;

            set
            {
                return;
                gravityOrientation = value;
                Physics2D.gravity = value switch
                {
                    Orientation.Down => new Vector2(0, -gravityStrength),
                    Orientation.Up => new Vector2(0, gravityStrength),
                    Orientation.Left => new Vector2(-gravityStrength, 0),
                    Orientation.Right => new Vector2(gravityStrength, 0),
                    _ => new Vector2(0, -gravityStrength)
                };
            }
        }

        private void OnEnable()
        {
            gravityOrientation = Orientation.Down;
            musicVolume = PlayerPrefs.HasKey("MusicVolume") ? PlayerPrefs.GetFloat("MusicVolume") : 1;
            sfxVolume = PlayerPrefs.HasKey("SfxVolume") ? PlayerPrefs.GetFloat("SfxVolume") : 1;
        }
    }
}