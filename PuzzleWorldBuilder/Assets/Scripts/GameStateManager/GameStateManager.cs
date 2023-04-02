using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    [SerializeField] BaseState startState;
    FiniteStateMachine fsm;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        BaseState[] states = GetComponents<BaseState>();
        fsm = new FiniteStateMachine(startState.GetType(), states);
    }

    private void Start()
    {
        fsm?.OnStart();
    }

    private void Update()
    {
        fsm?.OnUpdate();
    }

    private void FixedUpdate()
    {
        fsm?.OnFixedUpdate();
    }

    private void LateUpdate()
    {
        fsm?.OnLateUpdate();
    }

    public void SwitchState(System.Type state)
    {
        fsm?.SwitchState(state);
    }
}
