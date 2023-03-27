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

    Stack<SceneObject[]> redoStack;
    LinkedList<SceneObject[]> undoLinkedList;

    protected override void OnEnable()
    {
        base.OnEnable();
        redoStack = new Stack<SceneObject[]>();
        undoLinkedList = new LinkedList<SceneObject[]>();
    }

    public override void Execute()
    {
        // When we delete an object in the puzzlemaker that is selected, we should also remove it from the selected objects list
        SceneObject[] currentlySelectedObjects = InputCommands.selectedObjects.ToArray();
        foreach (SceneObject sceneObject in currentlySelectedObjects)
        {
            sceneObject.OnDeletion();
            InputCommands.selectedObjects.Remove(sceneObject);
        }
        undoLinkedList.AddLast(currentlySelectedObjects);
    }

    public override void Undo()
    {
        SceneObject[] undoSelctedObjects = undoLinkedList.Last.Value;
        undoLinkedList.RemoveLast();
        if (undoSelctedObjects[0] as TerrainObject)
        {
            foreach (TerrainObject terrainObject in undoSelctedObjects)
            {
                terrainObject.OnCreation();
                InputCommands.selectedObjects.Add(terrainObject);
            }
        }
        else if (undoSelctedObjects[0] as GridObject)
        {
            foreach (GridObject gridObject in undoSelctedObjects)
            {
                gridObject.OnReCreation();
                InputCommands.selectedObjects.Add(gridObject);
            }
        }
        redoStack.Push(undoSelctedObjects);
    }

    public override void Redo()
    {
        SceneObject[] redoSelctedObjects = redoStack.Pop();
        foreach (SceneObject sceneObject in redoSelctedObjects)
        {
            sceneObject.OnDeletion();
            InputCommands.selectedObjects.Remove(sceneObject);
        }
        undoLinkedList.AddLast(redoSelctedObjects);
    }

    public override void ClearFirstUndo()
    {
        SceneObject[] sceneObjects = undoLinkedList.First.Value;
        foreach (SceneObject sceneObject in sceneObjects)
        {
            Destroy(sceneObject.gameObject);
        }
        undoLinkedList.RemoveFirst();
    }

    public override void ClearRedo()
    {
        redoStack.Clear();
    }
}
