using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BackGroundLayerOptions : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    public static List<DropDownMenu> AllDropDownMenus = new List<DropDownMenu>();

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.selectedObject == null)
        {
            foreach (var menu in AllDropDownMenus)
            {
                menu.HideMenu();
            }
        }
    }

    public static void ClearDropdowns()
    {
        foreach (var menu in AllDropDownMenus)
        {
            menu.HideMenu();
        }
    }
}
