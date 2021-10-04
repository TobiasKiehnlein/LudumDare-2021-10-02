using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatePermanently : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Vector3 angle = Vector3.up;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(angle, Time.deltaTime * speed);
    }
}