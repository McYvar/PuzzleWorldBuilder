using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputCommands : MonoBehaviour
{
    [SerializeField] int maxUndoAmount = 10;
    private CommandManager commandManager;
    public static Dictionary<KeyCode, ICommand> keyCommands = new Dictionary<KeyCode, ICommand>();

    private void Start()
    {
        commandManager = new CommandManager(maxUndoAmount);
    }

    private void Update()
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
            commandManager.UndoCommand();
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            commandManager.RedoCommand();
        }
#else
        if (Input.GetKeyDown(KeyCode.Z) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            commandManager.UndoCommand();
        }
        else if (Input.GetKeyDown(KeyCode.Y) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            commandManager.RedoCommand();
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
}