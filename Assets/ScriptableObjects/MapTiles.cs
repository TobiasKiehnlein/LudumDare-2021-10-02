using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "MapTiles", menuName = "CustomGameSettings/MapTiles", order = 0)]
    public class MapTiles : ScriptableObject
    {
        public Tile[] tiles;
    }
}