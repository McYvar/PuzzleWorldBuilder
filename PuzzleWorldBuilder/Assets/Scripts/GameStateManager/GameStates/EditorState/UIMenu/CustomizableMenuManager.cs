using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CustomizableMenuManager : EditorBase
{
    [SerializeField] Color currentBorderColor;
    [SerializeField] Color currentMenuColor;
    List<IBorderCustomization> borders;
    List<IMenuCustomization> menus;

    public static CustomizableMenuManager instance { get; private set; }

    protected void Awake()
    {
        borders = new List<IBorderCustomization>();
        menus = new List<IMenuCustomization>();

        if (instance != null)
        {
            Debug.LogError("More than one border CustomizableMenuManager exists!");
        }
        instance = this;
    }

    protected override void Start()
    {
        base.Start();
        UpdateMenu();
    }

    void UpdateMenu()
    {
        foreach (var b in borders)
        {
            b.SetBorderColor(currentBorderColor);
        }

        foreach (var m in menus)
        {
            m.SetMenuColor(currentMenuColor);
        }
    }

    public void AddMeToBorders(IBorderCustomization border)
    {
        borders.Add(border);
        UpdateMenu();
    }

    public void AddMeToMenus(IMenuCustomization menu)
    {
        menus.Add(menu);
        UpdateMenu();
    }
}
