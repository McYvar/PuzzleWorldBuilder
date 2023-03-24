using System.Collections;
using UnityEngine;

public class CopyObjectCommand : BaseObjectCommands
{
    /// <summary>
    /// Date: 03/10/2023, By: Yvar
    /// The copy command handles the placement of objects into a "clipboard".
    /// We copy the elements that are selected in the puzzle builder.
    /// I updated the main ICommand interface and BaseCommand class to ask for a boolean that,
    /// depending on it's value, adds the command to the undo system of the commandmanager.
    /// This is because I want to execute the copy command, but don't add it to the undo system,
    /// thats also why this class only overrides the execute command from the base class.
    /// </summary>

    public override void Execute()
    {
        addToUndo = false;
        while (ClipBoard.clipboard.Count > 0)
        {
            Destroy(ClipBoard.clipboard[0].gameObject);
        }

        foreach (TerrainObject obj in InputCommands.selectedObjects)
        {
            CreateInvisible(obj).gameObject.AddComponent<ClipBoard>().AddToClipBoard();
        }
    }
}
