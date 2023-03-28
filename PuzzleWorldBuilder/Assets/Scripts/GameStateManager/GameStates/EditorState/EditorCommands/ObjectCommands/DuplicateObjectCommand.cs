using System.Collections.Generic;

public class DuplicateObjectCommand : BaseObjectCommands
{
    /// <summary>
    /// Date: 28/03/23, By: Yvar Toorop
    /// Essentially what this class does is duplicate the selected objects and
    /// overrides the selection to the new objects.
    /// </summary>
    LinkedList<SceneObject[]> undoLinkedList;
    Stack<SceneObject[]> redoStack;

    LinkedList<SceneObject[]> prevUndoLinkedList;
    Stack<SceneObject[]> prevRedoStack;

    protected override void OnEnable()
    {
        base.OnEnable();
        undoLinkedList = new LinkedList<SceneObject[]>();
        redoStack = new Stack<SceneObject[]>();
        prevUndoLinkedList = new LinkedList<SceneObject[]>();
        prevRedoStack = new Stack<SceneObject[]>();
    }

    public override void Execute()
    {
        List<SceneObject> duplicates = new List<SceneObject>();
        prevUndoLinkedList.AddLast(InputCommands.selectedObjects.ToArray());
        SceneObject[] currentlySelected = InputCommands.selectedObjects.ToArray();
        foreach (SceneObject sceneObject in currentlySelected)
        {
            SceneObject newSceneObject = CreateObject(sceneObject);
            sceneObject.OnDeselection();
            newSceneObject.OnSelection();
            duplicates.Add(newSceneObject);
        }
        undoLinkedList.AddLast(duplicates.ToArray());
    }

    public override void Undo()
    {
        SceneObject[] undoObjects = undoLinkedList.Last.Value;
        undoLinkedList.RemoveLast();
        SceneObject[] prevUndoObjects = prevUndoLinkedList.Last.Value;
        prevUndoLinkedList.RemoveLast();

        for (int i = 0; i < undoObjects.Length; i++)
        {
            undoObjects[i].OnDeselection();
            undoObjects[i].OnDeletion();
            prevUndoObjects[i].OnSelection();
        }

        redoStack.Push(undoObjects);
        prevRedoStack.Push(prevUndoObjects);
    }

    public override void Redo()
    {
        SceneObject[] redoObjects = redoStack.Pop();
        SceneObject[] prevRedoObjects = prevRedoStack.Pop();
        for (int i = 0; i < redoObjects.Length; i++)
        {
            prevRedoObjects[i].OnDeselection();
            redoObjects[i].OnCreation();
            redoObjects[i].OnSelection();
        }
        undoLinkedList.AddLast(redoObjects);
        prevUndoLinkedList.AddLast(prevRedoObjects);
    }

    public override void ClearFirstUndo()
    {
        undoLinkedList.RemoveFirst();
        prevUndoLinkedList.RemoveFirst();
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

        prevRedoStack.Clear();
    }
}
