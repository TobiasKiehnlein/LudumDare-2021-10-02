
using ScriptableObjects;
using UnityEngine;

public class MinimapCameraController : MonoBehaviour
{

    [SerializeField] private MapSettings settings;
    
    void Start()
    {
        GetComponent<Camera>().orthographicSize = settings.radius * settings.tileSize;
    }
}
