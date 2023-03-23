using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipSelectionObjectCommand : BaseObjectCommands
{
    List<TerrainObject> flipSelected = new List<TerrainObject>();
    List<TerrainObject> newSelected = new List<TerrainObject>();

    LinkedList<TerrainObject[]> curUndoLinkedList;
    LinkedList<TerrainObject[]> newUndoLinkedList;

    Stack<TerrainObject[]> curRedoStack;
    Stack<TerrainObject[]> newRedoStack;

    protected override void OnEnable()
    {
        base.OnEnable();
        newUndoLinkedList = new LinkedList<TerrainObject[]>();
        curUndoLinkedList = new LinkedList<TerrainObject[]>();

        curRedoStack = new Stack<TerrainObject[]>();
        newRedoStack = new Stack<TerrainObject[]>();
    }

    public override void Execute()
    {
        foreach (TerrainObject terrainObject in flipSelected)
        {
            terrainObject.OnDeselection();
        }
        curUndoLinkedList.AddLast(flipSelected.ToArray());

        foreach (TerrainObject terrainObject in newSelected)
        {
            terrainObject.OnSelection();
        }
        newUndoLinkedList.AddLast(newSelected.ToArray());
    }

    public override void Undo()
    {
        TerrainObject[] newTerrainObjects = newUndoLinkedList.Last.Value;
        newUndoLinkedList.RemoveLast();
        foreach (TerrainObject terrainObject in newTerrainObjects)
        {
            terrainObject.OnDeselection();
        }
        newRedoStack.Push(newTerrainObjects);

        TerrainObject[] curTerrainObjects = curUndoLinkedList.Last.Value;
        curUndoLinkedList.RemoveLast();
        foreach (TerrainObject terrainObject in curTerrainObjects)
        {
            terrainObject.OnSelection();
        }
        curRedoStack.Push(curTerrainObjects);
    }

    public override void Redo()
    {
        TerrainObject[] curTerrainObjects = curRedoStack.Pop();
        foreach (TerrainObject terrainObject in curTerrainObjects)
        {
            terrainObject.OnDeselection();
        }
        curUndoLinkedList.AddLast(curTerrainObjects);

        TerrainObject[] newTerrainObjects = newRedoStack.Pop();
        foreach (TerrainObject terrainObject in newTerrainObjects)
        {
            terrainObject.OnSelection();
        }
        newUndoLinkedList.AddLast(newTerrainObjects);

    }

    public override void ClearFirstUndo()
    {
        curUndoLinkedList.RemoveFirst();
        newUndoLinkedList.RemoveFirst();
    }

    public override void ClearRedo()
    {
        curRedoStack.Clear();
        newRedoStack.Clear();
    }

    public void InitializePreSelected(List<TerrainObject> currentlySelected, List<TerrainObject> newSelected)
    {
        this.newSelected.Clear();
        this.newSelected.AddRange(newSelected);

        flipSelected.Clear();
        foreach (TerrainObject terrainObject in currentlySelected)
        {
            if (this.newSelected.Contains(terrainObject))
            {
                flipSelected.Add(terrainObject);
                this.newSelected.Remove(terrainObject);
            }
        }
    }
}
