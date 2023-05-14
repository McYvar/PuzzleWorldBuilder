using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomizableMenu : EditorBase, IMenuCustomization
{
    [SerializeField] Image menu;

    protected override void Start()
    {
        base.Start();
        CustomizableMenuManager.instance.AddMeToMenus(this);
    }

    public void SetMenuColor(Color newColor)
    {
        menu.color = newColor;
    }
}
