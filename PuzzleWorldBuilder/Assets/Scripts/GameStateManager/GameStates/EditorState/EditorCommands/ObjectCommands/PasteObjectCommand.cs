using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PasteObjectCommand : BaseObjectCommands
{
    /// <summary>
    /// Date: 03/10/2023, By: Yvar
    /// The paste command pastes the available items on the "clipboard".
    /// </summary>
    /// 

    LinkedList<SceneObject[]> undoLinkedList;
    Stack<SceneObject[]> redoStack;

    protected override void OnEnable()
    {
        base.OnEnable();
        undoLinkedList = new LinkedList<SceneObject[]>();
        redoStack = new Stack<SceneObject[]>();
    }

    public override void Execute()
    {
        List<SceneObject> newObj = new List<SceneObject>();
        foreach (ClipBoard item in ClipBoard.clipboard)
        {
            SceneObject createdObject = CreateObject(item.GetComponent<SceneObject>());
            createdObject.name = item.normalName;
            newObj.Add(createdObject);
        }

        foreach (SceneObject sceneObject in newObj)
        {
            ClipBoard clipBoard = sceneObject.GetComponent<ClipBoard>();
            if (clipBoard != null) Destroy(clipBoard);
        }

        undoLinkedList.AddLast(newObj.ToArray());
    }

    public override void Undo()
    {
        SceneObject[] undoObjects = undoLinkedList.Last.Value;
        undoLinkedList.RemoveLast();
        foreach (SceneObject sceneObject in undoObjects)
        {
            sceneObject.OnDeletion();
        }
        redoStack.Push(undoObjects);
    }

    public override void Redo()
    {
        SceneObject[] redoObjects = redoStack.Pop();
        foreach (SceneObject sceneObject in redoObjects)
        {
            sceneObject.OnCreation();
        }
        undoLinkedList.AddLast(redoObjects);
    }

    public override void ClearFirstUndo()
    {
        undoLinkedList.RemoveFirst();
    }

    public override void ClearRedo()
    {
        while (redoStack.Count > 0)
        {
            foreach (SceneObject sceneObject in redoStack.Pop())
            {
                Destroy(sceneObject.gameObject);
            }
        }
    }
}
