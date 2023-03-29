using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class SceneObject : AbstractGameEditor
{
    // static list of all objects visible in the scene
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
        InputCommands.selectedObjects.Add(this);
    }

    public virtual void OnDeselection()
    {
        InputCommands.selectedObjects.Remove(this);
    }

    public virtual void MoveTo(Vector3 newPos) 
    {
    }

    public virtual void OnStartMove()
    {
        myStartPos = transform.position;
    }

    public virtual void OnFinishMove()
    {
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

    public virtual void InitializeOnLoad() { }
}

[System.Serializable]
public class SceneObjectData
{
    public Vector3 position;

    public SceneObjectData()
    {
        position = Vector3.zero;
    }
}