using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class SceneObject : AbstractGameEditor
{
    // static list of all objects visible in the scene
    public static List<SceneObject> sceneObjects = new List<SceneObject>();
    MeshRenderer meshRenderer;
    Collider anyCollider;

    protected override void OnEnable()
    {
        base.OnEnable();
        anyCollider = GetComponent<Collider>();
        anyCollider.enabled = false;
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void OnCreation()
    {
        anyCollider.enabled = true;
        meshRenderer.enabled = true;
        sceneObjects.Add(this);
    }

    public void OnDeletion()
    {
        anyCollider.enabled = false;
        meshRenderer.enabled = false;
        sceneObjects.Remove(this);
    }

    public void OnInvisibe()
    {
        anyCollider.enabled = false;
        meshRenderer.enabled = false;
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

    public override void EditorAwake()
    {
    }

    public override void EditorStart()
    {
    }

    public override void EditorUpdate()
    {
    }
}
