using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectObjectCommand : BaseObjectCommands
{
    Stack<SceneObject[]> undoStack;
    Stack<SceneObject[]> redoStack;
    List<SceneObject> preSelected = new List<SceneObject>();

    protected override void OnEnable()
    {
        base.OnEnable();
        redoStack = new Stack<SceneObject[]>();
        undoStack = new Stack<SceneObject[]>();
    }
    
    public override void Execute()
    {
        foreach (SceneObject sceneObject in preSelected)
        {
            sceneObject.OnSelection();
            InputCommands.selectedObjects.Add(sceneObject);
        }
        redoStack.Clear();
        undoStack.Push(preSelected.ToArray());
    }

    public override void Undo()
    {
        SceneObject[] redoObjects = undoStack.Pop();
        foreach (SceneObject sceneObject in redoObjects)
        {
            sceneObject.OnDeselection();
            InputCommands.selectedObjects.Remove(sceneObject);
        }
        preSelected.Clear();
        redoStack.Push(redoObjects);
    }

    public override void Redo()
    {
        SceneObject[] redoObjects = redoStack.Pop();
        preSelected.Clear();
        preSelected.AddRange(redoObjects);
        foreach (SceneObject sceneObject in preSelected)
        {
            sceneObject.OnSelection();
            InputCommands.selectedObjects.Add(sceneObject);
        }
        undoStack.Push(redoObjects);
    }

    public void InitializePreSelected(List<SceneObject> currentlySelected, List<SceneObject> preSelected)
    {
        this.preSelected.Clear();
        this.preSelected.AddRange(preSelected);

        foreach(SceneObject sceneObject in currentlySelected)
        {
            if (this.preSelected.Contains(sceneObject))
                this.preSelected.Remove(sceneObject);
        }
    }
}
