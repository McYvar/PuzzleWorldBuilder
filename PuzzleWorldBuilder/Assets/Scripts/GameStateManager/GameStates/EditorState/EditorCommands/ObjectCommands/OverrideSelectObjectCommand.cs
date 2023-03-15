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

    List<SceneObject> previouslySelected = new List<SceneObject>();
    List<SceneObject> currentlySelected = new List<SceneObject>();

    Stack<SceneObject[]> prevUndoStack;
    Stack<SceneObject[]> curUndoStack;

    Stack<SceneObject[]> curRedoStack;
    Stack<SceneObject[]> prevRedoStack;

    protected override void OnEnable()
    {
        base.OnEnable();
        prevUndoStack = new Stack<SceneObject[]>();
        curUndoStack = new Stack<SceneObject[]>();

        prevRedoStack = new Stack<SceneObject[]>();
        curRedoStack = new Stack<SceneObject[]>();
    }

    public override void Execute()
    {
        foreach (SceneObject sceneObject in previouslySelected)
        {
            sceneObject.OnDeselection();
            InputCommands.selectedObjects.Remove(sceneObject);
        }
        prevUndoStack.Push(previouslySelected.ToArray());

        foreach (SceneObject sceneObject in currentlySelected)
        {
            sceneObject.OnSelection();
            InputCommands.selectedObjects.Add(sceneObject);
        }
        curUndoStack.Push(currentlySelected.ToArray());
    }

    public override void Undo()
    {
        SceneObject[] curSceneObjects = curUndoStack.Pop();
        foreach (SceneObject sceneObject in curSceneObjects)
        {
            sceneObject.OnDeselection();
            InputCommands.selectedObjects.Remove(sceneObject);
        }
        curRedoStack.Push(curSceneObjects);

        SceneObject[] prevSceneObjects = prevUndoStack.Pop();
        foreach (SceneObject sceneObject in prevSceneObjects)
        {
            sceneObject.OnSelection();
            InputCommands.selectedObjects.Add(sceneObject);
        }
        prevRedoStack.Push(prevSceneObjects);
    }

    public override void Redo()
    {
        SceneObject[] curSceneObjects = curRedoStack.Pop();
        foreach (SceneObject sceneObject in curSceneObjects)
        {
            sceneObject.OnDeselection();
            InputCommands.selectedObjects.Remove(sceneObject);
        }
        prevUndoStack.Push(curSceneObjects);

        SceneObject[] prevSceneObjects = prevRedoStack.Pop();
        foreach (SceneObject sceneObject in prevSceneObjects)
        {
            sceneObject.OnSelection();
            InputCommands.selectedObjects.Add(sceneObject);
        }
        curUndoStack.Push(prevSceneObjects);
    }

    public void InitializeSelected(List<SceneObject> previouslySelected, List<SceneObject> currentlySelected)
    {
        this.previouslySelected.Clear();
        this.previouslySelected.AddRange(previouslySelected);

        this.currentlySelected.Clear();
        this.currentlySelected.AddRange(currentlySelected);
    }
}
