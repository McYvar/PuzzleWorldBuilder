using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractGameRunner : MonoBehaviour
{
    private void OnEnable() => PlayState.newRunnesQueue.Enqueue(this);
    private void OnDisable() => PlayState.removeRunnesQueue.Enqueue(this);
    public abstract void RunnerAwake();
    public abstract void RunnerStart();
    public abstract void RunnerUpdate();
    public abstract void RunnerFixedUpdate();
    public abstract void RunnerLateUpdate();
}
