using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipSelectionObjectCommand : BaseObjectCommands
{
    List<SceneObject> flipSelected = new List<SceneObject>();
    List<SceneObject> newSelected = new List<SceneObject>();

    Stack<SceneObject[]> curUndoStack;
    Stack<SceneObject[]> newUndoStack;

    Stack<SceneObject[]> curRedoStack;
    Stack<SceneObject[]> newRedoStack;

    protected override void OnEnable()
    {
        base.OnEnable();
        newUndoStack = new Stack<SceneObject[]>();
        curUndoStack = new Stack<SceneObject[]>();

        curRedoStack = new Stack<SceneObject[]>();
        newRedoStack = new Stack<SceneObject[]>();
    }

    public override void Execute()
    {
        foreach (SceneObject sceneObject in flipSelected)
        {
            sceneObject.OnDeselection();
            InputCommands.selectedObjects.Remove(sceneObject);
        }
        curUndoStack.Push(flipSelected.ToArray());

        foreach (SceneObject sceneObject in newSelected)
        {
            sceneObject.OnSelection();
            InputCommands.selectedObjects.Add(sceneObject);
        }
        newUndoStack.Push(newSelected.ToArray());
    }

    public override void Undo()
    {
        SceneObject[] newSceneObjects = newUndoStack.Pop();
        foreach (SceneObject sceneObject in newSceneObjects)
        {
            sceneObject.OnDeselection();
            InputCommands.selectedObjects.Remove(sceneObject);
        }
        newRedoStack.Push(newSceneObjects);

        SceneObject[] curSceneObjects = curUndoStack.Pop();
        foreach (SceneObject sceneObject in curSceneObjects)
        {
            sceneObject.OnSelection();
            InputCommands.selectedObjects.Add(sceneObject);
        }
        curRedoStack.Push(curSceneObjects);
    }

    public override void Redo()
    {
        SceneObject[] curSceneObjects = curRedoStack.Pop();
        foreach (SceneObject sceneObject in curSceneObjects)
        {
            sceneObject.OnDeselection();
            InputCommands.selectedObjects.Remove(sceneObject);
        }
        curUndoStack.Push(curSceneObjects);

        SceneObject[] newSceneObjects = newRedoStack.Pop();
        foreach (SceneObject sceneObject in newSceneObjects)
        {
            sceneObject.OnSelection();
            InputCommands.selectedObjects.Add(sceneObject);
        }
        newUndoStack.Push(newSceneObjects);

    }

    public void InitializePreSelected(List<SceneObject> currentlySelected, List<SceneObject> newSelected)
    {
        this.newSelected.Clear();
        this.newSelected.AddRange(newSelected);

        flipSelected.Clear();
        foreach (SceneObject sceneObject in currentlySelected)
        {
            if (this.newSelected.Contains(sceneObject))
            {
                flipSelected.Add(sceneObject);
                this.newSelected.Remove(sceneObject);
            }
        }
    }
}
