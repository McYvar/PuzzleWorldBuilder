using System.Collections.Generic;
using UnityEngine;

public class GridObject : SceneObject, IDataPersistence
{
    PuzzleGrid sharedGrid;
    public static List<GridObject> gridObjects = new List<GridObject>();

    TileInformation lastTileInformation = null;
    public bool isCreated = false;

    public GridObjectData mydata;

    protected override void OnEnable()
    {
        base.OnEnable();
        gridObjects.Add(this);
        isCreated = false;
        mydata = new GridObjectData();
        DataPersistenceManager.instance.AddDataPersistenceObject(this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        DataPersistenceManager.instance.RemoveDataPersistenceObject(this);
    }

    public override void OnSelection()
    {
        base.OnSelection();
        sharedGrid.OnSelectTile(transform.position);
    }

    public override void OnDeselection()
    {
        base.OnDeselection();
        sharedGrid.OnDeselectTile(transform.position);
    }

    public override void OnCreation()
    {
        isCreated = true;
        sharedGrid.CreateTile(transform.position);
        sharedGrid.OnSelectTile(transform.position);
    }

    public override void OnDeletion()
    {
        isCreated = false;
        lastTileInformation = sharedGrid.GetTile(transform.position);
        sharedGrid.OnDeleteTile(transform.position);
    }

    public void OnReCreation()
    {
        isCreated = true;
        sharedGrid.CreateTile(transform.position, lastTileInformation);
        sharedGrid.OnSelectTile(transform.position);
    }

    public void AssignGrid(PuzzleGrid grid)
    {
        sharedGrid = grid;
    }

    public override void MoveTo(Vector3 newPos)
    {
        sharedGrid.MoveTile(myStartPos, newPos);
        SavePostion();
    }

    public void SavePostion()
    {
        mydata.position = transform.position;
    }

    public void NewData()
    {
    }

    public void LoadData(GameData data)
    {
    }

    public void SaveData(ref GameData data)
    {
    }
}

[System.Serializable]
public class GridObjectData : SceneObjectData
{
    public TileType tileType;

    public GridObjectData()
    {
        position = Vector3.zero;
    }
}