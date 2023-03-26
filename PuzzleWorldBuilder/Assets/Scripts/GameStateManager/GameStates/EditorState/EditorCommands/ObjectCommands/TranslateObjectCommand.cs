using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranslateObjectCommand : BaseObjectCommands
{
    Vector3 positionToTranslate = Vector3.zero;
    LinkedList<Vector3> undoLinkedList;
    Stack<Vector3> redoStack;

    protected override void OnEnable()
    {
        base.OnEnable();
        undoLinkedList = new LinkedList<Vector3>();
        redoStack = new Stack<Vector3>();
    }

    public override void Execute()
    {
        // at this point the objects are moved already so I don't have to translate them
        undoLinkedList.AddLast(positionToTranslate);
    }

    public override void Undo()
    {
        // but in the undo I do
        Vector3 undoPosition = undoLinkedList.Last.Value;
        undoLinkedList.RemoveLast();
        foreach (SceneObject sceneObject in InputCommands.selectedObjects)
        {
            sceneObject.myStartPos = sceneObject.transform.position;
            sceneObject.MoveTo(-undoPosition);
        }
        redoStack.Push(undoPosition);
    }

    public override void Redo()
    {
        // and in the redo aswell
        Vector3 redoPosition = redoStack.Pop();
        foreach (SceneObject sceneObject in InputCommands.selectedObjects)
        {
            sceneObject.myStartPos = sceneObject.transform.position;
            sceneObject.MoveTo(redoPosition);
        }
        undoLinkedList.AddLast(redoPosition);
    }

    public override void ClearFirstUndo()
    {
        undoLinkedList.RemoveFirst();
    }

    public override void ClearRedo()
    {
        redoStack.Clear();
    }

    public void InitializeNewPostition(Vector3 newPostition)
    {
        positionToTranslate = newPostition;
    }
}
