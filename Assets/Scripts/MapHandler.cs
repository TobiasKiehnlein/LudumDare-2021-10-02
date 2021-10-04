using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using ScriptableObjects;
using UnityEngine;
using Utils;
using Random = System.Random;

public class MapHandler : MonoBehaviour
{
    [SerializeField] private MapTiles mapTiles;
    [SerializeField] private MapSettings mapSettings;
    private string[,] _map;
    private Rot[,] _rotations;
    private Dictionary<string, Tile> _tiles;
    private bool alreadyGeneratedSpaceStation;

    private void Start()
    {
        _tiles = mapTiles.tiles.ToDictionary(tile => tile.TileName, tile => tile);
        _map = new string[mapSettings.radius * 2 - 1, mapSettings.radius * 2 - 1];
        _rotations = new Rot[mapSettings.radius * 2 - 1, mapSettings.radius * 2 - 1];
        var maze = new Maze(mapSettings.radius);
        maze.Display();
        var rand = new Random();
        for (var i = 0; i < mapSettings.radius * 2 - 1; i++)
        for (var j = 0; j < mapSettings.radius * 2 - 1; j++)
        {
            var distanceToCenter = Vector2.Distance(new Vector2 {x = j, y = i},
                new Vector2 {x = mapSettings.radius - 1, y = mapSettings.radius - 1});
            if (!(distanceToCenter < mapSettings.radius)) continue;
            try
            {
                _map[i, j] = _tiles.Keys.ToArray()[rand.Next(0, _tiles.Count)];
                var (tileType, rotation) = maze.GetTileInfo(i, j);
                var tile = mapTiles.tiles.Where(x => x.Type == tileType).Shuffle(rand).First();

                if (tileType == TileType.DeadEnd && !alreadyGeneratedSpaceStation)
                {
                    alreadyGeneratedSpaceStation = true;
                    tile = mapTiles.spaceStation;
                }

                var go = Instantiate(tile.Prefab, transform);
                go.transform.localPosition = new Vector3 {x = i * mapSettings.tileSize, y = j * mapSettings.tileSize};
                go.transform.Rotate(new Vector3(0,0,1), (int) rotation);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        transform.Translate(-(mapSettings.radius - 1) * mapSettings.tileSize, -(mapSettings.radius - 1) * mapSettings.tileSize, 0);
    }
}

[Serializable]
public class Tile
{
    // Used as unique identifier in map generation and storing
    public string TileName;
    public TileType Type;
    public GameObject Prefab;
    public bool CanFaceUp;
    public bool CanFaceRight;
    public bool CanFaceDown;
    public bool CanFaceLeft;
}