using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteObjectCommand : BaseObjectCommands
{
    /// <summary>
    /// Date: 03/09/2023, By: Yvar
    /// A script that enherits the ICommand interface, thus making it a command.
    /// This command is responsible for savely removing objects from the puzzle scene.
    /// Only one instance of this command should exist
    /// </summary>

    Stack<TerrainObject[]> redoStack;
    LinkedList<TerrainObject[]> undoLinkedList;

    protected override void OnEnable()
    {
        base.OnEnable();
        redoStack = new Stack<TerrainObject[]>();
        undoLinkedList = new LinkedList<TerrainObject[]>();
    }

    public override void Execute()
    {
        // When we delete an object in the puzzlemaker that is selected, we should also remove it from the selected objects list
        TerrainObject[] currentlySelectedObjects = InputCommands.selectedTerrainObjects.ToArray();
        foreach (TerrainObject terrainObject in currentlySelectedObjects)
        {
            terrainObject.OnDeletion();
            InputCommands.selectedTerrainObjects.Remove(terrainObject);
        }
        undoLinkedList.AddLast(currentlySelectedObjects);
    }

    public override void Undo()
    {
        TerrainObject[] undoSelctedObjects = undoLinkedList.Last.Value;
        undoLinkedList.RemoveLast();
        foreach (TerrainObject terrainObject in undoSelctedObjects)
        {
            terrainObject.OnCreation();
            InputCommands.selectedTerrainObjects.Add(terrainObject);
        }
        redoStack.Push(undoSelctedObjects);
    }

    public override void Redo()
    {
        TerrainObject[] redoSelctedObjects = redoStack.Pop();
        foreach (TerrainObject terrainObject in redoSelctedObjects)
        {
            terrainObject.OnDeletion();
            InputCommands.selectedTerrainObjects.Remove(terrainObject);
        }
        undoLinkedList.AddLast(redoSelctedObjects);
    }

    public override void ClearFirstUndo()
    {
        TerrainObject[] terrainObjects = undoLinkedList.First.Value;
        foreach (TerrainObject terrainObject in terrainObjects)
        {
            Destroy(terrainObject.gameObject);
        }
        undoLinkedList.RemoveFirst();
    }

    public override void ClearRedo()
    {
        redoStack.Clear();
    }
}
