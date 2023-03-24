using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipSelectionObjectCommand : BaseObjectCommands
{
    List<SceneObject> flipSelected = new List<SceneObject>();
    List<SceneObject> newSelected = new List<SceneObject>();

    LinkedList<SceneObject[]> curUndoLinkedList;
    LinkedList<SceneObject[]> newUndoLinkedList;

    Stack<SceneObject[]> curRedoStack;
    Stack<SceneObject[]> newRedoStack;

    protected override void OnEnable()
    {
        base.OnEnable();
        newUndoLinkedList = new LinkedList<SceneObject[]>();
        curUndoLinkedList = new LinkedList<SceneObject[]>();

        curRedoStack = new Stack<SceneObject[]>();
        newRedoStack = new Stack<SceneObject[]>();
    }

    public override void Execute()
    {
        foreach (SceneObject sceneObject in flipSelected)
        {
            sceneObject.OnDeselection();
        }
        curUndoLinkedList.AddLast(flipSelected.ToArray());

        foreach (SceneObject sceneObject in newSelected)
        {
            sceneObject.OnSelection();
        }
        newUndoLinkedList.AddLast(newSelected.ToArray());
    }

    public override void Undo()
    {
        SceneObject[] newSceneObjects = newUndoLinkedList.Last.Value;
        newUndoLinkedList.RemoveLast();
        foreach (SceneObject sceneObject in newSceneObjects)
        {
            sceneObject.OnDeselection();
        }
        newRedoStack.Push(newSceneObjects);

        SceneObject[] curSceneObjects = curUndoLinkedList.Last.Value;
        curUndoLinkedList.RemoveLast();
        foreach (SceneObject sceneObject in curSceneObjects)
        {
            sceneObject.OnSelection();
        }
        curRedoStack.Push(curSceneObjects);
    }

    public override void Redo()
    {
        SceneObject[] curSceneObjects = curRedoStack.Pop();
        foreach (SceneObject sceneObject in curSceneObjects)
        {
            sceneObject.OnDeselection();
        }
        curUndoLinkedList.AddLast(curSceneObjects);

        SceneObject[] newSceneObjects = newRedoStack.Pop();
        foreach (SceneObject sceneObject in newSceneObjects)
        {
            sceneObject.OnSelection();
        }
        newUndoLinkedList.AddLast(newSceneObjects);

    }

    public override void ClearFirstUndo()
    {
        curUndoLinkedList.RemoveFirst();
        newUndoLinkedList.RemoveFirst();
    }

    public override void ClearRedo()
    {
        curRedoStack.Clear();
        newRedoStack.Clear();
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
