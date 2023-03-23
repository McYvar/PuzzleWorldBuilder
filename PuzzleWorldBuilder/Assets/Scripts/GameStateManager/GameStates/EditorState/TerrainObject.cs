using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class TerrainObject : SceneObject
{
    public override void OnCreation()
    {
        anyCollider.enabled = true;
        meshRenderer.enabled = true;
        sceneObjects.Add(this);
    }

    public override void OnDeletion()
    {
        anyCollider.enabled = false;
        meshRenderer.enabled = false;
        sceneObjects.Remove(this);
    }
}
