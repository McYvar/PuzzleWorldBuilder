using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIListener : MonoBehaviour
{
    /// <summary>
    /// Date: 03/31/23, By: Yvar Toorop
    /// This little class its purpose is to bind a method to the action listener below.
    /// The reason for this is, if the user is prompted to save because of a destructive action while not having saved yet,
    /// then I only need to create one UI element, to which their buttons have different behaviour depending on the methods
    /// that are added to this listener. Once the action is invoked or canceled, the listener is cleared.
    /// </summary>
    public static Action listener;

    public void InvokeListener()
    {
        listener?.Invoke();
        ClearListener();
    }

    public void ClearListener()
    {
        listener = null;
    }
}
