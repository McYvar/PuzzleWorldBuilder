using System.Collections.Generic;
using UnityEngine;

public class CommandManager
{
    private List<ICommand> undoListStack = new List<ICommand>();
    private Stack<ICommand> redoStack = new Stack<ICommand>();
    private int maxUndoAmount;
    public static int globalMaxUndoAmount = 0;

    public CommandManager(int maxUndoAmount)
    {
        this.maxUndoAmount = maxUndoAmount;
        globalMaxUndoAmount = maxUndoAmount;
    }

    public void ExecuteCommand(ICommand command)
    {
        command.ClearRedo();
        command.Execute();
        if (command.addToUndo)
        {
            Push(command);
            redoStack.Clear();
        }
        //Debug.Log("Executed: " + command.GetType());

        if (undoListStack.Count > maxUndoAmount)
        {
            RemoveAtBottom();
        }

        // update save state since a change was made
        DataPersistenceManager.instance.SetSavedState(false);
    }

    public void UndoCommand()
    {
        if (undoListStack.Count > 0)
        {
            ICommand command = Pop();
            command.Undo();
            redoStack.Push(command);
            //Debug.Log("Undo: " + command.GetType());
        }
    }

    public void RedoCommand()
    {
        if (redoStack.Count > 0)
        {
            ICommand command = redoStack.Pop();
            command.Redo();
            Push(command);
            //Debug.Log("Redo: " + command.GetType());
        }
    }

    public void Push(ICommand item)
    {
        undoListStack.Add(item);
    }

    public ICommand Pop()
    {
        if (undoListStack.Count > 0)
        {
            ICommand temp = undoListStack[undoListStack.Count - 1];
            undoListStack.RemoveAt(undoListStack.Count - 1);
            return temp;
        }
        return null;
    }

    public void RemoveAtBottom()
    {
        undoListStack[0].ClearFirstUndo();
        undoListStack.RemoveAt(0);
    }

    public void RemoveAtTop()
    {
        undoListStack.RemoveAt(undoListStack.Count - 1);
    }

    public void ClearAll()
    {
        foreach (ICommand command in undoListStack)
        {
            command.ClearFirstUndo();
        }
        undoListStack.Clear();

        while (redoStack.Count > 0)
        {
            redoStack.Pop().ClearRedo();
        }
    }
}

