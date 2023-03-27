using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveSelectionObjectCommand : BaseObjectCommands
{
    LinkedList<SceneObject[]> undoLinkedList;
    Stack<SceneObject[]> redoStack;
    List<SceneObject> preSelected = new List<SceneObject>();
    List<SceneObject> currentlySelected = new List<SceneObject>();

    protected override void OnEnable()
    {
        base.OnEnable();
        undoLinkedList = new LinkedList<SceneObject[]>();
        redoStack = new Stack<SceneObject[]>();
    }

    public override void Execute()
    {
        List<SceneObject> deselected = new List<SceneObject>();
        foreach (SceneObject sceneObject in preSelected)
        {
            if (currentlySelected.Contains(sceneObject))
            {
                sceneObject.OnDeselection();
                deselected.Add(sceneObject);
                Debug.Log(sceneObject.name);
            }
        }
        undoLinkedList.AddLast(deselected.ToArray());
    }

    public override void Undo()
    {
        SceneObject[] undoObjects = undoLinkedList.Last.Value;
        undoLinkedList.RemoveLast();
        foreach (SceneObject sceneObject in undoObjects)
        {
            sceneObject.OnSelection();
        }
        redoStack.Push(undoObjects);
    }

    public override void Redo()
    {
        SceneObject[] redoObjects = redoStack.Pop();
        foreach (SceneObject sceneObject in redoObjects)
        {
            sceneObject.OnDeselection();
        }
        undoLinkedList.AddLast(redoObjects);
    }

    public override void ClearFirstUndo()
    {
        undoLinkedList.RemoveFirst();
    }

    public override void ClearRedo()
    {
        redoStack.Clear();
    }

    public void InitializePreSelected(List<SceneObject> currentlySelected, List<SceneObject> preSelected)
    {
        this.preSelected.Clear();
        this.currentlySelected.Clear();

        this.currentlySelected.AddRange(currentlySelected);
        this.preSelected.AddRange(preSelected);
    }
}
