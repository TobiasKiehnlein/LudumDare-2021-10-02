using System.Linq;
using Enums;
using ScriptableObjects;
using UnityEngine;
using Random = System.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameSettings gameSettings;

    private float _nextChange = float.MaxValue;
    private Random _random;

    // Start is called before the first frame update
    void Start()
    {
        gameSettings.GravityOrientation = Orientation.Down;
        _nextChange = gameSettings.gravityChangeInterval;
        _random = new Random();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > _nextChange)
        {
            TriggerGravityChange();
            _nextChange += gameSettings.gravityChangeInterval + (float) _random.NextDouble() * gameSettings.gravityChangeRandomness * 2 - gameSettings.gravityChangeRandomness;
        }
    }

    private void TriggerGravityChange()
    {
        gameSettings.GravityOrientation = (Orientation) (((int) gameSettings.GravityOrientation + _random.Next(1, 3)) % 4);
    }
}