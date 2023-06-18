using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundMovementState : MoveState
{
    [SerializeField] float groundMoveForce;
    [SerializeField] float jumpForce;

    Vector3 deltaPos;
    float recoveryTimer;
    [SerializeField] float maxRecoveryTime;

    public override void OnEnter()
    {
        base.OnEnter();
        isGrounded = true;
        recoveryTimer = 0;
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        Vector3 resultMove = head.forward * verticalInput + 
            head.right * horizontalInput;

        rb.AddForce(resultMove.normalized * groundMoveForce);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (recoveryTimer < maxRecoveryTime)
        {
            recoveryTimer += Time.deltaTime;
            return;
        }

        if (!isGrounded)
        {
            stateManager.SwitchState(typeof(AirMovementState));
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space)) Jump();
    }

    public override void OnLateUpdate()
    {
        base.OnLateUpdate();
        if (platform != null)
        {
            FollowPlatform();
        }
        else
        {
            deltaPos = Vector3.zero;
        }
    }

    void Jump()
    {
        float nearVectorLenght = rb.velocity.magnitude * 
            Mathf.Cos(Vector3.Angle(-transform.up, rb.velocity) * Mathf.Deg2Rad);
        rb.velocity += transform.up * nearVectorLenght;
        rb.AddForce(transform.up * jumpForce, ForceMode.VelocityChange);
        groundObjectVelocity = deltaPos / Time.deltaTime;
        stateManager.SwitchState(typeof(AirMovementState));
    }

    void FollowPlatform()
    {
        transform.eulerAngles += platform.deltaEuler;
        transform.position += platform.deltaPos;

        // displacement is based of the centre point from the object the player is standing on
        Vector3 playerToGround = transform.position - platform.transform.position;
        Vector3 playerToGroundRotated = Quaternion.Euler(platform.deltaEuler) * playerToGround;
        Vector3 resultRotatedPos = playerToGroundRotated - playerToGround;
        transform.position += resultRotatedPos;

        deltaPos = platform.deltaPos + resultRotatedPos;
    }
}
