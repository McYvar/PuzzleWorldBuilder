using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Xml.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

public class FileDataHandler
{
    //BinaryFormatter bf;
    XmlSerializer xmlFormatter;
    string dataDirPath = "";

    public FileDataHandler(string dataDirPath, string dataFileName, XmlSerializer formatter)
    {
        xmlFormatter = formatter;
        this.dataDirPath = dataDirPath;
    }

    public GameData Load(string fileName)
    {
        string fullPath = Path.Combine(dataDirPath, fileName + ".PuzzleWorldBuilder");
        GameData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                using FileStream stream = new FileStream(fullPath, FileMode.Open);
                loadedData = (GameData)xmlFormatter.Deserialize(stream);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to load data from file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
    }

    public void Save(GameData data, string fileName)
    {
        string fullPath = Path.Combine(dataDirPath, fileName + ".PuzzleWorldBuilder");
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            using FileStream stream = new FileStream(fullPath, FileMode.Create);
            xmlFormatter.Serialize(stream, data);
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
    }
}
