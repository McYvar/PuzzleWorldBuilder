using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleStageEditor : EditorBase
{
    [SerializeField] Material[] noneTileMaterials;
    PuzzleGrid puzzleGrid;

    GridObject currentGridObject;

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (puzzleGrid == null) return;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit);
        if (hit.collider != null)
        {
            currentGridObject = hit.collider.GetComponent<GridObject>();
            if (currentGridObject != null) puzzleGrid.HighlightTile((int)hit.collider.transform.position.x, (int)hit.collider.transform.position.z);
            else puzzleGrid.UnhighLightCurrentTile();
        }
    }

    public PuzzleGrid GetPuzzleGrid()
    {
        return puzzleGrid;
    }

    public void InitializeNewGrid(int maxX, int maxZ, int minX, int minZ, Vector3[] locations, TileType[] tileTypes)
    {
        puzzleGrid = new PuzzleGrid(maxX, maxZ, Vector3.zero, noneTileMaterials);
        puzzleGrid.IncreaseGrid(minX, minZ);

        if (locations.Length > 0)
        {
            for (int i = 0; i < locations.Length; i++)
            {
                if (tileTypes[i] != TileType.NONE_TILE)
                {
                    puzzleGrid.CreateTile(locations[i], tileTypes[i]);
                }
            }
        }
    }

    public void InitializeNewGrid(int maxX, int maxZ, int minX, int minZ)
    {
        puzzleGrid = new PuzzleGrid(maxX, maxZ, Vector3.zero, noneTileMaterials);
        puzzleGrid.IncreaseGrid(minX, minZ);
    }
}
