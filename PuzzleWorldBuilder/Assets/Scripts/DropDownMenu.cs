using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class DropDownMenu : MonoBehaviour
{
    /// <summary>
    /// Date: 03/05/2023, By: Yvar
    /// Script for creating dropdown menu's easier
    /// Not very sophisticated yet. Somethings might not work, then you just have to re-enable the gameobject this script is
    /// on in the editor. Script essentially creates options for a dropdown menu that scales with the amount of options. 
    /// Unfortunately only deletes the last one created, so might work on that problem later on.
    /// </summary>

    [SerializeField] GameObject dropDownObject;
    RectTransform dropDownRect;

    [SerializeField] List<Button> buttons = new List<Button>();
    [SerializeField] Sprite dropDownImage;
    [SerializeField] float buttonWidth;
    [SerializeField] float buttonHeight;

    [SerializeField] float textOffset;
    [SerializeField] List<TextMeshProUGUI> buttonTexts = new List<TextMeshProUGUI>();

    bool display = false;
    RectTransform rectTransform;
    [SerializeField] GameObject emptyGameObj;
    [SerializeField] bool visibleOnEdit = false;
    [SerializeField] bool hideMainButtonText;
    [SerializeField] TextMeshProUGUI mainButtonText;

    [SerializeField] Canvas ParentCanvas;
    float canvasScale;

    private void OnEnable()
    {
        BackGroundLayerOptions.AllDropDownMenus.Add(this);

        rectTransform = GetComponent<RectTransform>();
        dropDownRect = dropDownObject.GetComponent<RectTransform>();
    }

    private void OnDisable()
    {
        BackGroundLayerOptions.AllDropDownMenus.Remove(this);
    }

    /// <summary>
    /// The script has to scale with the amount of option entries, also the text length is included in a seperate
    /// list to scale along aswell.
    /// </summary>
    private void Update()
    {
        if (!display)
        {
            dropDownObject.SetActive(false);
            if (hideMainButtonText && mainButtonText != null) mainButtonText.enabled = false;

#if UNITY_EDITOR
            if (EditorApplication.isPlaying) return;
#endif
            if (!visibleOnEdit) return;
        }

        dropDownObject.SetActive(true);
        if (mainButtonText != null) mainButtonText.enabled = true;

        int iterator = 0;
        foreach (var button in buttons)
        {
            button.targetGraphic.rectTransform.sizeDelta = new Vector2(buttonWidth, buttonHeight);
            button.targetGraphic.rectTransform.localPosition = dropDownRect.localPosition + new Vector3(1, (-(buttonHeight + 1) * iterator) - (rectTransform.sizeDelta.y + 3));
            iterator++;
        }
        foreach (var text in buttonTexts)
        {
            text.rectTransform.localPosition = new Vector2(textOffset, text.rectTransform.localPosition.y);
        }

        dropDownRect.sizeDelta = new Vector2(buttonWidth + 2, (buttonHeight + 1) * (buttons.Count + 1));
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
        button = CopyBaseButton(GetComponent<Button>(), button);
        buttons.Add(button);
        Image image = obj.AddComponent<Image>();
        image.sprite = dropDownImage;
        button.targetGraphic = image;
        image.rectTransform.pivot = new Vector2(0f, 1f);

        GameObject textObj = Instantiate(emptyGameObj, obj.transform);
        textObj.name = "Text (TMP) custom";

        textObj.AddComponent<CanvasRenderer>();
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Left;
        text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        text.rectTransform.sizeDelta = new Vector2(buttonWidth, buttonHeight);
        text.fontSize = 12;
        text.text = "button" + buttons.Count.ToString();
        text.color = Color.black;
        buttonTexts.Add(text);
    }

    public void RemoveButtonFromMenu()
    {
        DestroyImmediate(buttons[buttons.Count - 1].gameObject);
        buttons.RemoveAt(buttons.Count - 1);
        buttonTexts.RemoveAt(buttonTexts.Count - 1);
    }

    public void ClearButtons()
    {
        buttons.Clear();
        buttonTexts.Clear();
    }

    public Button CopyBaseButton(Button parent, Button copy)
    {
        copy.colors = parent.colors;
        return copy;
    }

    public void DisplayDropDownOnLocation(float x, float y)
    {
        BackGroundLayerOptions.ClearDropdowns();
        display = true;

        if (ParentCanvas != null) canvasScale = ParentCanvas.scaleFactor;
        if (x + dropDownRect.sizeDelta.x * canvasScale > Screen.width) x -= dropDownRect.sizeDelta.x * canvasScale;
        if (y - dropDownRect.sizeDelta.y * canvasScale < 0) y += dropDownRect.sizeDelta.y * canvasScale;

        rectTransform.position = new Vector2(x, y);
    }
}
