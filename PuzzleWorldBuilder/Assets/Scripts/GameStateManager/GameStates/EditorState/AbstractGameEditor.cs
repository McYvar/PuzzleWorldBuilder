using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractGameEditor : MonoBehaviour
{
    protected virtual void OnEnable() => EditorState.newEditorsQueue.Enqueue(this);
    protected virtual void OnDisable() => EditorState.removeEditorsQueue.Enqueue(this);

    public abstract void EditorUpdate();
}
