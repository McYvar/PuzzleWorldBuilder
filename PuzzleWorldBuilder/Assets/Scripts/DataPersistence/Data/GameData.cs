using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public List<TerrainObjectData> terrainObjectData;
    public List<GridObjectData> gridObjectData;
    public Vector3 playerPosition;

    public GameData()
    {
        terrainObjectData = new List<TerrainObjectData>();
        gridObjectData = new List<GridObjectData>();
        playerPosition = Vector3.zero;
    }

}
