using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutObjectCommand : BaseObjectCommands
{
    /// <summary>
    /// Date: 03/10/2023, By: Yvar
    /// Essentially cutting an object places it on the clipboard.
    /// Undoing this command should result on the item clipped to the clipboard still be on the clipboard.
    /// Then al redo does is delete the item that was intially cut.
    /// </summary>
    
    [SerializeField] CopyObjectCommand copyObjectCommand;
    [SerializeField] DeleteObjectCommand deleteObjectCommand;

    public override void Execute()
    {
        copyObjectCommand.Execute();
        deleteObjectCommand.Execute();
        Debug.Log("Copied objects to clipboard!");
    }

    public override void Undo()
    {
        deleteObjectCommand.Undo();
    }

    public override void Redo()
    {
        deleteObjectCommand.Redo();
    }
}
