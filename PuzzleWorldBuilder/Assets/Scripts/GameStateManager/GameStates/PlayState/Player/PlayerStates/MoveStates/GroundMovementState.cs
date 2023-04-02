using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundMovementState : MoveState
{
    [SerializeField] float groundMoveForce;
    [SerializeField] float jumpForce;

    public override void OnFixedUpdate()
    {
        Vector3 resultMove = transform.forward * verticalInput * groundMoveForce + 
            transform.right * horizontalInput * groundMoveForce;

        rb.AddForce(resultMove);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (Input.GetKey(KeyCode.Space)) Jump();
    }

    void Jump()
    {
        float nearVectorLenght = rb.velocity.magnitude * 
            Mathf.Cos(Vector3.Angle(-transform.up, rb.velocity) * Mathf.Deg2Rad);
        Debug.Log(rb.velocity);
        rb.velocity += transform.up * nearVectorLenght;
        Debug.Log(rb.velocity);
        rb.AddForce(transform.up * jumpForce, ForceMode.VelocityChange);
        stateManager.SwitchState(typeof(AirMovementState));
    }
}
