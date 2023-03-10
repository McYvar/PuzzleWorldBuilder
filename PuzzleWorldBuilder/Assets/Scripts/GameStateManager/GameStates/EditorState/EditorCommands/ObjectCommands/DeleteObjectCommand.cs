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
    Stack<SceneObject[]> previouslySelectedObjects;
    [SerializeField] DeSelectObjectCommand deSelectCommand;
    [SerializeField] SelectObjectCommand selectCommand;

    protected override void OnEnable()
    {
        base.OnEnable();
        redoObjectsStack = new Stack<GameObject[]>();
        previouslySelectedObjects = new Stack<SceneObject[]>();
    }

    public override void Execute()
    {
        // When we delete an object in the puzzlemaker that is selected, we should also remove it from the selected objects list
        if (InputCommands.selectedObjects.Count > 0)
        {
            if (InputCommands.selectedObjects[0] != null)
            {
                if (!InputCommands.selectedObjects[0].IsAlive())
                {
                    Debug.Log("Nothing to delete!");
                    addToUndo = false;
                    return;
                }
            }
        }
        else
        {
            Debug.Log("Nothing to delete!");
            addToUndo = false;
            return;
        }

        addToUndo = true;
        GameObject[] selectedObjects = new GameObject[InputCommands.selectedObjects.Count];
        // For an object to be removed it first needs to be selected, if none is selected, we add a null object to the undo list
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            selectedObjects[i] = InputCommands.selectedObjects[i].gameObject;
        }
        previouslySelectedObjects.Push(InputCommands.selectedObjects.ToArray());
        deSelectCommand.Execute();

        foreach (GameObject obj in selectedObjects)
        {
            if (obj != null) DeleteObject(obj);
        }

        AddObjectToLinkedList(selectedObjects);
        redoObjectsStack.Clear();
    }

    public override void Undo()
    {
        InputCommands.selectedObjects = new List<SceneObject>(previouslySelectedObjects.Pop());
        selectCommand.Execute();

        GameObject[] undoObjects = ObjectList.allGOList.Last.Value;
        ObjectList.allGOList.RemoveLast();
        foreach(GameObject obj in undoObjects)
        {
            obj.GetComponent<SceneObject>().OnCreation();
        }
        redoObjectsStack.Push(undoObjects);
    }

    public override void Redo()
    {
        previouslySelectedObjects.Push(InputCommands.selectedObjects.ToArray());
        deSelectCommand.Execute();

        GameObject[] redoObjects = redoObjectsStack.Pop();
        foreach(GameObject obj in redoObjects)
        {
            if (obj != null) DeleteObject(obj);
        }
        AddObjectToLinkedList(redoObjects);
    }
}
