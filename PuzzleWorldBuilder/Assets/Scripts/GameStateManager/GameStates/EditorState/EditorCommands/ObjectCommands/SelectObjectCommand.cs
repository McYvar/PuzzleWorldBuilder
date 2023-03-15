using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectObjectCommand : BaseObjectCommands
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
        foreach (SceneObject sceneObject in InputCommands.selectedObjects)
        {
            sceneObject.OnSelection();
        }
        redoStack.Clear();
        undoStack.Push(InputCommands.selectedObjects.ToArray());
    }

    public override void Undo()
    {
        SceneObject[] redoObjects = undoStack.Pop();
        foreach (SceneObject sceneObject in redoObjects)
        {
            sceneObject?.OnDeselection();
        }
        InputCommands.selectedObjects.Clear();
        redoStack.Push(redoObjects);
    }

    public override void Redo()
    {
        SceneObject[] redoObjects = redoStack.Pop();
        InputCommands.selectedObjects.Clear();
        InputCommands.selectedObjects.AddRange(redoObjects);
        foreach (SceneObject sceneObject in InputCommands.selectedObjects)
        {
            sceneObject?.OnSelection();
        }
        undoStack.Push(redoObjects);
    }
}
