using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddObjectCommand : BaseCommand, ICommand
{
    /// <summary>
    /// Date: 03/07/2023, By: Yvar
    /// A script that enherits the ICommand interface, thus making it a command.
    /// This command is able to add certain assets to the puzzle by (hopefully later dragging
    /// and dropping) pressing a button
    /// </summary>
    // now a key, later a drag and drop
    [SerializeField] GameObject myObjectPrefab;

    Transform parent;

    Stack<GameObject> redoObjectStack = new Stack<GameObject>();

    public override void OnEnable() 
    {
        base.OnEnable();
        parent = FindObjectOfType<ObjectList>().transform; // ugly... but only once I suppose
    }

    public override void Execute()
    {
        /// Add object, link last object as previous to this one
        while (redoObjectStack.Count > 0)
        {
            Destroy(redoObjectStack.Pop());
        }
        AddObjectToLinkedList(CreateObject(myObjectPrefab));
    }

    public override void Undo()
    {
        /// Undo adding this object, link this object as next to last object
        DeleteObject(ObjectList.globalObjectUndoList.Last.Value);
        redoObjectStack.Push(ObjectList.globalObjectUndoList.Last.Value);
        ObjectList.globalObjectUndoList.RemoveLast();
    }

    public override void Redo() 
    {
        /// In this redo an instance of a destroyed game object should be instantiated again,
        /// however doing so is pretty hard since the information for it is delete thus should be
        /// stored somewhere else on destroying
        /// 
        /// Redo adding previous object linked to the most recent one, link redo object to the recent one
        GameObject redoObject = redoObjectStack.Pop();
        redoObject.GetComponent<AddedObject>().OnCreation();
        AddObjectToLinkedList(redoObject);
    }

    public GameObject CreateObject(GameObject objectToCreate)
    {
        GameObject newObject = Instantiate(objectToCreate, parent);
        newObject.name = objectToCreate.name;

        newObject.AddComponent<AddedObject>().OnCreation();

        return newObject;
    }
    
    /// Maybe by deleting a selection of objects in the future, parent them under a new game object,
    /// then upon undo, unparent them again...
    public void DeleteObject(GameObject objectToDelete)
    {
        objectToDelete.GetComponent<AddedObject>().OnDeletion();
    }

    public void AddObjectToLinkedList(GameObject objectToAdd)
    {
        if (ObjectList.globalObjectUndoList.Count > CommandManager.globalMaxUndoAmount)
        {
            ObjectList.globalObjectUndoList.RemoveFirst();
        }

        ObjectList.globalObjectUndoList.AddLast(objectToAdd);
    }
}

[RequireComponent(typeof(MeshRenderer))]
public class AddedObject : GameEditor
{
    MeshRenderer meshRenderer;

    private void OnEnable()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void OnCreation()
    {
        meshRenderer.enabled = true;
    }

    public void OnDeletion()
    {
        meshRenderer.enabled = false;
    }

    public override void EditorUpdate()
    {
    }
}
