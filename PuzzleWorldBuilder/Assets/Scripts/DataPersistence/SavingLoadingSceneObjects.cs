using System.Collections.Generic;
using UnityEngine;

public class SavingLoadingSceneObjects : MonoBehaviour, IDataPersistence
{
    [SerializeField] InputCommands inputCommands;
    [SerializeField] PuzzleStageEditor puzzleStageEditor;

    [SerializeField] Transform parent;
    [SerializeField] GameObject[] usedPrefabs;

    [SerializeField] GameObject player;

    public void NewData()
    {
        inputCommands.ResetTool();
        ClearObjects();
        puzzleStageEditor.InitializeNewGrid(30, 30, 0, 0);
        player.transform.position = new Vector3(15, 3, 15);
    }

    void ClearObjects()
    {
        if (TerrainObject.terrainObjects.Count > 0)
        {
            foreach (TerrainObject terrainObject in TerrainObject.terrainObjects)
            {
                Destroy(terrainObject.gameObject);
            }
            TerrainObject.terrainObjects.Clear();
        }
        if (GridObject.gridObjects.Count > 0)
        {
            foreach (GridObject gridObject in GridObject.gridObjects)
            {
                Destroy(gridObject.gameObject);
            }
            GridObject.gridObjects.Clear();
        }
    }

    public void LoadData(GameData data)
    {
        if (data == null) return;
        inputCommands.ResetTool();
        ClearObjects();

        if (data.terrainObjectData.Count > 0)
        {
            foreach (TerrainObjectData terrainObjectData in data.terrainObjectData)
            {
                bool found = false;
                foreach (GameObject go in usedPrefabs)
                {
                    if (go.name == terrainObjectData.name)
                    {
                        GameObject loadedObj = Instantiate(go, parent);
                        loadedObj.transform.position = terrainObjectData.position;
                        loadedObj.transform.rotation = terrainObjectData.rotation;
                        loadedObj.name = go.name;
                        TerrainObject terrainObject = loadedObj.GetComponent<TerrainObject>();
                        terrainObject.OnCreation();
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    Debug.LogWarning("No prefab with the name '" + terrainObjectData.name + "' exists");
                }
            }
        }

        // loading grid
        if (data.gridObjectData.Count > 0)
        {
            int maxX = 0;
            int minX = 0;
            int maxZ = 0;
            int minZ = 0;
            List<Vector3> positions = new List<Vector3>();
            List<TileType> tileTypes = new List<TileType>();
            foreach (GridObjectData gridObjectData in data.gridObjectData)
            {
                Vector3 cur = gridObjectData.position;
                positions.Add(cur);
                tileTypes.Add(gridObjectData.tileType);
                if (cur.x > maxX) maxX = (int)cur.x;
                if (cur.z > maxZ) maxZ = (int)cur.z;
                if (cur.x < minX) minX = (int)cur.x;
                if (cur.z < minZ) minZ = (int)cur.z;
            }
            puzzleStageEditor.InitializeNewGrid(maxX + 1, maxZ + 1, minX, minZ, positions.ToArray(), tileTypes.ToArray());
        }

        // loading player
        player.transform.position = data.playerPosition;
    }

    public void SaveData(ref GameData data)
    {
        // saving terrain objects
        data.terrainObjectData.Clear();
        if (TerrainObject.terrainObjects.Count > 0)
        {
            foreach (TerrainObject terrainObject in TerrainObject.terrainObjects)
            {
                terrainObject.SaveObject();
                data.terrainObjectData.Add(terrainObject.myData);
            }
        }

        // saving grid
        data.gridObjectData.Clear();
        if (GridObject.gridObjects.Count > 0)
        {
            foreach (GridObject gridObject in GridObject.gridObjects)
            {
                gridObject.SaveObject();
                data.gridObjectData.Add(gridObject.myData);
            }
        }

        PlayerObject playerObject = player.GetComponent<PlayerObject>();
        playerObject.SaveObject();
        data.playerPosition = playerObject.myData.position;
    }
}
