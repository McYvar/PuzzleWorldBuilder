using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class TerrainObject : SceneObject
{
    public static List<TerrainObject> terrainObjects = new List<TerrainObject>();

    public override void OnCreation()
    {
        meshRenderer.material.SetInt("_UseShader", 0);
        anyCollider.enabled = true;
        meshRenderer.enabled = true;
        terrainObjects.Add(this);
    }

    public override void OnDeletion()
    {
        anyCollider.enabled = false;
        meshRenderer.enabled = false;
        terrainObjects.Remove(this);
    }

    public override void OnSelection()
    {
        base.OnSelection();
        // outline material has to be the first assigned material to objects, maybe better solution for this later
        meshRenderer.material.SetInt("_UseShader", 1);
    }

    public override void OnDeselection()
    {
        base.OnDeselection();
        meshRenderer.material.SetInt("_UseShader", 0);
    }

    public override void MoveTo(Vector3 newPos)
    {
        transform.position = myStartPos + newPos;
    }
}
