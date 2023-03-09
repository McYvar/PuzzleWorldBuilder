using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class SceneObject : AbstractGameEditor
{
    public static List<SceneObject> sceneObjects = new List<SceneObject>();
    MeshRenderer meshRenderer;

    protected override void OnEnable()
    {
        base.OnEnable();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void OnCreation()
    {
        meshRenderer.enabled = true;
        sceneObjects.Add(this);
    }

    public void OnDeletion()
    {
        meshRenderer.enabled = false;
        sceneObjects.Remove(this);
    }

    public void OnSelection()
    {
        // outline material has to be the first assigned material to objects, maybe better solution for this later
        meshRenderer.material.SetInt("_UseShader", 1);
    }

    public void OnDeselection()
    {
        meshRenderer.material.SetInt("_UseShader", 0);
    }

    public bool IsAlive()
    {
        return meshRenderer.enabled;
    }

    public override void EditorUpdate()
    {
    }
}
