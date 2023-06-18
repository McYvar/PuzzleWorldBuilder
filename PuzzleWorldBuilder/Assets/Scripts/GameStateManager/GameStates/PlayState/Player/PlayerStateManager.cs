using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateManager : RunnerBase
{
    /// <summary>
    /// Date: 04/02/23, By: Yvar Toorop
    /// VERY EXPERIMENTAL BUT BEST PLAYER CONTROLLER I'VE MADE YET
    /// A while back, I made a player controller that uses a state manager to handle movement.
    /// Now I do the same, however, it's much cleaner and also takes object movement into account.
    /// For example, if you are on a moving and spinning platform, then you move along with it.
    /// When you jump, the deltamove that was applied to you when standing on the platform, gets
    /// converted to a velocity vector by dividing it trough delta time. Then dis velocity vector
    /// gets added to the actual velocity. 
    /// Also, the downward force on jumping imidiately after landing is calculated and subtracted
    /// from the current velocity. This way, when actually jumping, you don't weirdly slow down
    /// on some arbitrary jump attempt.
    /// Furthermore, probs more addons, but so far, all of this also works in every direction.
    /// Say you want you character to walk on the roof, and all things still function very well.
    /// </summary>
    [SerializeField] BaseState startState;
    FiniteStateMachine fsm;

    private void Start()
    {
        BaseState[] states = GetComponents<BaseState>();
        fsm = new FiniteStateMachine(startState.GetType(), states);
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        fsm?.OnFixedUpdate();
    }

    public override void OnLateUpdate()
    {
        base.OnLateUpdate();
        fsm?.OnLateUpdate();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        fsm?.OnUpdate();
    }

    public void SwitchState(System.Type state)
    {
        fsm?.SwitchState(state);
    }

    public void OnStopPlayMode()
    {
        fsm.SwitchState(typeof(IdleState));
    }
}
