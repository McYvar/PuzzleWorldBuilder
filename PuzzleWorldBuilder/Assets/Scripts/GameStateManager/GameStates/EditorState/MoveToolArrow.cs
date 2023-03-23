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

    Vector3 resultMove;

    [SerializeField] MeshRenderer myMesh;
    [SerializeField] Collider myCollider;
    [Range(0, 30), SerializeField] float minViewAngle;
    [SerializeField] bool isFreeMove;
    bool doEmission;

    [SerializeField] bool doPositionSnap;
    [SerializeField] float snapSize;

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
        float angle = Vector3.Angle(mainCamera.transform.forward, transform.forward);
        if (angle < minViewAngle || angle > 180 - minViewAngle)
        {
            myMesh.enabled = false;
            myCollider.enabled = false;
        }
        else
        {
            myMesh.enabled = true;
            myCollider.enabled = true;
        }

        if (doEmission)
        {
            myMesh.material.EnableKeyword("_EMISSION");
        }
        else
        {
            myMesh.material.DisableKeyword("_EMISSION");
        }
    }

    public void MouseDown(float currentArrowDepth, Vector3 currentToolCentre)
    {
        doEmission = true;
        arrowsDepth = currentArrowDepth;
        toolCentre.position = currentToolCentre;
        forwardDepth = (toolCentre.position - mainCamera.transform.position).magnitude * Mathf.Cos(Vector3.Angle(mainCamera.transform.forward, toolCentre.position - mainCamera.transform.position) * Mathf.Deg2Rad);
        startPos = toolCentre.position;

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

        foreach (TerrainObject terrainObject in InputCommands.selectedTerrainObjects)
        {
            // for each selected object we define a starting position
            terrainObject.myStartPos = terrainObject.transform.position;
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

        if (isFreeMove)
        {
            // in the case of a free move, we just set the resultMove to the displacement
            if (doPositionSnap)
            {
                int currentSnapX = (int)(displacement.x / snapSize);
                int currentSnapY = (int)(displacement.y / snapSize);
                int currentSnapZ = (int)(displacement.z / snapSize);
                resultMove = new Vector3(currentSnapX * snapSize, currentSnapY * snapSize, currentSnapZ * snapSize);
            }
            else resultMove = displacement;
        }
        else
        {
            // the displacement then needs to be translated into the forward direction of this arrow for a non-free move
            // we use the magnitude of the arrow, and translate this into the lenght of the forward direction using the angle between the two
            float arrowForwardLength = displacement.magnitude * Mathf.Cos(Vector3.Angle(displacement, transform.forward) * Mathf.Deg2Rad);
            if (doPositionSnap)
            {
                int currentSnap = (int)(arrowForwardLength / snapSize);
                resultMove = transform.forward * currentSnap * snapSize;
            }
            else resultMove = transform.forward * arrowForwardLength;
        }
        foreach (TerrainObject terrainObject in InputCommands.selectedTerrainObjects)
        {
            // Then we aply this result move to every selected object
            terrainObject.transform.position = terrainObject.myStartPos + resultMove;
        }

        float newArrowsDepth = arrowsDepth / Mathf.Cos(mouseAngle * Mathf.Deg2Rad);
        arrows.position = mainCamera.transform.position + (toolCentre.position - mainCamera.transform.position).normalized * newArrowsDepth;
    }

    public Vector3 MouseUp()
    {
        doEmission = false;
        return resultMove;
    }
}
