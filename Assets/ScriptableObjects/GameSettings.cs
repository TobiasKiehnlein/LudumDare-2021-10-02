using System;
using Enums;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "CustomGameSettings/GameSettings", order = 0)]
    public class GameSettings : ScriptableObject
    {
        [SerializeField] private Orientation gravityOrientation;
        [SerializeField] private float gravityStrength = 9.81f;
        public float gravityChangeInterval = 20;
        public float gravityChangeRandomness = 5;
        
        public Orientation GravityOrientation
        {
            get => gravityOrientation;
            set
            {
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
    }
}