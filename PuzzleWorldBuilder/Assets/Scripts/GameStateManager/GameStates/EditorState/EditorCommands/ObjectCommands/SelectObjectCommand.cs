using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectObjectCommand : BaseObjectCommands
{
    LinkedList<TerrainObject[]> undoLinkedList;
    Stack<TerrainObject[]> redoStack;
    List<TerrainObject> preSelected = new List<TerrainObject>();

    protected override void OnEnable()
    {
        base.OnEnable();
        undoLinkedList = new LinkedList<TerrainObject[]>();
        redoStack = new Stack<TerrainObject[]>();
    }
    
    public override void Execute()
    {
        foreach (TerrainObject terrainObject in preSelected)
        {
            terrainObject.OnSelection();
        }
        undoLinkedList.AddLast(preSelected.ToArray());
    }

    public override void Undo()
    {
        TerrainObject[] redoObjects = undoLinkedList.Last.Value;
        undoLinkedList.RemoveLast();
        foreach (TerrainObject terrainObject in redoObjects)
        {
            terrainObject.OnDeselection();
        }
        preSelected.Clear();
        redoStack.Push(redoObjects);
    }

    public override void Redo()
    {
        TerrainObject[] redoObjects = redoStack.Pop();
        preSelected.Clear();
        preSelected.AddRange(redoObjects);
        foreach (TerrainObject terrainObject in preSelected)
        {
            terrainObject.OnSelection();
        }
        undoLinkedList.AddLast(redoObjects);
    }

    public override void ClearFirstUndo()
    {
        undoLinkedList.RemoveFirst();
    }

    public override void ClearRedo()
    {
        redoStack.Clear();
    }

    public int InitializePreSelected(List<TerrainObject> currentlySelected, List<TerrainObject> preSelected)
    {
        this.preSelected.Clear();
        this.preSelected.AddRange(preSelected);

        foreach(TerrainObject terrainObject in currentlySelected)
        {
            if (this.preSelected.Contains(terrainObject))
                this.preSelected.Remove(terrainObject);
        }
        return this.preSelected.Count;
    }
}
