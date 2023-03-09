using UnityEngine;

public class ExampleCommand : AbstractGameEditor, ICommand
{
    [SerializeField] KeyCode exampleKey;

    protected override void OnEnable()
    {
        base.OnEnable();
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