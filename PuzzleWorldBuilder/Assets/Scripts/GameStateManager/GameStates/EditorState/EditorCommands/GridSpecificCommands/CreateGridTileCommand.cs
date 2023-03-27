using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateGridTileCommand : BaseEditorCommand
{
    GridObject[] tilesToCreate;
    Stack<GridObject[]> redoStack;
    LinkedList<GridObject[]> undoLinkedList;

    protected override void OnEnable()
    {
        base.OnEnable();
        redoStack = new Stack<GridObject[]>();
        undoLinkedList = new LinkedList<GridObject[]>();
    }

    public override void Execute()
    {
        foreach (GridObject gridObject in tilesToCreate)
        {
            gridObject.OnCreation();
            gridObject.OnSelection();
        }
        undoLinkedList.AddLast(tilesToCreate);
    }

    public override void Undo()
    {
        GridObject[] undoTiles = undoLinkedList.Last.Value;
        undoLinkedList.RemoveLast();
        foreach (GridObject gridObject in undoTiles)
        {
            gridObject.OnDeletion();
            gridObject.OnDeselection();
        }
        redoStack.Push(undoTiles);
    }

    public override void Redo()
    {
        GridObject[] redoTiles = redoStack.Pop();
        foreach (GridObject gridObject in redoTiles)
        {
            gridObject.OnCreation();
            gridObject.OnSelection();
        }
        undoLinkedList.AddLast(redoTiles);
    }

    public override void ClearFirstUndo()
    {
        undoLinkedList.RemoveFirst();
    }

    public override void ClearRedo()
    {
        redoStack.Clear();
    }

    public void InitializeTiles(GridObject[] tiles)
    {
        tilesToCreate = tiles;
    }
}
