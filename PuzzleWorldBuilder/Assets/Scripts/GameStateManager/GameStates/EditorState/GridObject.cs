using System.Collections.Generic;
using UnityEngine;

public class GridObject : SceneObject
{
    PuzzleGrid sharedGrid;
    public static List<GridObject> gridObjects = new List<GridObject>();

    Color unselectedColor = Color.red;
    Color selectedColor = Color.yellow;

    protected override void OnEnable()
    {
        base.OnEnable();
        gridObjects.Add(this);
    }

    public override void OnSelection()
    {
        base.OnSelection();
        Debug.Log("Selected grid: " + name);
        meshRenderer.material.color = selectedColor;
    }

    public override void OnDeselection()
    {
        base.OnDeselection();
        Debug.Log("Deselected grid: " + name);
        meshRenderer.material.color = unselectedColor;
    }

    public override void OnDeletion()
    {
    }

    public void Initialize(PuzzleGrid grid)
    {
        sharedGrid = grid;
    }

    public override void OnFinishMove()
    {
        meshRenderer.material.color = selectedColor;
    }

    public override void MoveTo(Vector3 newPos)
    {
        sharedGrid.MoveTile(myStartPos, newPos);
    }
}