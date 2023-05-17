using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    [SerializeField] BaseState startState;
    FiniteStateMachine fsm;

    public static GameStateManager Instance;

    private void Start()
    {
        Instance = this;
        DontDestroyOnLoad(this);
        BaseState[] states = GetComponents<BaseState>();
        fsm = new FiniteStateMachine(startState.GetType(), states);
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
