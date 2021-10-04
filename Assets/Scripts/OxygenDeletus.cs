using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class OxygenDeletus : MonoBehaviour
{

     [SerializeField] private float deSpawnRate = .5f;
    private static Unity.Mathematics.Random _random = new Unity.Mathematics.Random( (uint) System.DateTime.Now.Millisecond);
    // Start is called before the first frame update
    public void Start()
    {
        if(_random.NextFloat() < deSpawnRate)
            DestroyImmediate(gameObject);
    }

}
