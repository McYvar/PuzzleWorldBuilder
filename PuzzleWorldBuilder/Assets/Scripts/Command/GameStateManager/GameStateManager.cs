using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    FiniteStateMachine fsm;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        BaseState[] states = GetComponents<BaseState>();
        fsm = new FiniteStateMachine(typeof(EditorState), states);
    }

    private void Update()
    {
        if (fsm != null) fsm.OnUpdate();
    }

    private void FixedUpdate()
    {
        if (fsm != null) fsm.OnFixedUpdate();
    }

    public void SwitchState(System.Type state)
    {
        fsm.SwitchState(state);
    }
}
