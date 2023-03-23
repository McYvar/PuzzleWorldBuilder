﻿using System.Collections.Generic;
using UnityEngine;

public class SceneObject : AbstractGameEditor
{
    // static list of all objects visible in the scene
    public static List<SceneObject> sceneObjects = new List<SceneObject>();
    [HideInInspector] public Vector3 myStartPos;
    protected MeshRenderer meshRenderer;
    protected Collider anyCollider;

    protected override void OnEnable()
    {
        base.OnEnable();
        anyCollider = GetComponent<Collider>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public virtual void OnCreation()
    {
    }

    public virtual void OnDeletion()
    {
    }

    public void OnInvisibe()
    {
        anyCollider.enabled = false;
        meshRenderer.enabled = false;
    }

    public virtual void OnSelection()
    {
        // outline material has to be the first assigned material to objects, maybe better solution for this later
        meshRenderer.material.SetInt("_UseShader", 1);
    }

    public virtual void OnDeselection()
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
