using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class IdleState : BaseState
{
    Rigidbody rb;

    [SerializeField] Transform mainCameraPivot;
    [SerializeField] Transform mainCamera;
    [SerializeField] Transform head;
    
    Vector3 cameraStartPos;
    Vector3 pivotStartPos;
    Quaternion cameraStartRot;
    Quaternion pivotStartRot;

    [SerializeField] float transitionTime = 3f;
    float transitionTimer = 0;

    public override void OnAwake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnStart()
    {
    }

    public override void OnEnter()
    {
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        pivotStartPos = mainCameraPivot.position;
        cameraStartPos = mainCamera.position;
        pivotStartRot = mainCameraPivot.rotation;
        cameraStartRot = mainCamera.rotation;

        transitionTimer = transitionTime;
    }

    public override void OnExit()
    {
        rb.useGravity = true;
    }

    public override void OnFixedUpdate()
    {
    }

    public override void OnUpdate()
    {
        float lerpValue = Mathf.InverseLerp(transitionTime, 0, transitionTimer);

        mainCameraPivot.position =
            Vector3.Lerp(pivotStartPos,
            head.position,
            lerpValue);

        mainCamera.position =
            Vector3.Lerp(cameraStartPos,
            mainCameraPivot.position,
            lerpValue);

        mainCameraPivot.rotation =
            Quaternion.Slerp(pivotStartRot,
            Quaternion.Euler(0, 0, 0),
            lerpValue);

        mainCamera.rotation =
            Quaternion.Slerp(cameraStartRot,
            head.rotation,
            lerpValue);
        if (transitionTimer <= 0)
        {
            mainCameraPivot.position = head.position;
            mainCamera.position = mainCameraPivot.position;
            mainCameraPivot.rotation = Quaternion.Euler(0, 0, 0);
            mainCamera.rotation = head.rotation;

            SwitchToMoveState();
        }
        else transitionTimer -= Time.deltaTime;
    }

    public void SwitchToMoveState()
    {
        stateManager.SwitchState(typeof(GroundMovementState));
    }

    public override void OnLateUpdate()
    {
    }
}
