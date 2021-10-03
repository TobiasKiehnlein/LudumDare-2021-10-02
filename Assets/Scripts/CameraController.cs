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
    private SpaceManAnimator _spaceManAnimator;

    private void Start()
    {
        _spaceManAnimator = player.GetComponentInChildren<SpaceManAnimator>();
    }

    void LateUpdate()
    {
        if (_spaceManAnimator.GetCurrentState() != SpaceManAnimator.AnimatorState.FreeFalling) //Todo replace with something like "isFreeFall"
        {
            var moveInterval = _mapSettings.tileSize / 2;
            var dest = new Vector3 {x = (float) Math.Round(player.position.x / moveInterval) * moveInterval, y = (float) Math.Round(player.position.y / (moveInterval / 2)) * (moveInterval / 2), z = transform.position.z};
            transform.position = Vector3.Lerp(transform.position, dest, speed * Time.deltaTime * 15);
        }
        else
        {
            var pos = transform.position;
            var playerPos = player.position;
            var dest = new Vector3 {x = playerPos.x, y = playerPos.y, z = pos.z};
            var lerpSpeed = speed * Time.deltaTime * (float) Math.Pow(2, Vector3.Distance(pos, dest));
            transform.position = Vector3.Lerp(pos, dest, lerpSpeed);
        }
    }
}