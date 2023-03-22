using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleStageEditor : AbstractGameEditor
{
    [SerializeField] PuzzleGrid puzzleGrid;

    public override void EditorAwake()
    {
        puzzleGrid.Initialize();
    }

    public override void EditorStart()
    {
    }

    public override void EditorUpdate()
    {
    }

    private void OnDrawGizmos()
    {
        if (!UnityEditor.EditorApplication.isPlaying) return;
        foreach (Vector2Int vec in puzzleGrid.gridRegister)
        {
            if (puzzleGrid.GetTileType(vec) == TileType.NONE_TILE) Gizmos.color = Color.red;
            else if (puzzleGrid.GetTileType(vec) == TileType.EDGE_TILE) Gizmos.color = Color.yellow;
            else if (puzzleGrid.GetTileType(vec) == TileType.PLACEABLE_TILE) Gizmos.color = Color.blue;

            Gizmos.DrawSphere(new Vector3(vec.x, 0, vec.y), 0.3f);
        }
    }
}
