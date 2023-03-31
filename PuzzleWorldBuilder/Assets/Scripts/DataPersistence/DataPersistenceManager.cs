using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

public class DataPersistenceManager : MonoBehaviour
{
    GameData gameData;
    public static DataPersistenceManager instance { get; private set; }
    FileDataHandler dataHandler;

    List<IDataPersistence> dataPersistenceObjects;
    Queue<IDataPersistence> addQueue;
    Queue<IDataPersistence> removeQueue;

    XmlSerializer xmlFormatter;

    string currentFile = "";

    private void Awake()
    {
        DontDestroyOnLoad(this);

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
        xmlFormatter = new XmlSerializer(typeof(GameData));
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
        gameData = dataHandler.Load(currentFile);

        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(gameData);
        }
    }

    public void SaveFile()
    {
        dataHandler = new FileDataHandler(Application.persistentDataPath, currentFile, xmlFormatter);
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(ref gameData);
        }

        dataHandler.Save(gameData, currentFile);
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

    public void AddNewFileAction()
    {
        UIListener.listener += NewFile;
    }

    
}
