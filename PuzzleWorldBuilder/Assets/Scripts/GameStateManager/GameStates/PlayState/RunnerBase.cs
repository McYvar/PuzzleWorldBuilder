using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunnerBase : BaseState
{
    private void OnEnable() => PlayState.newRunnesQueue.Enqueue(this);
    private void OnDisable() => PlayState.removeRunnesQueue.Enqueue(this);

    public override void OnEnter() { }

    public override void OnExit() { }

    public override void OnUpdate() { }

    public override void OnFixedUpdate() { }

    public override void OnLateUpdate() { }
}
