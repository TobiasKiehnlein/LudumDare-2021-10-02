using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "MapSettings", menuName = "CustomGameSettings/MapSettings", order = 0)]
    public class MapSettings : ScriptableObject
    {
        // amount of tiles that will be placed next to each other
        [Range(2,25)]
        public int radius = 10;
        public float tileSize = 45;
    }
}