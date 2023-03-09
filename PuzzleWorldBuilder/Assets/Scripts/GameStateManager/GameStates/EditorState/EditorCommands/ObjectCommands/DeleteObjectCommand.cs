using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteObjectCommand : BaseObjectCommands
{
    /// <summary>
    /// Date: 03/09/2023, By: Yvar
    /// A script that enherits the ICommand interface, thus making it a command.
    /// This command is responsible for savely removing objects from the puzzle scene.
    /// Only one instance of this command should exist
    /// </summary>

    Stack<GameObject[]> redoObjectsStack;

    protected override void OnEnable()
    {
        base.OnEnable();
        redoObjectsStack = new Stack<GameObject[]>();
    }

    public override void Execute()
    {
        GameObject[] selectedObjects = new GameObject[SelectObjectCommand.selectedObjects.Count];
        // For an object to be removed it first needs to be selected, if none is selected, we add a null object to the undo list
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            selectedObjects[i] = SelectObjectCommand.selectedObjects[i].gameObject;
        }

        foreach (GameObject obj in selectedObjects)
        {
            if (obj != null) DeleteObject(obj);
        }

        AddObjectToLinkedList(selectedObjects);
    }

    public override void Undo()
    {
        GameObject[] undoObjects = ObjectList.list.Last.Value;
        ObjectList.list.RemoveLast();
        foreach(GameObject obj in undoObjects)
        {
            obj.GetComponent<SceneObject>().OnCreation();
        }
        redoObjectsStack.Push(undoObjects);
    }

    public override void Redo()
    {
        GameObject[] redoObjects = redoObjectsStack.Pop();
        foreach(GameObject obj in redoObjects)
        {
            if (obj != null) DeleteObject(obj);
        }
        AddObjectToLinkedList(redoObjects);
    }
}
