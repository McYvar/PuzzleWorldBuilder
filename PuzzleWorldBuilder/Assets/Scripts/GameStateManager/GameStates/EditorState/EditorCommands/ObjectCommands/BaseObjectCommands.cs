using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseObjectCommands : BaseEditorCommand
{
    [SerializeField] Transform parent;

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    public void SetParent(Transform parent)
    {
        this.parent = parent;
    }

    public SceneObject CreateObject(SceneObject objectToCreate)
    {
        SceneObject newObject = Instantiate(objectToCreate, parent);
        newObject.name = objectToCreate.name;
        newObject.OnCreation();
        return newObject;
    }

    public SceneObject CreateInvisible(SceneObject objectToCreate)
    {
        SceneObject newObject = Instantiate(objectToCreate, parent);
        newObject.name = objectToCreate.name;
        newObject.OnInvisibe();
        return newObject;
    }
}
