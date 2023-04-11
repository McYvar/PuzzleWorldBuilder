using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirMovementState : MoveState
{
    [SerializeField] float airMoveForce;
    float jumpDelay;

    public override void OnEnter()
    {
        base.OnEnter();
        jumpDelay = 0;
        rb.velocity += groundObjectVelocity;
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        Vector3 resultMove = head.forward * verticalInput +
            head.right * horizontalInput;

        rb.AddForce(resultMove.normalized * airMoveForce);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (jumpDelay < 0.1f) 
        {
            jumpDelay += Time.deltaTime;
            return;
        }
        if (isGrounded) stateManager.SwitchState(typeof(GroundMovementState));
    }
}
