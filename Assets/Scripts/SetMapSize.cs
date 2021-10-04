using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SetMapSize : MonoBehaviour
{
    [SerializeField] private MapSettings mapSettings;

    private TMP_Text _text;
    private Slider _slider;

    private void Start()
    {
        _text = GetComponent<TMP_Text>();
        _slider = GetComponent<Slider>();
        if (_slider != null)
        {
            _slider.value = mapSettings.radius;
        }
    }

    public void OnChange(float val)
    {
        mapSettings.radius = (int) Math.Round(val);
    }

    public void Update()
    {
        if (_text != null)
        {
            _text.text = $"Map size ({mapSettings.radius}):";
        }
    }
}