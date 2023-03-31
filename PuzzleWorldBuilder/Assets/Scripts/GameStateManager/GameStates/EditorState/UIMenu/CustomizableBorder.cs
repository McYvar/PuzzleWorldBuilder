using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomizableBorder : AbstractGameEditor, IBorderCustomization
{
    [SerializeField] Image outlineTop;
    [SerializeField] Image outlineBottom;
    [SerializeField] Image outlineLeft;
    [SerializeField] Image outlineRight;

    public override void EditorAwake()
    {
    }

    public override void EditorStart()
    {
        CustomizableMenuManager.instance.AddMeToBorders(this);
    }

    public override void EditorUpdate()
    {
    }

    public void SetBorderColor(Color newColor)
    {
        outlineTop.color = newColor;
        outlineBottom.color = newColor;
        outlineLeft.color = newColor;
        outlineRight.color = newColor;
    }
}
