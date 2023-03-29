using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class TerrainObject : SceneObject
{
    public static List<TerrainObject> terrainObjects = new List<TerrainObject>();
    public TerrainObjectData myData;

    protected override void OnEnable()
    {
        base.OnEnable();
        myData = new TerrainObjectData();
    }

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
        myData.position = transform.position;
    }

    public void Initialize(string name)
    {
        myData.position = transform.position;
        myData.name = name;
    }
}

[System.Serializable]
public class TerrainObjectData : SceneObjectData
{
    /// <summary>
    /// Date 03/29/23, By: Yvar Toorop
    /// Since saving and loading is a fairly new concept to me... for this class I will save the name of the
    /// terrain object. Upon loading this data object will go trough a set of strings to check what prefab
    /// belongs to it. Then it will be spawned and placed on the right position.
    /// </summary>
    public string name;
    public Quaternion rotation;

    public TerrainObjectData()
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;
    }
}
