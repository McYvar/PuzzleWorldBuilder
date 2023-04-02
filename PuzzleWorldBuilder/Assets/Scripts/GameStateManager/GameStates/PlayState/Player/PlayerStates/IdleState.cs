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
    [SerializeField, Range(0f, 1f)] float smoothTime;

    Vector3 pivotRef = Vector3.zero;
    Vector3 camRef = Vector3.zero;
    
    Vector3 startPos;
    Quaternion cameraStartRot;
    Quaternion pivotStartRot;

    public override void OnAwake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnStart()
    {
    }

    public override void OnEnter()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false;

        startPos = mainCamera.position;
        cameraStartRot = mainCamera.rotation;
        pivotStartRot = mainCameraPivot.rotation;
    }

    public override void OnExit()
    {
    }

    public override void OnFixedUpdate()
    {
    }

    public override void OnUpdate()
    {
        mainCameraPivot.position =
            Vector3.SmoothDamp(mainCameraPivot.position,
            head.position,
            ref camRef,
            smoothTime);

        mainCamera.position =
            Vector3.SmoothDamp(mainCamera.position,
            mainCameraPivot.position,
            ref pivotRef,
            smoothTime);

        float lerpValue = Mathf.InverseLerp((head.position - startPos).magnitude, 0, (head.position - mainCamera.position).magnitude);
        Debug.Log(lerpValue);

        mainCameraPivot.rotation =
            Quaternion.Slerp(pivotStartRot,
            Quaternion.Euler(0, 0, 0),
            lerpValue);

        mainCamera.rotation =
            Quaternion.Slerp(cameraStartRot,
            transform.rotation,
            lerpValue);
        if (Vector3.Distance(head.position, mainCameraPivot.position) < 0.1f &&
            Vector3.Distance(mainCamera.localPosition, Vector3.zero) < 0.1f)
        {
            SwitchToMoveState();
        }
    }

    public void SwitchToMoveState()
    {
        stateManager.SwitchState(typeof(GroundMovementState));
    }

    public override void OnLateUpdate()
    {
    }
}
