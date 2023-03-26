using System.Collections.Generic;
using UnityEngine;

public class GridObject : SceneObject
{
    PuzzleGrid sharedGrid;
    public static List<GridObject> gridObjects = new List<GridObject>();

    Material unselectedMaterial = null;
    Material selectedMaterial = null;

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
    }

    public void Initialize(PuzzleGrid grid)
    {
        selectedMaterial = meshRenderer.material;
        sharedGrid = grid;
    }

    public override void OnStartMove()
    {
    }

    public override void OnFinishMove()
    {
    }

    public override void MoveTo(Vector3 newPos)
    {
        sharedGrid.MoveTile(myStartPos, newPos);
    }
}