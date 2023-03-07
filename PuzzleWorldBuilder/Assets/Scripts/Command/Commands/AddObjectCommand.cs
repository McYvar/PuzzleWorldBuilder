using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddObjectCommand : MonoBehaviour, ICommand
{
    // now a key, later a drag and drop
    [SerializeField] KeyCode addObjectKey;

    [SerializeField] GameObject myObjectPrefab;
    [SerializeField] Transform ObjectList;
    
    // This linked list is static because if there only exists one instance of it which can only
    // consist of a max amount of elements instead of multiple per command
    public static LinkedList<GameObject> globalObjectUndoList = new LinkedList<GameObject>();
    Stack<GameObject> redoObjectStack = new Stack<GameObject>();

    public void OnEnable() => InputCommands.AddKeyCommand(addObjectKey, this);
    public void OnDisable() => InputCommands.RemoveCommand(addObjectKey);

    public void Execute()
    {
        /// Add object, link last object as previous to this one
        while (redoObjectStack.Count > 0)
        {
            Destroy(redoObjectStack.Pop());
        }
        AddObjectToLinkedList(CreateObject(myObjectPrefab));
    }

    public void Undo()
    {
        /// Undo adding this object, link this object as next to last object
        DeleteObject(globalObjectUndoList.Last.Value);
        redoObjectStack.Push(globalObjectUndoList.Last.Value);
        globalObjectUndoList.RemoveLast();
    }

    public void Redo() 
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
        GameObject newObject = Instantiate(objectToCreate, ObjectList);
        newObject.name = objectToCreate.name;

        newObject.AddComponent<AddedObject>().OnCreation();

        return newObject;
    }

    public void DeleteObject(GameObject objectToDelete)
    {
        objectToDelete.GetComponent<AddedObject>().OnDeletion();
    }

    public void AddObjectToLinkedList(GameObject objectToAdd)
    {
        if (globalObjectUndoList.Count > CommandManager.globalMaxUndoAmount)
        {
            Destroy(globalObjectUndoList.First.Value);
            globalObjectUndoList.RemoveFirst();
        }

        globalObjectUndoList.AddLast(objectToAdd);
    }
}

[RequireComponent(typeof(MeshRenderer))]
public class AddedObject : MonoBehaviour
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
}
