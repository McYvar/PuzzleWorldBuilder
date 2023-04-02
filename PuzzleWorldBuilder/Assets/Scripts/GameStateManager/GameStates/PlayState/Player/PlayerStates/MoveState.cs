using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MoveState : BaseState
{
    protected Rigidbody rb;
    protected float horizontalInput;
    protected float verticalInput;

    protected float upInput;

    [SerializeField] Transform mainCamera;
    [SerializeField] Transform head;

    public override void OnAwake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnStart()
    {
    }

    public override void OnEnter()
    {
        rb.useGravity = true;
        //Physics.gravity = -transform.up * 9.81f;
    }

    public override void OnExit()
    {
    }

    public override void OnFixedUpdate()
    {
    }

    public override void OnUpdate()
    {
        horizontalInput = 0;
        if (Input.GetKey(KeyCode.D)) horizontalInput += 1;
        if (Input.GetKey(KeyCode.A)) horizontalInput -= 1;

        verticalInput = 0;
        if (Input.GetKey(KeyCode.W)) verticalInput += 1;
        if (Input.GetKey(KeyCode.S)) verticalInput -= 1;
    }

    public override void OnLateUpdate()
    {
        // replace w/ cam movement
        mainCamera.position = head.position;
        mainCamera.rotation = transform.rotation;
    }
}
