using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameEditor : MonoBehaviour
{
    private void OnEnable() => EditorState.editors.Add(this);
    private void OnDisable() => EditorState.editors.Remove(this);

    public abstract void EditorUpdate();
}
