using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float speed = 10;
    [SerializeField] private MapSettings _mapSettings;

    public bool MoveToPlayer => _moveToPlayer;

    private bool _moveToPlayer;

    // Update is called once per frame
    void Update()
    {
        var moveInterval = _mapSettings.tileSize / 2;
        var dest = new Vector3 {x = (float)Math.Round(player.position.x/ moveInterval)*moveInterval, y =(float)Math.Round(player.position.y/ (moveInterval / 2))* (moveInterval / 2), z = transform.position.z};
        transform.position = Vector3.Lerp(transform.position, dest, speed * Time.deltaTime);
    }
}