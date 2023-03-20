using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToolArrow : AbstractGameEditor
{
    [SerializeField] Transform parent;
    [SerializeField] Camera mainCamera;
    [SerializeField] Transform toolCentre;
    Vector3 startPos;
    float setToolDepth;
    Vector3 offset;

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

    public void MouseDown(float depth)
    {
        setToolDepth = depth;
        startPos = parent.position;

        // the tool position is a tiny bit depenand on a offset determined by the mouse position, here we calulate that offset
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.nearClipPlane));
        Vector3 fromCamToToolDirection = parent.position - mainCamera.transform.position;

        // first we need both angles from the maincamera to both the mouse and the tool
        float mouseAngle = Vector3.Angle(mainCamera.transform.forward, mousePosition - mainCamera.transform.position);
        float toolAngle = Vector3.Angle(mainCamera.transform.forward, fromCamToToolDirection);

        // then since we know the depth of the tool, we need to calculate the mousepoint depth
        // first we need the camera depth calulated using the tool angle and the tool depth (camera depth = adjacent, toolDepth = oblique)
        float cameraDepth = setToolDepth * Mathf.Cos(toolAngle * Mathf.Deg2Rad);
        
        // then using this camera depth we calculate the mouseDepth
        float mouseDepth = cameraDepth / Mathf.Cos(mouseAngle * Mathf.Deg2Rad);

        // now we translate the mouseDepth into a offset vector
        Vector3 fromCamToMouseDirection = mousePosition - mainCamera.transform.position;
        offset = fromCamToMouseDirection.normalized * mouseDepth - fromCamToToolDirection.normalized * setToolDepth;

        foreach (SceneObject sceneObject in InputCommands.selectedObjects)
        {
            // for each selected object we define a starting position
            sceneObject.myStartPos = sceneObject.transform.position;
        }
    }

    public void MouseMove()
    {
        // now we caluclate the mousePosition each frame so we can calculate how much the object and tool is translated using the mouse
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.nearClipPlane));

        // and again with the mouseAngle and current original tooldepth, we can calculate the new tooldepth
        float mouseAngle = Vector3.Angle(mainCamera.transform.forward, mousePosition - mainCamera.transform.position);
        float newToolDepth = setToolDepth / Mathf.Cos(mouseAngle * Mathf.Deg2Rad);

        // and the tool position can be calculated using this depth
        Vector3 fromCamToMouseDirection = mousePosition - mainCamera.transform.position;
        parent.position = mainCamera.transform.position + fromCamToMouseDirection.normalized * newToolDepth - offset;

        // now we need to determine the displaced position vector
        Vector3 displacement = parent.position - startPos;

        foreach (SceneObject sceneObject in InputCommands.selectedObjects)
        {
            sceneObject.transform.position = sceneObject.myStartPos + displacement;
        }
    }

    public void MouseUp()
    {
    }
}
