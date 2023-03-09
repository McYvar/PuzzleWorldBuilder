using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseObjectCommands : BaseEditorCommand
{
    Transform parent;

    protected override void OnEnable()
    {
        base.OnEnable();
        parent = FindObjectOfType<ObjectList>().transform; // ugly... but only for once I suppose... for each instance of this script...
    }

    public GameObject CreateObject(GameObject objectToCreate)
    {
        GameObject newObject = Instantiate(objectToCreate, parent);
        newObject.name = objectToCreate.name;

        newObject.AddComponent<SceneObject>().OnCreation();

        return newObject;
    }

    /// Maybe by deleting a selection of objects in the future, parent them under a new game object,
    /// then upon undo, unparent them again...
    public void DeleteObject(GameObject objectToDelete)
    {
        SceneObject sceneObject = objectToDelete.GetComponent<SceneObject>();
        if (sceneObject != null) sceneObject.OnDeletion();
    }

    public void AddObjectToLinkedList(GameObject[] objectToAdd)
    {
        if (ObjectList.list.Count > CommandManager.globalMaxUndoAmount)
        {
            ObjectList.list.RemoveFirst();
        }

        ObjectList.list.AddLast(objectToAdd);
    }
}
