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

    List<SceneObject> currentlySelected = new List<SceneObject>();
    List<SceneObject> nextSelected = new List<SceneObject>();

    LinkedList<SceneObject[]> curUndoLinkedList;
    LinkedList<SceneObject[]> nextUndoLinkedList;

    Stack<SceneObject[]> nextRedoStack;
    Stack<SceneObject[]> currentRedoStack;

    protected override void OnEnable()
    {
        base.OnEnable();
        curUndoLinkedList = new LinkedList<SceneObject[]>();
        nextUndoLinkedList = new LinkedList<SceneObject[]>();

        currentRedoStack = new Stack<SceneObject[]>();
        nextRedoStack = new Stack<SceneObject[]>();
    }

    public override void Execute()
    {
        foreach (SceneObject sceneObject in currentlySelected)
        {
            sceneObject.OnDeselection();
        }
        curUndoLinkedList.AddLast(currentlySelected.ToArray());

        foreach (SceneObject sceneObject in nextSelected)
        {
            sceneObject.OnSelection();
        }
        nextUndoLinkedList.AddLast(nextSelected.ToArray());
    }

    public override void Undo()
    {
        SceneObject[] nextSelected = nextUndoLinkedList.Last.Value;
        nextUndoLinkedList.RemoveLast();
        foreach (SceneObject sceneObject in nextSelected)
        {
            sceneObject.OnDeselection();
        }
        nextRedoStack.Push(nextSelected);

        SceneObject[] currentlySelected = curUndoLinkedList.Last.Value;
        curUndoLinkedList.RemoveLast();
        foreach (SceneObject sceneObject in currentlySelected)
        {
            sceneObject.OnSelection();
        }
        currentRedoStack.Push(currentlySelected);
    }

    public override void Redo()
    {
        SceneObject[] currentlySelected = currentRedoStack.Pop();
        foreach (SceneObject sceneObject in currentlySelected)
        {
            sceneObject.OnDeselection();
        }
        curUndoLinkedList.AddLast(currentlySelected);

        SceneObject[] nextSelected = nextRedoStack.Pop();
        foreach (SceneObject sceneObject in nextSelected)
        {
            sceneObject.OnSelection();
        }
        nextUndoLinkedList.AddLast(nextSelected);
    }

    public override void ClearFirstUndo()
    {
        curUndoLinkedList.RemoveFirst();
        nextUndoLinkedList.RemoveFirst();
    }

    public override void ClearRedo()
    {
        currentRedoStack.Clear();
        nextRedoStack.Clear();
    }

    public void InitializeSelected(List<SceneObject> currentlySelected, List<SceneObject> nextSelected)
    {
        this.currentlySelected.Clear();
        this.currentlySelected.AddRange(currentlySelected);

        this.nextSelected.Clear();
        this.nextSelected.AddRange(nextSelected);
    }
}
