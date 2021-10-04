using System;
using UnityEngine;
using Utils;

public class ToggleVisibility : MonoBehaviour
{
    [SerializeField] private KeyCode keyCode;
    [SerializeField] private bool defaultValue = true;
    [SerializeField] private bool pauseGameWhileVisible = false;
    private Canvas _canvas;

    private void Start()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.enabled = defaultValue;
    }

    void Update()
    {
        if (Input.GetKeyDown(keyCode))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        _canvas.enabled = !_canvas.enabled;
        if (pauseGameWhileVisible)
        {
            Time.timeScale = 1 - _canvas.enabled.ToInt();
        }
    }
}
