using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleStageEditor : AbstractGameEditor
{
    [SerializeField] Material[] noneTileMaterials;
    PuzzleGrid puzzleGrid;

    GridObject currentGridObject;

    public override void EditorAwake()
    {
        puzzleGrid = new PuzzleGrid(30, 30, Vector3.zero, noneTileMaterials);
    }

    public override void EditorStart()
    {
    }

    public override void EditorUpdate()
    {
        if (Input.GetKeyUp(KeyCode.S))
        {
            puzzleGrid.IncreaseGrid(1, 1);
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            puzzleGrid.IncreaseGrid(-1, -1);
        }

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit);
        if (hit.collider != null)
        {
            currentGridObject = hit.collider.GetComponent<GridObject>();
            if (currentGridObject != null) puzzleGrid.HighlightTile((int)hit.collider.transform.position.x, (int)hit.collider.transform.position.z);
        }
    }

    public PuzzleGrid GetPuzzleGrid()
    {
        return puzzleGrid;
    }

    private void OnDrawGizmos()
    {
    }
}
