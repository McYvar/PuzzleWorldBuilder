using UnityEngine;

public class ExampleCommand : AbstractGameEditor, ICommand
{
    [SerializeField] KeyCode exampleKey;

    [SerializeField] bool addToUndoSystem = true;
    public bool addToUndo { get; set; }

    protected override void OnEnable()
    {
        base.OnEnable();
        addToUndo = addToUndoSystem;
        InputCommands.AddKeyCommand(exampleKey, this);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        InputCommands.RemoveKeyCommand(exampleKey);
    }

    public void Execute()
    {
        Debug.Log("Execute!");
        transform.position += Vector3.right;
    }
    public void Undo()
    {
        Debug.Log("Undo!");
        transform.position -= Vector3.right;
    }

    public void Redo()
    {
        Execute();
    }

    public override void EditorUpdate()
    {
    }
}