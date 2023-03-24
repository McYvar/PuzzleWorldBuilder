using System.Collections.Generic;
using UnityEngine;

public class DeselectObjectCommand : BaseEditorCommand
{
    LinkedList<TerrainObject[]> undoLinkedList;
    Stack<TerrainObject[]> redoStack;
    List<TerrainObject> preDeselected;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        undoLinkedList = new LinkedList<TerrainObject[]>();
        redoStack = new Stack<TerrainObject[]>();
        preDeselected = new List<TerrainObject>();
    }

    public override void Execute()
    {
        TerrainObject[] terrainObjects = preDeselected.ToArray();
        foreach (TerrainObject terrainObject in terrainObjects)
        {
            terrainObject.OnDeselection();
        }
        preDeselected.Clear();
        redoStack.Clear();
        undoLinkedList.AddLast(terrainObjects);
    }

    public override void Undo()
    {
        TerrainObject[] terrainObjects = undoLinkedList.Last.Value;
        undoLinkedList.RemoveLast();
        foreach (TerrainObject terrainObject in terrainObjects)
        {
            terrainObject.OnSelection();
        }
        preDeselected.Clear();
        preDeselected.AddRange(terrainObjects);
        redoStack.Push(terrainObjects);
    }

    public override void Redo()
    {
        TerrainObject[] terrainObjects = redoStack.Pop();
        foreach (TerrainObject terrainObject in terrainObjects)
        {
            terrainObject.OnDeselection();
        }
        preDeselected.Clear();
        undoLinkedList.AddLast(terrainObjects);
    }

    public override void ClearFirstUndo()
    {
        undoLinkedList.RemoveFirst();
    }

    public override void ClearRedo()
    {
        redoStack.Clear();
    }

    public void InitializePreDeselected(List<TerrainObject> preDeselected)
    {
        this.preDeselected.Clear();
        this.preDeselected.AddRange(preDeselected);
    }
}
