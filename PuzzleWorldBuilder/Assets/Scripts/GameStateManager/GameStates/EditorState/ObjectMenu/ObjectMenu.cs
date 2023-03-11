using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMenu : BaseMenuWindow
{
    /// <summary>
    /// Date: 03/10/2023, By: Yvar
    /// The object menu class keeps track of all listed prefabs.
    /// The size of the menu is determined by the amount of objects listed.
    /// Also will the menu be categorized, so for each category there is a seperate list.
    /// </summary>

    [SerializeField] ObjectMenuItemSpace objectMenuItemSpace;
    [SerializeField] float spaceBetweenItems;
    [SerializeField] float edgeWidthOffset;
    [SerializeField] float edgeHeightOffset;
    [SerializeField, Range(1, 7)] int maxItemsInARow;

    [SerializeField] Catagory[] catagories;
    [SerializeField] int currentCategory = 0;
    int navigator = 0;

    protected override void OnEnable()
    {
        base.OnEnable();
        CreateCatagoryItems();
        UpdateSizeDelta();
        DisplayCatagory(currentCategory);
    }

    protected override void Update()
    {
        base.Update();
    }
    
    public override void EditorUpdate()
    {
        if (navigator >= catagories.Length)
        {
            navigator = 0;
            return;
        }
    }

    void UpdateSizeDelta()
    {
        if (objectMenuItemSpace.GetRectTransform() != null)
        rectTransform.sizeDelta = new Vector2(maxItemsInARow * objectMenuItemSpace.GetRectTransform().sizeDelta.x + (maxItemsInARow + 1) * spaceBetweenItems + 2 * edgeWidthOffset,
                                             (((catagories[currentCategory].items.Count - 1) / maxItemsInARow) + 1) * objectMenuItemSpace.GetRectTransform().sizeDelta.y + (((catagories[currentCategory].items.Count - 1) / maxItemsInARow) + 2) * spaceBetweenItems + 2 * edgeHeightOffset);
    }

    void CreateCatagoryItems()
    {
        for (int catagoryIterator = 0; catagoryIterator < catagories.Length; catagoryIterator++)
        {
            List<GameObject> createdItems = new List<GameObject>();
            for (int itemIterator = 0; itemIterator < catagories[catagoryIterator].items.Count; itemIterator++)
            {
                GameObject newItem = Instantiate(objectMenuItemSpace.gameObject, transform);
                RectTransform newItemRect = newItem.GetComponent<RectTransform>();
                if (newItemRect != null)
                {
                    newItemRect.localPosition = new Vector2(edgeWidthOffset + spaceBetweenItems + (spaceBetweenItems + objectMenuItemSpace.GetRectTransform().sizeDelta.x) * (itemIterator % maxItemsInARow), -(edgeHeightOffset + spaceBetweenItems + (spaceBetweenItems + objectMenuItemSpace.GetRectTransform().sizeDelta.y) * (itemIterator / maxItemsInARow)));
                }
                newItem.SetActive(false);
                createdItems.Add(newItem);
            }
            catagories[catagoryIterator].items = createdItems;
        }
    }

    public void DisplayCatagory(int category)
    {
        foreach (var item in catagories[currentCategory].items)
        {
            item.gameObject.SetActive(false);
        }
        currentCategory = category;
        foreach (var item in catagories[currentCategory].items)
        {
            item.gameObject.SetActive(true);
        }
        UpdateSizeDelta();
    }
}

[System.Serializable]
public struct Catagory
{
    public string name;
    //maybe turn this into a image...
    public List<GameObject> items;
}
