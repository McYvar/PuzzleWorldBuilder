using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataPersistence
{
    void NewData();
    void LoadData(GameData data);
    void SaveData(ref GameData data);
}
