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
    }

    public override void OnFixedUpdate()
    {
        Vector3 resultMove = transform.forward * verticalInput * airMoveForce +
            transform.right * horizontalInput * airMoveForce;

        rb.AddForce(resultMove);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (jumpDelay < 0.1f) 
        {
            jumpDelay += Time.deltaTime;
            return;
        }

        Ray ray = new Ray(transform.position, -transform.up);
        float sphereRadius = 0.4f;
        if (Physics.SphereCast(ray, sphereRadius, transform.localScale.y - (sphereRadius / 2) + 0.01f))
        {
            stateManager.SwitchState(typeof(GroundMovementState));
        }
    }
}
