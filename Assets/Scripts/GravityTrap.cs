using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;


public class GravityTrap : MonoBehaviour
{
   
   [SerializeField] private GameSettings gameSettings;
 [SerializeField] private int warpCountPerTrigger;
   [SerializeField] private float sleepDuration;
   [SerializeField] private float timeBetweenWarps;
   private bool _active;
   private int _remainingWarps;
   private float _timeToNextWarp;
   private float _sleepTimer = 0f;
   private readonly Random _random = new Random(); 
   private void Update()
   {
      _sleepTimer -= Time.deltaTime;
      if (_sleepTimer < 0)
         _sleepTimer = 0;

      if (_active)
      {
         if (_timeToNextWarp > Mathf.Epsilon)
         {
            _timeToNextWarp -= Time.deltaTime;

         }else

         {
            if (_remainingWarps == 0)
            {
               _active = false;
            }
            else
            {
               _remainingWarps--;
               gameSettings.GravityOrientation =
                  (Orientation) (((int) gameSettings.GravityOrientation + _random.Next(1, 3)) % 4);
               _timeToNextWarp = timeBetweenWarps + 2 * (float) _random.NextDouble();

            }
         }




      }
   }

   private void OnTriggerEnter2D(Collider2D other)
   {
      
      Debug.Log(other);
      Debug.Log(other.tag);
      if (_sleepTimer > Mathf.Epsilon) return;
      if (!other.CompareTag("Player")) return;
      _active = true;
      _sleepTimer = sleepDuration;
      _remainingWarps = warpCountPerTrigger;
   }
}
