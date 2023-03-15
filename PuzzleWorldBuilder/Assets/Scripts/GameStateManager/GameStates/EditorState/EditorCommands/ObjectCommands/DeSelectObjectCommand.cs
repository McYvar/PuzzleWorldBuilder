using System.Collections.Generic;
using UnityEngine;

public class DeSelectObjectCommand : BaseEditorCommand
{
    Stack<SceneObject[]> undoStack;
    Stack<SceneObject[]> redoStack;

    protected override void OnEnable()
    {
        base.OnEnable();
        redoStack = new Stack<SceneObject[]>();
        undoStack = new Stack<SceneObject[]>();
    }

    public override void Execute()
    {
        SceneObject[] sceneObjects = InputCommands.selectedObjects.ToArray();
        foreach (SceneObject sceneObject in sceneObjects)
        {
            sceneObject.OnDeselection();
        }
        InputCommands.selectedObjects.Clear();
        redoStack.Clear();
        undoStack.Push(sceneObjects);
    }

    public override void Undo()
    {
        SceneObject[] sceneObjects = undoStack.Pop();
        foreach (SceneObject sceneObject in sceneObjects)
        {
            sceneObject.OnSelection();
        }
        InputCommands.selectedObjects.Clear();
        InputCommands.selectedObjects.AddRange(sceneObjects);
        redoStack.Push(sceneObjects);
    }

    public override void Redo()
    {
        SceneObject[] sceneObjects = redoStack.Pop();
        foreach (SceneObject sceneObject in sceneObjects)
        {
            sceneObject.OnDeselection();
        }
        InputCommands.selectedObjects.Clear();
        undoStack.Push(sceneObjects);
    }
}
