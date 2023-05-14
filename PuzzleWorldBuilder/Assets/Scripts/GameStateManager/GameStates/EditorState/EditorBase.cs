using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorBase : BaseState
{
    protected virtual void OnEnable() => EditorState.editorsAddQueue.Enqueue(this);
    protected virtual void OnDisable() => EditorState.editorsRemoveQueue.Enqueue(this);

    protected virtual void Start() { }

    public override void OnAwake() { }

    public override void OnStart() { }

    public override void OnEnter() { }

    public override void OnExit() { }

    public override void OnUpdate() { }

    public override void OnFixedUpdate() { }

    public override void OnLateUpdate() { }
}
