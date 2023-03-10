using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectList : AbstractGameEditor
{
    /// <summary>
    /// Date: 03/08/23, By: Yvar
    /// This class keeps track of the created objects, it's gameobject is the parent to all the objects that the editor
    /// adds to their puzzle. Also a set of objects that where just created are in here to keep track of their undo/redo action
    /// 
    /// ONLY ONE INSTANCE OF THIS CLASS SHOULD EXIST
    /// This linked list is static because if there only exists one instance of it which can only
    /// consist of a max amount of elements instead of multiple per command
    /// 
    /// Update: 03/09/23
    /// I have done something to store this in two places... or something like that... bit messy
    /// </summary>

    public static LinkedList<GameObject[]> allGOList = new LinkedList<GameObject[]>();

    public override void EditorUpdate()
    {
    }
}
