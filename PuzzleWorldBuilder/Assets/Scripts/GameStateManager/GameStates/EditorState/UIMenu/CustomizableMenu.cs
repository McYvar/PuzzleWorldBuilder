using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomizableMenu : AbstractGameEditor, IMenuCustomization
{
    [SerializeField] Image menu;

    public override void EditorAwake()
    {
    }

    public override void EditorStart()
    {
        CustomizableMenuManager.instance.AddMeToMenus(this);
    }

    public override void EditorUpdate()
    {
    }

    public void SetMenuColor(Color newColor)
    {
        menu.color = newColor;
    }
}
