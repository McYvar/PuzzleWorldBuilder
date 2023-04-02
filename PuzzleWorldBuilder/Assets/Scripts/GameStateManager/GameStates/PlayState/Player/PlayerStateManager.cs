using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateManager : AbstractGameRunner
{
    [SerializeField] BaseState startState;
    FiniteStateMachine fsm;

    public override void RunnerAwake() 
    { 
        BaseState[] states = GetComponents<BaseState>();
        fsm = new FiniteStateMachine(startState.GetType(), states);
    }

    public override void RunnerStart() 
    {
        fsm?.OnStart();
    }

    public override void RunnerFixedUpdate()
    {
        fsm?.OnFixedUpdate();
    }

    public override void RunnerLateUpdate()
    {
        fsm?.OnLateUpdate();
    }

    public override void RunnerUpdate()
    {
        fsm?.OnUpdate();
    }

    public void SwitchState(System.Type state)
    {
        fsm?.SwitchState(state);
    }
}
