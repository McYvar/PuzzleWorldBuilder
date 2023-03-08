using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCommand : EditorState, ICommand
{
    [SerializeField] KeyCode baseKey;

    public virtual void OnEnable() => InputCommands.AddKeyCommand(baseKey, this);
    public virtual void OnDisable() => InputCommands.RemoveCommand(baseKey);

    public virtual void Execute()
    {
    }

    public virtual void Undo()
    {
    }

    public virtual void Redo()
    {
    }
}
