using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToolArrow : AbstractGameEditor
{
    [SerializeField] Camera mainCamera;
    [SerializeField] Transform toolCentre;
    Transform arrows;
    float arrowsDepth;
    Vector3 startPos;
    float forwardDepth;
    Vector3 offset;
    Vector3 displacement;

    protected override void OnEnable()
    {
        base.OnEnable();
        arrows = transform.parent;
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

    public void MouseDown(float depth, Vector3 centre)
    {
        arrowsDepth = depth;
        toolCentre.position = centre;
        forwardDepth = (toolCentre.position - mainCamera.transform.position).magnitude * Mathf.Cos(Vector3.Angle(mainCamera.transform.forward, toolCentre.position - mainCamera.transform.position) * Mathf.Deg2Rad);
        startPos = toolCentre.position;
        Debug.Log(forwardDepth);

        // the tool position is a tiny bit depenand on a offset determined by the mouse position, here we calulate that offset
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.nearClipPlane));
        Vector3 fromCamToToolDirection = toolCentre.position - mainCamera.transform.position;

        // first we need both angles from the maincamera to both the mouse and the tool
        float mouseAngle = Vector3.Angle(mainCamera.transform.forward, mousePosition - mainCamera.transform.position);
        float toolAngle = Vector3.Angle(mainCamera.transform.forward, fromCamToToolDirection);

        // then since we know the depth of the tool, we need to calculate the mousepoint depth
        // first we need the camera depth calulated using the tool angle and the tool depth (camera depth = adjacent, toolDepth = oblique)
        float cameraDepth = forwardDepth * Mathf.Cos(toolAngle * Mathf.Deg2Rad);
        
        // then using this camera depth we calculate the mouseDepth
        float mouseDepth = cameraDepth / Mathf.Cos(mouseAngle * Mathf.Deg2Rad);

        // now we translate the mouseDepth into a offset vector
        Vector3 fromCamToMouseDirection = mousePosition - mainCamera.transform.position;
        offset = fromCamToMouseDirection.normalized * mouseDepth - fromCamToToolDirection.normalized * forwardDepth;

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
        float newToolDepth = forwardDepth / Mathf.Cos(mouseAngle * Mathf.Deg2Rad);

        // and the tool position can be calculated using this depth
        Vector3 fromCamToMouseDirection = mousePosition - mainCamera.transform.position;
        toolCentre.position = mainCamera.transform.position + fromCamToMouseDirection.normalized * newToolDepth - offset;

        // now we need to determine the displaced position vector
        displacement = toolCentre.position - startPos;

        foreach (SceneObject sceneObject in InputCommands.selectedObjects)
        {
            sceneObject.transform.position = sceneObject.myStartPos + displacement;
        }

        float newArrowsDepth = arrowsDepth / Mathf.Cos(mouseAngle * Mathf.Deg2Rad);
        arrows.transform.position = mainCamera.transform.position + (toolCentre.position - mainCamera.transform.position).normalized * newArrowsDepth;
    }

    public void MouseUp()
    {
        // add displacement to a movecommand!
    }
}
