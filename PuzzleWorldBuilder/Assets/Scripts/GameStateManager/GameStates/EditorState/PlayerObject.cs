using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObject : TerrainObject
{
    public new PlayerObjectData myData;

    protected override void OnEnable()
    {
        base.OnEnable();
        myData = new PlayerObjectData();
    }

    public override void SaveObject()
    {
        myData.position = transform.position;
    }
}

public class PlayerObjectData : TerrainObjectData
{
    public PlayerObjectData() : base()
    {
        name = "player";
    }
}