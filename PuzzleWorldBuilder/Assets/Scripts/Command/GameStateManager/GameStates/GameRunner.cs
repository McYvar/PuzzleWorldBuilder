using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameRunner : MonoBehaviour
{
    private void OnEnable() => PlayState.runners.Add(this);
    private void OnDisable() => PlayState.runners.Remove(this);
    public abstract void RunnerUpdate();
    public abstract void RunnerFixedUpdate();
}
