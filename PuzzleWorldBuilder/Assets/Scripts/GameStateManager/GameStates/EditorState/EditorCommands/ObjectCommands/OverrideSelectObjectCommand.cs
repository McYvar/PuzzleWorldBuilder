using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverrideSelectObjectCommand : BaseObjectCommands
{
    /// <summary>
    /// Date: 03/15/2023, By: Yvar
    /// I felt like this was necessary...
    /// So it's a command activates when other objects in the scene are already selected, and you override this selection.
    /// </summary>

    List<TerrainObject> currentlySelected = new List<TerrainObject>();
    List<TerrainObject> nextSelected = new List<TerrainObject>();

    LinkedList<TerrainObject[]> curUndoLinkedList;
    LinkedList<TerrainObject[]> nextUndoLinkedList;

    Stack<TerrainObject[]> nextRedoStack;
    Stack<TerrainObject[]> currentRedoStack;

    protected override void OnEnable()
    {
        base.OnEnable();
        curUndoLinkedList = new LinkedList<TerrainObject[]>();
        nextUndoLinkedList = new LinkedList<TerrainObject[]>();

        currentRedoStack = new Stack<TerrainObject[]>();
        nextRedoStack = new Stack<TerrainObject[]>();
    }

    public override void Execute()
    {
        foreach (TerrainObject terrainObject in currentlySelected)
        {
            terrainObject.OnDeselection();
        }
        curUndoLinkedList.AddLast(currentlySelected.ToArray());

        foreach (TerrainObject terrainObject in nextSelected)
        {
            terrainObject.OnSelection();
        }
        nextUndoLinkedList.AddLast(nextSelected.ToArray());
    }

    public override void Undo()
    {
        TerrainObject[] nextSelected = nextUndoLinkedList.Last.Value;
        nextUndoLinkedList.RemoveLast();
        foreach (TerrainObject terrainObject in nextSelected)
        {
            terrainObject.OnDeselection();
        }
        nextRedoStack.Push(nextSelected);

        TerrainObject[] currentlySelected = curUndoLinkedList.Last.Value;
        curUndoLinkedList.RemoveLast();
        foreach (TerrainObject terrainObject in currentlySelected)
        {
            terrainObject.OnSelection();
        }
        currentRedoStack.Push(currentlySelected);
    }

    public override void Redo()
    {
        TerrainObject[] currentlySelected = currentRedoStack.Pop();
        foreach (TerrainObject terrainObject in currentlySelected)
        {
            terrainObject.OnDeselection();
        }
        curUndoLinkedList.AddLast(currentlySelected);

        TerrainObject[] nextSelected = nextRedoStack.Pop();
        foreach (TerrainObject terrainObject in nextSelected)
        {
            terrainObject.OnSelection();
        }
        nextUndoLinkedList.AddLast(nextSelected);
    }

    public override void ClearFirstUndo()
    {
        curUndoLinkedList.RemoveFirst();
        nextUndoLinkedList.RemoveFirst();
    }

    public override void ClearRedo()
    {
        currentRedoStack.Clear();
        nextRedoStack.Clear();
    }

    public void InitializeSelected(List<TerrainObject> currentlySelected, List<TerrainObject> nextSelected)
    {
        this.currentlySelected.Clear();
        this.currentlySelected.AddRange(currentlySelected);

        this.nextSelected.Clear();
        this.nextSelected.AddRange(nextSelected);
    }
}
