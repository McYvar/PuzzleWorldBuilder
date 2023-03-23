using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class TerrainObject : SceneObject
{
    public static List<TerrainObject> terrainObject = new List<TerrainObject>();
    public override void OnCreation()
    {
        meshRenderer.material.SetInt("_UseShader", 0);
        anyCollider.enabled = true;
        meshRenderer.enabled = true;
        terrainObject.Add(this);
    }

    public override void OnDeletion()
    {
        anyCollider.enabled = false;
        meshRenderer.enabled = false;
        terrainObject.Remove(this);
    }

    public override void OnSelection()
    {
        // outline material has to be the first assigned material to objects, maybe better solution for this later
        meshRenderer.material.SetInt("_UseShader", 1);
        InputCommands.selectedTerrainObjects.Add(this);
    }

    public override void OnDeselection()
    {
        meshRenderer.material.SetInt("_UseShader", 0);
        InputCommands.selectedTerrainObjects.Remove(this);
    }
}
