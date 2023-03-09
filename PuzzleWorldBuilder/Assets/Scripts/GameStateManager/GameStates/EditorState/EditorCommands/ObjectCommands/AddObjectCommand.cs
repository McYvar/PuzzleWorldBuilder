using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddObjectCommand : BaseObjectCommands
{
    /// <summary>
    /// Date: 03/07/2023, By: Yvar
    /// A script that enherits the ICommand interface, thus making it a command.
    /// This command is able to add certain assets to the puzzle by (hopefully later dragging
    /// and dropping) pressing a button.
    /// Multiple instances of this class can exist
    /// </summary>
    
    // now a key, later a drag and drop
    [SerializeField] GameObject myObjectPrefab;

    Stack<GameObject> redoObjectStack;

    protected override void OnEnable()
    {
        base.OnEnable();
        redoObjectStack = new Stack<GameObject>();
    }

    public override void Execute()
    {
        /// Add object, link last object as previous to this one
        while (redoObjectStack.Count > 0)
        {
            Destroy(redoObjectStack.Pop());
        }
        GameObject[] toArray = { CreateObject(myObjectPrefab) };
        AddObjectToLinkedList(toArray);
    }

    public override void Undo()
    {
        /// Undo adding this object, link this object as next to last object
        redoObjectStack.Push(ObjectList.list.Last.Value[0]);
        DeleteObject(ObjectList.list.Last.Value[0]);
        ObjectList.list.RemoveLast();
    }

    public override void Redo() 
    {
        /// In this redo an instance of a destroyed game object should be instantiated again,
        /// however doing so is pretty hard since the information for it is delete thus should be
        /// stored somewhere else on destroying
        /// 
        /// Redo adding previous object linked to the most recent one, link redo object to the recent one
        GameObject redoObject = redoObjectStack.Pop();
        SceneObject sceneObject = redoObject.GetComponent<SceneObject>();
        if (sceneObject != null) sceneObject.OnCreation();
        GameObject[] toArray = { redoObject };
        AddObjectToLinkedList(toArray);
    }
}
