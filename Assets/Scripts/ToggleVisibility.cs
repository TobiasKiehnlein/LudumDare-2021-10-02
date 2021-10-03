using System;
using UnityEngine;

public class ToggleVisibility : MonoBehaviour
{
    [SerializeField] private KeyCode keyCode;
    private Canvas _canvas;

    private void Start()
    {
        _canvas = GetComponent<Canvas>();
    }

    void Update()
    {
        if (Input.GetKeyDown(keyCode))
        {
            _canvas.enabled = !_canvas.enabled;
        }
    }
}
