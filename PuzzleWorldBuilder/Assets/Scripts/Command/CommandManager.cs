using System.Collections.Generic;
using UnityEngine;

public class CommandManager
{
    private List<ICommand> commandListStack = new List<ICommand>();
    private Stack<ICommand> redoStack = new Stack<ICommand>();
    private int maxUndoAmount;

    public CommandManager(int maxUndoAmount)
    {
        this.maxUndoAmount = maxUndoAmount;
    }

    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        Push(command);
        redoStack.Clear();

        if (commandListStack.Count > maxUndoAmount)
        {
            RemoveAtBottom();
        }
    }

    public void UndoCommand()
    {
        if (commandListStack.Count > 0)
        {
            ICommand command = Pop();
            command.Undo();
            redoStack.Push(command);
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
            command.Execute();
            Push(command);
        }
        else
        {
            Debug.Log("Nothing left to redo!");
        }
    }

    public void Push(ICommand item)
    {
        commandListStack.Add(item);
    }

    public ICommand Pop()
    {
        if (commandListStack.Count > 0)
        {
            ICommand temp = commandListStack[commandListStack.Count - 1];
            commandListStack.RemoveAt(commandListStack.Count - 1);
            return temp;
        }
        return null;
    }

    public void RemoveAtBottom()
    {
        commandListStack.RemoveAt(0);
    }
}

