using System.Collections.Generic;
using UnityEngine;

public class DeselectObjectCommand : BaseEditorCommand
{
    LinkedList<SceneObject[]> undoLinkedList;
    Stack<SceneObject[]> redoStack;
    List<SceneObject> preDeselected;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        undoLinkedList = new LinkedList<SceneObject[]>();
        redoStack = new Stack<SceneObject[]>();
        preDeselected = new List<SceneObject>();
    }

    public override void Execute()
    {
        SceneObject[] sceneObjects = preDeselected.ToArray();
        foreach (SceneObject sceneObject in sceneObjects)
        {
            sceneObject.OnDeselection();
        }
        preDeselected.Clear();
        redoStack.Clear();
        undoLinkedList.AddLast(sceneObjects);
    }

    public override void Undo()
    {
        SceneObject[] sceneObjects = undoLinkedList.Last.Value;
        undoLinkedList.RemoveLast();
        foreach (SceneObject sceneObject in sceneObjects)
        {
            sceneObject.OnSelection();
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
        }
        preDeselected.Clear();
        undoLinkedList.AddLast(sceneObjects);
    }

    public override void ClearFirstUndo()
    {
        undoLinkedList.RemoveFirst();
    }

    public override void ClearRedo()
    {
        redoStack.Clear();
    }

    public void InitializePreDeselected(List<SceneObject> preDeselected)
    {
        this.preDeselected.Clear();
        this.preDeselected.AddRange(preDeselected);
    }
}
