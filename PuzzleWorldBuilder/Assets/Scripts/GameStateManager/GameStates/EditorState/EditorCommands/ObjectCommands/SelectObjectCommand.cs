using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectObjectCommand : BaseObjectCommands
{
    LinkedList<SceneObject[]> undoLinkedList;
    Stack<SceneObject[]> redoStack;
    List<SceneObject> preSelected = new List<SceneObject>();

    protected override void OnEnable()
    {
        base.OnEnable();
        undoLinkedList = new LinkedList<SceneObject[]>();
        redoStack = new Stack<SceneObject[]>();
    }
    
    public override void Execute()
    {
        foreach (SceneObject sceneObject in preSelected)
        {
            sceneObject.OnSelection();
        }
        undoLinkedList.AddLast(preSelected.ToArray());
    }

    public override void Undo()
    {
        SceneObject[] redoObjects = undoLinkedList.Last.Value;
        undoLinkedList.RemoveLast();
        foreach (SceneObject sceneObject in redoObjects)
        {
            sceneObject.OnDeselection();
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

    public int InitializePreSelected(List<SceneObject> currentlySelected, List<SceneObject> preSelected)
    {
        this.preSelected.Clear();
        this.preSelected.AddRange(preSelected);

        foreach(SceneObject sceneObject in currentlySelected)
        {
            if (this.preSelected.Contains(sceneObject))
                this.preSelected.Remove(sceneObject);
        }
        return this.preSelected.Count;
    }
}
