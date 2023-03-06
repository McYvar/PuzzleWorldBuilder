using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddObjectCommand : MonoBehaviour, ICommand
{
    // now a key, later a drag and drop
    [SerializeField] KeyCode addObjectKey;
    [SerializeField] GameObject myObject;
    GameObject myCopyObject;

    public void OnEnable() => InputCommands.AddKeyCommand(addObjectKey, this);
    public void OnDisable() => InputCommands.RemoveCommand(addObjectKey);

    public void Execute()
    {
        myCopyObject = Instantiate(myObject, Vector3.zero, Quaternion.identity);
    }

    public void Undo()
    {
        Destroy(myCopyObject);
    }
}
