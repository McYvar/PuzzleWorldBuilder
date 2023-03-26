using System.Collections.Generic;
using UnityEngine;

public class GridObject : SceneObject
{
    PuzzleGrid sharedGrid;
    public static List<GridObject> gridObjects = new List<GridObject>();

    protected override void OnEnable()
    {
        base.OnEnable();
        gridObjects.Add(this);
    }

    public override void OnSelection()
    {
        base.OnSelection();
        Debug.Log("Selected grid: " + name);
        sharedGrid.OnSelectTile(transform.position);
    }

    public override void OnDeselection()
    {
        base.OnDeselection();
        Debug.Log("Deselected grid: " + name);
        sharedGrid.OnDeselectTile(transform.position);
    }

    public override void OnDeletion()
    {
        sharedGrid.OnDeleteTile(transform.position);
    }

    public void Initialize(PuzzleGrid grid)
    {
        sharedGrid = grid;
    }

    public override void MoveTo(Vector3 newPos)
    {
        base.MoveTo(newPos);
        sharedGrid.MoveTile(myStartPos, actualMove);
    }
}