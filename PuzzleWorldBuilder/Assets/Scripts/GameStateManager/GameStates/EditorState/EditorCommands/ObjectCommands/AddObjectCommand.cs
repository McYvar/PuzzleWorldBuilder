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
    [SerializeField] TerrainObject myObjectPrefab;
    LinkedList<TerrainObject> undoLinkedList = new LinkedList<TerrainObject>();
    Stack<TerrainObject> redoStack = new Stack<TerrainObject>();

    public override void Execute()
    {
        // Add object, link last object as previous to this one
        undoLinkedList.AddLast(CreateObject(myObjectPrefab));
    }

    public override void Undo()
    {
        // Undo adding this object, link this object as next to last object
        TerrainObject terrainObject = undoLinkedList.Last.Value;
        undoLinkedList.RemoveLast();
        terrainObject.OnDeletion();
        redoStack.Push(terrainObject);
    }

    public override void Redo() 
    {
        /// In this redo an instance of a destroyed game object should be instantiated again,
        /// however doing so is pretty hard since the information for it is delete thus should be
        /// stored somewhere else on destroying
        /// 
        /// Redo adding previous object linked to the most recent one, link redo object to the recent one
        TerrainObject terrainObject = redoStack.Pop();
        terrainObject.OnCreation();
        undoLinkedList.AddLast(terrainObject);
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
