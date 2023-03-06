using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class BackGroundLayerOptions : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    public static List<DropDownMenu> AllDropDownMenus = new List<DropDownMenu>();
    [SerializeField] DropDownMenu FloatingDropDownMenu;
    [SerializeField] Canvas backGroundCanvas;

    public UnityEvent leftClickUp;
    public UnityEvent middleClickUp;
    public UnityEvent rightClickUp;

    public UnityEvent leftClickDown;
    public UnityEvent middleClickDown;
    public UnityEvent rightClickDown;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            leftClickDown.Invoke();
        }
        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            middleClickDown.Invoke();
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            rightClickDown.Invoke();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            leftClickUp.Invoke();
        }
        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            middleClickUp.Invoke();
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            rightClickUp.Invoke();
            OpenFloatingDropDown(eventData);
        }
    }

    public static void ClearDropdowns()
    {
        foreach (var menu in AllDropDownMenus)
        {
            menu.HideMenu();
        }
    }

    public void OpenFloatingDropDown(PointerEventData eventData)
    {
        FloatingDropDownMenu.DisplayDropDownOnLocation(eventData.position.x, eventData.position.y);
    }
}
