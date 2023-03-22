using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PuzzleGrid/Grid", fileName = "Grid")]
public class PuzzleGrid : ScriptableObject
{
    /// <summary>
    /// Date: 03/22/23, By: Yvar
    /// Okay, this is what I have in mind: A 2d grid that you can extend using arrows. The most outer layer will always have arrows pointing outwards.
    /// The outer layer consists of "NONE tiles". They do not have a visible tile on them, no objects should be placed on them.
    /// When you press such arrow, that tile will extend, creating a visible tile on the grid where objects can be placed down.
    /// Around the newly created tile, a detection system will determine if there should grow a new NONE tile.
    /// This class also tracks the total grid size, which should be able to grow.
    /// </summary>

    [HideInInspector] public List<Vector2Int> gridRegister;
    [SerializeField] int totalWidth;
    [SerializeField] int totalLength;
    [SerializeField] GameObject Prefab_PLACEABLE_TILE;
    [SerializeField] GameObject Prefab_EDGE_TILE;
    TileInformation[,] tilesInformation;

    /*
    public PuzzleGrid(int startWidth, int StartLength)
    {
        totalWidth = startWidth;
        totalLength = StartLength;

        // First we setup the first grid
        gridRegister = new List<Vector2Int>();
        tilesInformation = new TileInformation[totalWidth, totalLength];
        for (int width = 0; width < startWidth; width++)
        {
            for (int length = 0; length < StartLength; length++)
            {
                RegisterNewTile(new Vector2Int(width, length));
                if (width == 0 || length == 0 || width == startWidth - 1 || length == StartLength - 1)
                {
                    tilesInformation[width, length].myType = TileType.NONE_TILE;
                }
                else if (width == 1 || length == 1 || width == startWidth - 2 || length == StartLength - 2)
                {
                    tilesInformation[width, length].myType = TileType.EDGE_TILE;
                    // add arrow here?
                }
                else
                {
                    tilesInformation[width, length].myType = TileType.PLACEABLE_TILE;
                    // add tile here?
                }
                tilesInformation[width, length].myLocation = new Vector2Int(width, length);
            }
        }
    }
    */

    public void Initialize()
    {
        // First we setup the first grid
        gridRegister = new List<Vector2Int>();
        tilesInformation = new TileInformation[totalWidth, totalLength];
        for (int width = 0; width < totalWidth; width++)
        {
            for (int length = 0; length < totalLength; length++)
            {
                RegisterNewTile(new Vector2Int(width, length));
                if (width == 0 || length == 0 || width == totalWidth - 1 || length == totalLength - 1)
                {
                    tilesInformation[width, length].myType = TileType.NONE_TILE;
                }
                else if (width == 1 || length == 1 || width == totalWidth - 2 || length == totalLength - 2)
                {
                    tilesInformation[width, length].myType = TileType.EDGE_TILE;
                    Instantiate(Prefab_EDGE_TILE, new Vector3(width, 0, length), Quaternion.identity);
                }
                else
                {
                    tilesInformation[width, length].myType = TileType.PLACEABLE_TILE;
                    Instantiate(Prefab_PLACEABLE_TILE, new Vector3(width, 0, length), Quaternion.identity);
                }
                tilesInformation[width, length].myLocation = new Vector2Int(width, length);
            }
        }
    }

    void RegisterNewTile(Vector2Int newTile)
    {
        gridRegister.Add(newTile);
    }

    void RemoveTile(Vector2Int removeTile)
    {
        // should be casted to some sort of undo, but for now keep it simple
        if (gridRegister.Contains(removeTile))
            gridRegister.Remove(removeTile);
    }

    /// <summary>
    /// Algoritm to update the grids tile types. To determine the tiletype, there should be a few conditions:
    /// 1. The most outer tiles are ALWAYS NONE tiles.
    /// 2. 
    /// </summary>
    void UpdateGrid()
    {

    }

    public TileType GetTileType(Vector2Int location)
    {
        return tilesInformation[location.x, location.y].myType;
    }
}

public struct TileInformation
{
    public TileType myType;
    public Vector2Int myLocation;
}

public enum TileType { NONE_TILE = 0, EDGE_TILE = 1 , PLACEABLE_TILE = 2 }
