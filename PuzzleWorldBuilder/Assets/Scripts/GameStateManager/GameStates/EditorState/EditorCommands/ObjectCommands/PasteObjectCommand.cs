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

    Stack<GameObject[]> redoObjectsStack;
    Stack<GameObject[]> undoObjectsStack;

    protected override void OnEnable()
    {
        base.OnEnable();
        redoObjectsStack = new Stack<GameObject[]>();
        undoObjectsStack = new Stack<GameObject[]>();
    }

    public override void Execute()
    {
        while (redoObjectsStack.Count > 0)
        {
            foreach (GameObject obj in redoObjectsStack.Pop())
            {
                Destroy(obj);
            }
        }

        if (ClipBoard.clipboard.Count == 0)
        {
            Debug.Log("Nothing to paste");
            Debug.Log(InputCommands.selectedObjects.Count);
            addToUndo = false;
            return;
        }
        addToUndo = true;

        List<GameObject> newObj = new List<GameObject>();
        foreach (ClipBoard item in ClipBoard.clipboard)
        {
            GameObject createdObject = CreateObject(item.gameObject);
            createdObject.name = item.normalName;
            newObj.Add(createdObject);
        }

        foreach (GameObject obj in newObj)
        {
            ClipBoard clipBoard = obj.GetComponent<ClipBoard>();
            if (clipBoard != null) Destroy(clipBoard);
        }

        undoObjectsStack.Push(newObj.ToArray());
    }

    public override void Undo()
    {
        GameObject[] undoObjects = undoObjectsStack.Pop();
        foreach (GameObject obj in undoObjects)
        {
            DeleteObject(obj);
        }
        redoObjectsStack.Push(undoObjects);
    }

    public override void Redo()
    {
        GameObject[] redoObjects = redoObjectsStack.Pop();
        foreach (GameObject obj in redoObjects)
        {
            SceneObject sceneObject = obj.GetComponent<SceneObject>();
            if (sceneObject != null) sceneObject.OnCreation();
        }
        undoObjectsStack.Push(redoObjects);
    }
}
