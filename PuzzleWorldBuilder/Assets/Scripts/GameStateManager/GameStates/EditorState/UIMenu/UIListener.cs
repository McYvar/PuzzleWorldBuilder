using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIListener : MonoBehaviour
{
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
