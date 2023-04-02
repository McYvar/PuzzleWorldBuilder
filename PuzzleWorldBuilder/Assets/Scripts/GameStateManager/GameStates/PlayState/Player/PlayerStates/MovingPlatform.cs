using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : AbstractGameRunner
{
    Vector3 lastPos;
    public Vector3 deltaPos;

    Vector3 lastEuler;
    public Vector3 deltaEuler;

    public override void RunnerAwake()
    {
    }

    public override void RunnerStart()
    {
        lastPos = transform.position;
        lastEuler = transform.eulerAngles;
    }

    public override void RunnerUpdate()
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

    public override void RunnerFixedUpdate()
    {
    }

    public override void RunnerLateUpdate()
    {
    }
}
