using UnityEngine;

public class GridObject : SceneObject
{
    PuzzleGrid sharedGrid;

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    public override void OnSelection()
    {
    }

    public override void OnDeselection()
    {

    }

    public override void OnDeletion()
    {
    }

    public void AssignGrid(PuzzleGrid grid) 
    {
        sharedGrid = grid;
    }
}