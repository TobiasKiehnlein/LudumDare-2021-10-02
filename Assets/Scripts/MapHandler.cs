using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using Enums;
using JetBrains.Annotations;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;
using Random = System.Random;

public class MapHandler : MonoBehaviour
{
    [SerializeField] private MapTiles mapTiles;
    [SerializeField] private MapSettings mapSettings;
    private Dictionary<string, Tile> _tiles;
    private string[,] _map;
    private Rot[,] _rotations;

    void Start()
    {
        _tiles = mapTiles.tiles.ToDictionary(tile => tile.TileName, tile => tile);
        _map = new string[mapSettings.radius * 2 - 1, mapSettings.radius * 2 - 1];
        _rotations = new Rot[mapSettings.radius * 2 - 1, mapSettings.radius * 2 - 1];
        var maze = new Maze(mapSettings.radius);
        maze.Display();
        var rand = new Random();
        for (var i = 0; i < mapSettings.radius * 2 - 1; i++)
        {
            for (var j = 0; j < mapSettings.radius * 2 - 1; j++)
            {
                var distanceToCenter = Vector2.Distance(new Vector2 {x = j, y = i}, new Vector2 {x = mapSettings.radius - 1, y = mapSettings.radius - 1});
                if (!(distanceToCenter < mapSettings.radius)) continue;

                _map[i, j] = _tiles.Keys.ToArray()[rand.Next(0, _tiles.Count)];
                var currentTileInfo = maze.GetTileInfo(i, j);
                Tile tile;
                try
                {
                    tile = mapTiles.tiles.First(x => x.Type == currentTileInfo.Item1);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                var go = Instantiate(tile.Prefab, transform);
                go.transform.localPosition = new Vector3 {x = i * mapSettings.tileSize, y = j * mapSettings.tileSize};
                go.transform.Rotate(Vector3.left, -90);
                go.transform.Rotate(Vector3.up, (int) currentTileInfo.Item2);
            }
        }

        transform.Translate(-(mapSettings.radius - 1) * mapSettings.tileSize, -(mapSettings.radius - 1) * mapSettings.tileSize, 0);


        Utils.Utils.Print2DArray(_map);
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