using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;

[ExecuteAlways]
public class DropDownMenu : MonoBehaviour
{
    [SerializeField] GameObject dropDownObject;
    RectTransform dropDownRect;
    [SerializeField] float dropDownHeight;

    [SerializeField] List<Button> buttons = new List<Button>();
    [SerializeField] Sprite dropDownImage;
    [SerializeField] float buttonWidth;
    [SerializeField] float buttonHeight;

    bool display = false;
    RectTransform rectTransform;
    [SerializeField] GameObject emptyGameObj;

    private void OnEnable()
    {
        BackGroundLayerOptions.AllDropDownMenus.Add(this);

        rectTransform = GetComponent<RectTransform>();
        dropDownRect = dropDownObject.GetComponent<RectTransform>();
        if (emptyGameObj == null) emptyGameObj = new GameObject("Empty");
    }

    private void OnDisable()
    {
        BackGroundLayerOptions.AllDropDownMenus.Remove(this);
        DestroyImmediate(emptyGameObj);
    }

    private void Update()
    {
        if (!display)
        {
            dropDownObject.SetActive(false);
            if (EditorApplication.isPlaying) return;
        }

        dropDownObject.SetActive(true);

        int iterator = 0;
        foreach (var button in buttons)
        {
            button.targetGraphic.rectTransform.sizeDelta = new Vector2(buttonWidth, buttonHeight);
            button.targetGraphic.rectTransform.localPosition = dropDownRect.localPosition + new Vector3(1,  (-(buttonHeight + 1) * iterator) - (rectTransform.sizeDelta.y + 3));
            iterator++;
        }

        dropDownRect.sizeDelta = new Vector2(buttonWidth + 2, (buttonHeight + 2) * (iterator + 0.5f));
    }

    public void DisplayMenu()
    {
        BackGroundLayerOptions.ClearDropdowns();
        display = true;
    }

    public void HideMenu()
    {
        display = false;
    }

    public void ToggleMenu()
    {
        if (display)
        {
            display = false;
        }
        else
        {
            BackGroundLayerOptions.ClearDropdowns();
            display = true;
        }
    }

    public void AddButtonToMenu()
    {
        GameObject obj = Instantiate(emptyGameObj, dropDownObject.transform);
        obj.name = "Button" + buttons.Count;
        Button button = obj.AddComponent<Button>();
        buttons.Add(button);
        Image image = obj.AddComponent<Image>();
        image.sprite = dropDownImage;
        button.targetGraphic = image;
        button.onClick.AddListener(() => HideMenu());
        image.rectTransform.pivot = rectTransform.pivot + new Vector2(dropDownRect.localPosition.x, 0);

        GameObject textObj = Instantiate(emptyGameObj, obj.transform);
        textObj.name = "Text (TMP) custom";

        textObj.AddComponent<CanvasRenderer>();
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        text.rectTransform.sizeDelta = new Vector2(buttonWidth, buttonHeight);
        text.fontSize = 12;
        text.alignment = TextAlignmentOptions.Center;
        text.text = "button" + buttons.Count.ToString();
        text.color = Color.black;
    }

    public void RemoveButtonFromMenu()
    {
        DestroyImmediate(buttons[buttons.Count - 1].gameObject);
        buttons.RemoveAt(buttons.Count - 1);
    }

    public void ClearButtons()
    {
        buttons.Clear();
    }
}
