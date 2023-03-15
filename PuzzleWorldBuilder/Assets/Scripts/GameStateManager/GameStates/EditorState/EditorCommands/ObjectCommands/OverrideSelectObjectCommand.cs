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
        foreach (SceneObject sceneObject in previouslySelected)
        {
            sceneObject.OnDeselection();
        }
        foreach (SceneObject sceneObject in InputCommands.selectedObjects)
        {
            sceneObject.OnSelection();
        }
        undoStack.Push(previouslySelected.ToArray());
    }

    public override void Undo()
    {
        foreach (SceneObject sceneObject in InputCommands.selectedObjects)
        {
            sceneObject.OnDeselection();
        }
        SceneObject[] sceneObjects = undoStack.Pop();
        previouslySelected.Clear();
        previouslySelected.AddRange(InputCommands.selectedObjects);
        InputCommands.selectedObjects.Clear();
        InputCommands.selectedObjects.AddRange(sceneObjects);
        foreach (SceneObject sceneObject in sceneObjects)
        {
            sceneObject.OnSelection();
        }
        redoStack.Push(previouslySelected.ToArray());
    }

    public override void Redo()
    {
        foreach (SceneObject sceneObject in InputCommands.selectedObjects)
        {
            sceneObject.OnDeselection();
        }
        SceneObject[] sceneObjects = redoStack.Pop();
        previouslySelected.Clear();
        previouslySelected.AddRange(InputCommands.selectedObjects);
        InputCommands.selectedObjects.Clear();
        InputCommands.selectedObjects.AddRange(sceneObjects);
        foreach (SceneObject sceneObject in sceneObjects)
        {
            sceneObject.OnSelection();
        }
        undoStack.Push(previouslySelected.ToArray());
    }

    public void InitializePrevSelected(List<SceneObject> previouslySelected)
    {
        this.previouslySelected.Clear();
        this.previouslySelected.AddRange(previouslySelected);
    }
}
