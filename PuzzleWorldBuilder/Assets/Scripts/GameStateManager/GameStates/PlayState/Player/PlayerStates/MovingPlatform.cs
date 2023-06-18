using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : RunnerBase
{
    [SerializeField] Vector3 moveVector;
    Vector3 lastPos;
    [HideInInspector] public Vector3 deltaPos;

    [SerializeField] Vector3 rotateVector;
    Vector3 lastEuler;
    [HideInInspector] public Vector3 deltaEuler;

    private void Start()
    {
        lastPos = transform.position;
        lastEuler = transform.eulerAngles;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        UpdateDelta();

        if (Input.GetKey(KeyCode.E))
        {
            transform.position += moveVector * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.R))
        {
            transform.eulerAngles += rotateVector * Time.deltaTime;
        }
    }

    void UpdateDelta()
    {
        deltaPos = transform.position - lastPos;
        lastPos = transform.position;

        deltaEuler = transform.eulerAngles - lastEuler;
        if (deltaEuler.x > 180) deltaEuler.x -= 360;
        if (deltaEuler.x < 180) deltaEuler.x += 360;
        if (deltaEuler.y > 180) deltaEuler.y -= 360;
        if (deltaEuler.y < 180) deltaEuler.y += 360;
        if (deltaEuler.z > 180) deltaEuler.z -= 360;
        if (deltaEuler.z < 180) deltaEuler.z += 360;
        lastEuler = transform.eulerAngles;
    }
}
