using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayOxygen : MonoBehaviour
{
    [SerializeField] private GameSettings _gameSettings;
    private Slider _slider;
    // Start is called before the first frame update
    void Start()
    {
        _slider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_slider != null)
        {
            _slider.value = _gameSettings.oxygenCurrent / _gameSettings.oxygenMax;
        }
    }
}
