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
    [SerializeField] SceneObject myObjectPrefab;
    LinkedList<SceneObject> undoLinkedList = new LinkedList<SceneObject>();
    Stack<SceneObject> redoStack = new Stack<SceneObject>();

    public override void Execute()
    {
        // Add object, link last object as previous to this one
        TerrainObject createdObject = CreateObject(myObjectPrefab) as TerrainObject;
        createdObject.myData.name = createdObject.name;
        undoLinkedList.AddLast(createdObject);
    }

    public override void Undo()
    {
        // Undo adding this object, link this object as next to last object
        SceneObject sceneObject = undoLinkedList.Last.Value;
        undoLinkedList.RemoveLast();
        sceneObject.OnDeletion();
        redoStack.Push(sceneObject);
    }

    public override void Redo() 
    {
        /// In this redo an instance of a destroyed game object should be instantiated again,
        /// however doing so is pretty hard since the information for it is delete thus should be
        /// stored somewhere else on destroying
        /// 
        /// Redo adding previous object linked to the most recent one, link redo object to the recent one
        SceneObject sceneObject = redoStack.Pop();
        sceneObject.OnCreation();
        undoLinkedList.AddLast(sceneObject);
    }

    public override void ClearFirstUndo()
    {
        undoLinkedList.RemoveFirst();
    }

    public override void ClearRedo()
    {
        while (redoStack.Count > 0)
        {
            Destroy(redoStack.Pop().gameObject);
        }
    }
}
