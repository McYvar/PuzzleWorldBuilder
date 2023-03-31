using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Item : AbstractGameEditor
{
    public Sprite sprite;
    CommandManager commandManager;
    AddObjectCommand addObject;

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    public void Initialize(CommandManager commandManager, Transform parent)
    {
        this.commandManager = commandManager;
        addObject = GetComponent<AddObjectCommand>();
        addObject.addToUndo = true;
        addObject.SetParent(parent);
    }

    public void AddItemToScene()
    {
        commandManager.ExecuteCommand(addObject);
    }

    public override void EditorAwake()
    {
    }

    public override void EditorStart()
    {
    }

    public override void EditorUpdate()
    {
    }
}
