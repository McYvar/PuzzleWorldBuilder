using UnityEngine;

public class ExampleCommand : MonoBehaviour, ICommand
{
    [SerializeField] KeyCode exampleKey;

    public void OnEnable() => InputCommands.AddKeyCommand(exampleKey, this);
    public void OnDisable() => InputCommands.RemoveCommand(exampleKey);

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
}