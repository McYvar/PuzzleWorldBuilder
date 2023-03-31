using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GotoNextUI : MonoBehaviour
{
    public void OpenMenu(GameObject newMenu)
    {
        newMenu.SetActive(true);
    }

    public void CloseMe()
    {
        gameObject.SetActive(false);
    }

    public void AddOpenMenuAction(GameObject newMenu)
    {
        UIListener.listener += () => newMenu.SetActive(true);
    }
}
