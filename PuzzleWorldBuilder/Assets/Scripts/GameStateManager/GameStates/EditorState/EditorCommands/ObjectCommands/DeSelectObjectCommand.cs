using System.Collections.Generic;
using UnityEngine;

public class DeselectObjectCommand : BaseEditorCommand
{
    Stack<SceneObject[]> undoStack;
    Stack<SceneObject[]> redoStack;
    List<SceneObject> preDeselected = new List<SceneObject>();

    protected override void OnEnable()
    {
        base.OnEnable();
        redoStack = new Stack<SceneObject[]>();
        undoStack = new Stack<SceneObject[]>();
    }

    public override void Execute()
    {
        SceneObject[] sceneObjects = preDeselected.ToArray();
        foreach (SceneObject sceneObject in sceneObjects)
        {
            sceneObject.OnDeselection();
            InputCommands.selectedObjects.Remove(sceneObject);
        }
        preDeselected.Clear();
        redoStack.Clear();
        undoStack.Push(sceneObjects);
    }

    public override void Undo()
    {
        SceneObject[] sceneObjects = undoStack.Pop();
        foreach (SceneObject sceneObject in sceneObjects)
        {
            sceneObject.OnSelection();
            InputCommands.selectedObjects.Add(sceneObject);
        }
        preDeselected.Clear();
        preDeselected.AddRange(sceneObjects);
        redoStack.Push(sceneObjects);
    }

    public override void Redo()
    {
        SceneObject[] sceneObjects = redoStack.Pop();
        foreach (SceneObject sceneObject in sceneObjects)
        {
            sceneObject.OnDeselection();
            InputCommands.selectedObjects.Remove(sceneObject);
        }
        preDeselected.Clear();
        undoStack.Push(sceneObjects);
    }

    public void InitializePreDeselected(List<SceneObject> preDeselected)
    {
        this.preDeselected.Clear();
        this.preDeselected.AddRange(preDeselected);
    }
}
