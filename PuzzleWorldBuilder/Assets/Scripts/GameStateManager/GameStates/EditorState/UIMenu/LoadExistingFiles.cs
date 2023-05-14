using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI;

public class LoadExistingFiles : EditorBase
{
    [SerializeField] GameObject framePrefab;
    [SerializeField] GameObject topLevelParent;
    RectTransform myRect;

    protected override void Start()
    {
        base.Start();
        myRect = GetComponent<RectTransform>();
        myRect.pivot = new Vector2(0, 1);
        UpdateList();
    }

    public void UpdateList()
    {
        string[] files = Directory.GetFiles(Application.persistentDataPath + "/", "*.puzzlebuildtool");

        int iteration = 0;
        foreach (string file in files)
        {
            GameObject fileObject = Instantiate(framePrefab, transform);
            RectTransform rect = fileObject.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0, 1);
            rect.localPosition = new Vector2(8, -8 - (rect.sizeDelta.y * iteration) - (4 * iteration));

            TMP_Text name = fileObject.GetComponentInChildren<TMP_Text>();
            name.text = Path.GetFileNameWithoutExtension(file);
            iteration++;

            Button button = fileObject.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => DataPersistenceManager.instance.SetFileName(name.text));
            button.onClick.AddListener(DataPersistenceManager.instance.LoadFile);
            button.onClick.AddListener(() => topLevelParent.SetActive(false));
        }

        myRect.sizeDelta = new Vector2(0, (80 * iteration));
    }
}
