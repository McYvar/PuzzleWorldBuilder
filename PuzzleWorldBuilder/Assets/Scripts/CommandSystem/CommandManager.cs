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
        Push(command);
        command.Execute();
        redoStack.Clear();
        Debug.Log("Executed: " + command.GetType());

        if (undoListStack.Count > maxUndoAmount)
        {
            RemoveAtBottom();
        }
    }

    public void UndoCommand()
    {
        if (undoListStack.Count > 0)
        {
            ICommand command = Pop();
            command.Undo();
            redoStack.Push(command);
            Debug.Log("Undo: " + command.GetType());
        }
        else
        {
            Debug.Log("Nothing left to undo!");
        }
    }

    public void RedoCommand()
    {
        if (redoStack.Count > 0)
        {
            ICommand command = redoStack.Pop();
            command.Redo();
            Push(command);
            Debug.Log("Redo: " + command.GetType());
        }
        else
        {
            Debug.Log("Nothing left to redo!");
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
        undoListStack.RemoveAt(0);
    }

    public void RemoveAtTop()
    {
        undoListStack.RemoveAt(undoListStack.Count - 1);
    }
}

