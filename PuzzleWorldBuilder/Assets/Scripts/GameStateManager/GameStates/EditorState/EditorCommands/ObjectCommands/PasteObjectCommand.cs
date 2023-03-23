using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PasteObjectCommand : BaseObjectCommands
{
    /// <summary>
    /// Date: 03/10/2023, By: Yvar
    /// The paste command pastes the available items on the "clipboard".
    /// </summary>
    /// 

    LinkedList<TerrainObject[]> undoLinkedList;
    Stack<TerrainObject[]> redoStack;

    protected override void OnEnable()
    {
        base.OnEnable();
        undoLinkedList = new LinkedList<TerrainObject[]>();
        redoStack = new Stack<TerrainObject[]>();
    }

    public override void Execute()
    {
        List<TerrainObject> newObj = new List<TerrainObject>();
        foreach (ClipBoard item in ClipBoard.clipboard)
        {
            TerrainObject createdObject = CreateObject(item.GetComponent<TerrainObject>());
            createdObject.name = item.normalName;
            newObj.Add(createdObject);
        }

        foreach (TerrainObject terrainObject in newObj)
        {
            ClipBoard clipBoard = terrainObject.GetComponent<ClipBoard>();
            if (clipBoard != null) Destroy(clipBoard);
        }

        undoLinkedList.AddLast(newObj.ToArray());
    }

    public override void Undo()
    {
        TerrainObject[] undoObjects = undoLinkedList.Last.Value;
        undoLinkedList.RemoveLast();
        foreach (TerrainObject terrainObject in undoObjects)
        {
            terrainObject.OnDeletion();
        }
        redoStack.Push(undoObjects);
    }

    public override void Redo()
    {
        TerrainObject[] redoObjects = redoStack.Pop();
        foreach (TerrainObject terrainObject in redoObjects)
        {
            terrainObject.OnCreation();
        }
        undoLinkedList.AddLast(redoObjects);
    }

    public override void ClearFirstUndo()
    {
        undoLinkedList.RemoveFirst();
    }

    public override void ClearRedo()
    {
        while (redoStack.Count > 0)
        {
            foreach (TerrainObject terrainObject in redoStack.Pop())
            {
                Destroy(terrainObject.gameObject);
            }
        }
    }
}
