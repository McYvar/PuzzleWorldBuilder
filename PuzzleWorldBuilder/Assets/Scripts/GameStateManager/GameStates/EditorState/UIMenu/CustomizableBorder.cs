using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomizableBorder : EditorBase, IBorderCustomization
{
    [SerializeField] Image outlineTop;
    [SerializeField] Image outlineBottom;
    [SerializeField] Image outlineLeft;
    [SerializeField] Image outlineRight;

    protected void Start()
    {
        CustomizableMenuManager.instance.AddMeToBorders(this);
    }

    public void SetBorderColor(Color newColor)
    {
        outlineTop.color = newColor;
        outlineBottom.color = newColor;
        outlineLeft.color = newColor;
        outlineRight.color = newColor;
    }
}
