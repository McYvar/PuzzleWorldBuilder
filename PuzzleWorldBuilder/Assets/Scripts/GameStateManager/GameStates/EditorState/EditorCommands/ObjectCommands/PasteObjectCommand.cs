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

    LinkedList<TerrainObject[]> undoPrevSelected;
    Stack<TerrainObject[]> redoPrevSelected;

    protected override void OnEnable()
    {
        base.OnEnable();
        undoLinkedList = new LinkedList<TerrainObject[]>();
        redoStack = new Stack<TerrainObject[]>();

        undoPrevSelected = new LinkedList<TerrainObject[]>();
        redoPrevSelected = new Stack<TerrainObject[]>();
    }

    public override void Execute()
    {
        TerrainObject[] currentlySelected = InputCommands.selectedTerrainObjects.ToArray();
        foreach (TerrainObject terrainObject in currentlySelected)
        {
            terrainObject.OnDeselection();
        }
        undoPrevSelected.AddLast(currentlySelected);

        List<TerrainObject> newObj = new List<TerrainObject>();
        foreach (ClipBoard item in ClipBoard.clipboard)
        {
            TerrainObject createdObject = CreateObject(item.GetComponent<TerrainObject>());
            createdObject.name = item.normalName;
            createdObject.OnSelection();
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
            terrainObject.OnDeselection();
        }
        redoStack.Push(undoObjects);

        TerrainObject[] prevSelected = undoPrevSelected.Last.Value;
        undoPrevSelected.RemoveLast();
        foreach (TerrainObject terrainObject in prevSelected)
        {
            terrainObject.OnSelection();
        }
        redoPrevSelected.Push(prevSelected);
    }

    public override void Redo()
    {
        TerrainObject[] currentlySelected = redoPrevSelected.Pop();
        foreach (TerrainObject terrainObject in currentlySelected)
        {
            terrainObject.OnSelection();
        }
        undoPrevSelected.AddLast(currentlySelected);

        TerrainObject[] redoObjects = redoStack.Pop();
        foreach (TerrainObject terrainObject in redoObjects)
        {
            terrainObject.OnCreation();
            terrainObject.OnSelection();
        }
        undoLinkedList.AddLast(redoObjects);
    }

    public override void ClearFirstUndo()
    {
        undoLinkedList.RemoveFirst();
        undoPrevSelected.RemoveFirst();
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

        redoPrevSelected.Clear();
    }
}
