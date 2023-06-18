using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Item : EditorBase
{
    public Sprite sprite;
    CommandManager commandManager;
    AddObjectCommand addObject;

    public void Initialize(CommandManager commandManager, Transform parent, Transform spawnPivot)
    {
        this.commandManager = commandManager;
        addObject = GetComponent<AddObjectCommand>();
        addObject.addToUndo = true;
        addObject.SetParent(parent);
        addObject.SetSpawnPivot(spawnPivot);
    }

    public void AddItemToScene()
    {
        commandManager.ExecuteCommand(addObject);
    }
}
