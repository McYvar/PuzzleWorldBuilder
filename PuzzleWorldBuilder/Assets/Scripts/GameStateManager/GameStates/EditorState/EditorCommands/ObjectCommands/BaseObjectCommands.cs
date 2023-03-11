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

    public GameObject CreateObject(GameObject objectToCreate)
    {
        GameObject newObject = Instantiate(objectToCreate, parent);
        newObject.name = objectToCreate.name;

        SceneObject sceneObject = newObject.GetComponent<SceneObject>();
        if (sceneObject != null)sceneObject.OnCreation();
        else newObject.AddComponent<SceneObject>().OnCreation();

        return newObject;
    }

    public GameObject CreateInvisible(GameObject objectToCreate)
    {
        GameObject newObject = Instantiate(objectToCreate, parent);
        newObject.name = objectToCreate.name;
        SceneObject sceneObject = newObject.GetComponent<SceneObject>();
        if (sceneObject != null) sceneObject.OnDeletion();
        else newObject.AddComponent<SceneObject>().OnDeletion();
        return newObject;
    }

    /// Maybe by deleting a selection of objects in the future, parent them under a new game object,
    /// then upon undo, unparent them again...
    /// Update: approach is different now, with GameObject arrays
    public void DeleteObject(GameObject objectToDelete)
    {
        // When this function is called, the objects have to be manually removed from the GO linked list
        SceneObject sceneObject = objectToDelete.GetComponent<SceneObject>();
        if (sceneObject != null) sceneObject.OnDeletion();
    }
}
