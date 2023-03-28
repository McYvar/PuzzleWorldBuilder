using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] string fileName;

    GameData gameData;
    public static DataPersistenceManager instance { get; private set; }
    FileDataHandler dataHandler;

    List<IDataPersistence> dataPersistenceObjects;
    Queue<IDataPersistence> addQueue;
    Queue<IDataPersistence> removeQueue;

    BinaryFormatter bf;
    XmlSerializer xmlFormatter;

    private void Awake()
    {
        dataPersistenceObjects = new List<IDataPersistence>();
        addQueue = new Queue<IDataPersistence>();
        removeQueue = new Queue<IDataPersistence>();
        if (instance != null)
        {
            Debug.LogError("Found more than one Data Persistence Manager in scene");
        }
        instance = this;
    }

    private void Start()
    {
        bf = new BinaryFormatter();
        xmlFormatter = new XmlSerializer(typeof(GameData));
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, xmlFormatter);
        dataPersistenceObjects = FindAllDataPersistenceObjects();
    }

    private void Update()
    {
        if (addQueue.Count > 0)
            dataPersistenceObjects.Add(addQueue.Dequeue());
        if (removeQueue.Count > 0)
            dataPersistenceObjects.Remove(removeQueue.Dequeue());
    }

    public void NewFile()
    {
        // promt for filename
        gameData = new GameData();

        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.NewData();
        }
    }

    public void LoadFile()
    {
        gameData = dataHandler.Load();

        if (gameData == null)
        {
            Debug.Log("No data was found. Initializing data to defaults");
            NewFile();
        }

        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(gameData);
        }
    }

    public void SaveFile()
    {
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(ref gameData);
        }

        dataHandler.Save(gameData);
    }

    public void AddDataPersistenceObject(IDataPersistence objToAdd)
    {
        addQueue.Enqueue(objToAdd);
    }

    public void RemoveDataPersistenceObject(IDataPersistence objToRemove)
    {
        removeQueue.Enqueue(objToRemove);
    }

    List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects);
    }
}
