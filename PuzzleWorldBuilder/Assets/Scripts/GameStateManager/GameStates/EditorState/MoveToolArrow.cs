using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToolArrow : AbstractGameEditor
{
    [SerializeField] Transform parent;
    [SerializeField] Camera mainCamera;
    Vector3 startPos;
    Vector3 endPos;

    Vector3 mouseStart;
    Vector3 deltaPos;

    float startDepth;
    float currentDepth;

    protected override void OnEnable()
    {
        base.OnEnable();
        parent = transform.parent;
    }

    public override void EditorAwake()
    {
    }

    public override void EditorStart()
    {
    }

    public override void EditorUpdate()
    {
    }

    public void MouseDown()
    {
        startDepth = (parent.transform.position - mainCamera.transform.position).magnitude;

        Vector3 mousePoint = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.nearClipPlane));
        Vector3 fromCamToMouseDirection = mousePoint - mainCamera.transform.position;
        float angle = Vector3.Angle(mainCamera.transform.forward, fromCamToMouseDirection);
        currentDepth = startDepth / Mathf.Cos(angle * Mathf.Deg2Rad);
        Vector3 resultVector = mainCamera.transform.position + fromCamToMouseDirection.normalized * startDepth;
        parent.position = resultVector;
        deltaPos = parent.position;
    }

    public void MouseMove()
    {
        Vector3 mousePoint = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.nearClipPlane));
        Vector3 fromCamToMouseDirection = mousePoint - mainCamera.transform.position;
        float angle = Vector3.Angle(mainCamera.transform.forward, fromCamToMouseDirection);
        currentDepth = startDepth / Mathf.Cos(angle * Mathf.Deg2Rad);
        Vector3 resultVector = mainCamera.transform.position + fromCamToMouseDirection.normalized * startDepth;

        deltaPos = parent.position - deltaPos;
        foreach (SceneObject sceneObject in InputCommands.selectedObjects)
        {
            // needs some scalar that takes the object distance into account... or something...
            sceneObject.transform.position += deltaPos;
        }
        deltaPos = parent.position;
        parent.position = resultVector;
    }

    public void MouseUp()
    {
    }
}
