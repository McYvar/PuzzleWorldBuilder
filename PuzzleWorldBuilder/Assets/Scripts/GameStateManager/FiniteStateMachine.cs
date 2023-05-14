using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiniteStateMachine
{
    // Finite state machine based on in lesson given instructions
    private Dictionary<System.Type, BaseState> stateDictionary = new Dictionary<System.Type, BaseState>();

    private BaseState currentState;

    public FiniteStateMachine(System.Type startState, BaseState[] states)
    {
        foreach(BaseState state in states)
        {
            state.Initialize(this);
            stateDictionary.Add(state.GetType(), state);
        }
        SwitchState(startState);
    }

    public void OnUpdate()
    {
        currentState?.OnUpdate();
    }

    public void OnFixedUpdate()
    {
        currentState?.OnFixedUpdate();
    }

    public void OnLateUpdate()
    {
        currentState?.OnLateUpdate();
    }

    public void SwitchState(System.Type newStateStype)
    {
        currentState?.OnExit();
        currentState = stateDictionary[newStateStype];
        currentState?.OnEnter();
    }

    public void DebugCurrentState()
    {
        Debug.Log(currentState);
    }
}
