using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputCommands : GameEditor
{
    /// <summary>
    /// Date: 03/08/2023, By: Yvar
    /// A Class that handles the input from the user, things such as the use of the dropdown menu's,
    /// but also like copy, paste, undo and redo using the keyboard
    /// </summary>
    [SerializeField] int maxUndoAmount = 10;
    private CommandManager commandManager;
    public static Dictionary<KeyCode, ICommand> keyCommands = new Dictionary<KeyCode, ICommand>();

    private void Start()
    {
        commandManager = new CommandManager(maxUndoAmount);
    }

    public override void EditorUpdate()
    {
        foreach (KeyCode keyCode in keyCommands.Keys)
        {
            if (Input.GetKeyDown(keyCode))
            {
                ICommand command = keyCommands[keyCode];
                commandManager.ExecuteCommand(command);
            }
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Undo();
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            Redo();
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            Copy();
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            Paste();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            Cut();
        }
#else
        if (Input.GetKeyDown(KeyCode.Z) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            Undo();
        }
        else if (Input.GetKeyDown(KeyCode.Y) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            Redo();
        }
        else if (Input.GetKeyDown(KeyCode.C) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            Copy();
        }
        else if (Input.GetKeyDown(KeyCode.V) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            Paste();
        }
        else if (Input.GetKeyDown(KeyCode.X) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            Cut();
        }
#endif
    }

    public static void AddKeyCommand(KeyCode keyCode, ICommand command)
    {
        if (!keyCommands.ContainsKey(keyCode))
        keyCommands.Add(keyCode, command);
    }

    public static void ChangeKeyCommandKey(KeyCode oldKeyCode, KeyCode newKeyCode)
    {
        ICommand temp;
        keyCommands.Remove(oldKeyCode, out temp);
        keyCommands.Add(newKeyCode, temp);
    }

    public static void RemoveCommand(KeyCode keyCode)
    {
        keyCommands.Remove(keyCode);
    }

    public void Undo()
    {
        commandManager.UndoCommand();
    }

    public void Redo()
    {
        commandManager.RedoCommand();
    }

    public void Copy()
    {
        /// Copy some object, when making a copy of an object instantiate some invisible copy of it and put in on the "clipboard".
        /// When there is an object already on the "clipboard", delte this object, and replace it with the new copy.
        /// Upon copying a bigger group of objects, I was thinking of parenting it under one gameobject so the object creation class
        /// doesn't have to be rewritten
        /// 03/08/2023 Update: did that anyway
    }

    public void Paste()
    {
        /// Paste some object, before you can paste a copy has to exist, otherwise nothing happens.
        /// When pasting a copy, the creation of this copy has to go trough the class handling the creation of objects
        /// in the level editor so the creation of it can be undone.
    }

    public void Cut()
    {
        /// Cut some object, upon cutting the object should be removed trough the class handling the deletion of objects
        /// so this action can be undone. Also when cutting, an invisible copy of this object is made and put on the "clipboard".
    }
}