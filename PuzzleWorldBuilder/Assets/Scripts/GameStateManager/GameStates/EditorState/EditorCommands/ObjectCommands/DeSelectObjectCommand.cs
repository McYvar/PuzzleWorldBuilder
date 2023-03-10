using System.Collections.Generic;

public class DeSelectObjectCommand : BaseEditorCommand
{
    Stack<SceneObject[]> redoStack;
    Stack<SceneObject[]> undoStack;

    protected override void OnEnable()
    {
        base.OnEnable();
        redoStack = new Stack<SceneObject[]>();
        undoStack = new Stack<SceneObject[]>();
    }

    public override void Execute()
    {
        SceneObject[] sceneObjects = new SceneObject[InputCommands.selectedObjects.Count];
        for (int i = 0; i < sceneObjects.Length; i++)
        {
            if (InputCommands.selectedObjects[i] != null) sceneObjects[i] = InputCommands.selectedObjects[i];
            sceneObjects[i].OnDeselection();
        }
        InputCommands.selectedObjects.Clear();
        undoStack.Push(sceneObjects);
        redoStack.Clear();
    }

    public override void Undo()
    {
        SceneObject[] sceneObjects = undoStack.Pop();
        for (int i = 0; i < sceneObjects.Length; i++)
        {
            if (sceneObjects[i] != null) InputCommands.selectedObjects.Add(sceneObjects[i]);
            sceneObjects[i].OnSelection();
        }
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
