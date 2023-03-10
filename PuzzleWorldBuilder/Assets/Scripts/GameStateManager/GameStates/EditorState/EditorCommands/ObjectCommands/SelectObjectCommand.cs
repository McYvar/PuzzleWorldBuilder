using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectObjectCommand : BaseObjectCommands
{
    Stack<SceneObject[]> redoStack;

    protected override void OnEnable()
    {
        base.OnEnable();
        redoStack = new Stack<SceneObject[]>();
    }
    
    public override void Execute()
    {
        foreach (SceneObject sceneObject in InputCommands.selectedObjects)
        {
            sceneObject.OnSelection();
        }
        redoStack.Clear();
    }

    public override void Undo()
    {
        SceneObject[] redoSceneObjects = new SceneObject[InputCommands.selectedObjects.Count];
        for(int i = 0; i < redoSceneObjects.Length; i++)
        {
            InputCommands.selectedObjects[i].OnDeselection();
            redoSceneObjects[i] = InputCommands.selectedObjects[i];
        }
        InputCommands.selectedObjects.Clear();
        redoStack.Push(redoSceneObjects);
    }

    public override void Redo()
    {
        SceneObject[] redoObjects = redoStack.Pop();
        for(int i = 0;i < redoObjects.Length;i++)
        {
            redoObjects[i].OnSelection();
            InputCommands.selectedObjects.Add(redoObjects[i]);
        }
    }
}
