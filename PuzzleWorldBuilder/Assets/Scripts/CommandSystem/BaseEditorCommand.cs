using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEditorCommand : AbstractGameEditor, ICommand
{
    [SerializeField] KeyCode baseKey;
    [SerializeField] protected InputCommands inputCommands;

    protected override void OnEnable()
    {
        base.OnEnable();
        InputCommands.AddKeyCommand(baseKey, this);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        InputCommands.RemoveKeyCommand(baseKey);
    }

    public virtual void Execute()
    {
    }

    public virtual void Undo()
    {
    }

    public virtual void Redo()
    {
    }

    public override void EditorUpdate()
    {
    }
}
