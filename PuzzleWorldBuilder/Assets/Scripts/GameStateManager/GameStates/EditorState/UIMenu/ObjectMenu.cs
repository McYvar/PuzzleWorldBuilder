using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectMenu : BaseMenuWindow
{
    /// <summary>
    /// Date: 03/10/2023, By: Yvar
    /// The object menu class keeps track of all listed prefabs.
    /// The size of the menu is determined by the amount of objects listed.
    /// Also will the menu be categorized, so for each category there is a seperate list.
    /// </summary>

    [SerializeField] ObjectMenuItemSpace objectMenuItemSpacePrefab;
    [SerializeField] ObjectMenuItemSpace categoryButtonPrefab;
    [SerializeField] float spaceBetweenItems;
    [SerializeField] float edgeWidthOffset;
    [SerializeField] float edgeHeightOffset;
    [SerializeField, Range(1, 7)] int maxItemsInARow;

    [SerializeField] Category[] categories;
    [SerializeField] int currentCategory = 0;
    int navigator = 0;

    [SerializeField] InputCommands inputCommands;
    [SerializeField] Transform objectListTransform;

    [SerializeField] Transform spawnPivot;

    protected void Start()
    {
        CreateCategoryItems();
        UpdateSizeDelta();
        DisplayCategory(currentCategory);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (navigator >= categories.Length)
        {
            navigator = 0;
            return;
        }
    }

    void UpdateSizeDelta()
    {
        float compareHeight = categories.Length * categoryButtonPrefab.GetRectTransform().sizeDelta.x;
        float newHeight = (((categories[currentCategory].parentObjectTransforms.Count - 1) / maxItemsInARow) + 1) * objectMenuItemSpacePrefab.GetRectTransform().sizeDelta.y + (((categories[currentCategory].parentObjectTransforms.Count - 1) / maxItemsInARow) + 2) * spaceBetweenItems + 2 * edgeHeightOffset;
        if (newHeight < compareHeight) newHeight = compareHeight  - outlineWidth * 0.5f;
        if (objectMenuItemSpacePrefab.GetRectTransform() != null && categories.Length > 0)
        rectTransform.sizeDelta = new Vector2(maxItemsInARow * objectMenuItemSpacePrefab.GetRectTransform().sizeDelta.x + (maxItemsInARow + 1) * spaceBetweenItems + 2 * edgeWidthOffset, newHeight);
    }

    void CreateCategoryItems()
    {
        for (int categoryIterator = 0; categoryIterator < categories.Length; categoryIterator++)
        {
            List<Transform> createdItems = new List<Transform>();
            for (int itemIterator = 0; itemIterator < categories[categoryIterator].items.Count; itemIterator++)
            {
                // first we create an instance of the item frame prefab and place it in the correct position
                Transform newItem = Instantiate(objectMenuItemSpacePrefab.gameObject, transform).transform;
                RectTransform newItemRect = newItem.GetComponent<RectTransform>();
                if (newItemRect != null)
                {
                    newItemRect.localPosition = new Vector2(edgeWidthOffset + spaceBetweenItems + (spaceBetweenItems + objectMenuItemSpacePrefab.GetRectTransform().sizeDelta.x) * (itemIterator % maxItemsInARow), -(edgeHeightOffset + spaceBetweenItems + (spaceBetweenItems + objectMenuItemSpacePrefab.GetRectTransform().sizeDelta.y) * (itemIterator / maxItemsInARow)));
                }
                newItem.gameObject.SetActive(false);
                createdItems.Add(newItem);

                // then inside this newly created item frame we put an image inside with specific offsets and assign the 
                Item currentItem = categories[categoryIterator].items[itemIterator];
                currentItem.Initialize(inputCommands.GetCommandManager(), objectListTransform, spawnPivot);
                ObjectMenuItemSpace currentItemSpace = newItem.GetComponent<ObjectMenuItemSpace>();
                currentItemSpace.button.image.sprite = currentItem.sprite;
                currentItemSpace.button.onClick.AddListener(() => currentItem.AddItemToScene());
            }
            // now add and connect a button to the category
            categories[categoryIterator].parentObjectTransforms = createdItems;
            GameObject buttonObj = Instantiate(categoryButtonPrefab.gameObject, transform);
            ObjectMenuItemSpace button = buttonObj.GetComponent<ObjectMenuItemSpace>();
            categories[categoryIterator].button = button;
            RectTransform currentButtonRectTransform = button.GetRectTransform();
            currentButtonRectTransform.localPosition = new Vector2(0, -currentButtonRectTransform.sizeDelta.x * categoryIterator);
            int currentCategory = categoryIterator;
            button.button.onClick.AddListener(() => DisplayCategory(currentCategory));
            button.text.text = categories[currentCategory].name;
        }
    }

    public void DisplayCategory(int category)
    {
        foreach (var item in categories[currentCategory].parentObjectTransforms)
        {
            item.gameObject.SetActive(false);
        }
        categories[currentCategory].button.button.image.color = categories[currentCategory].button.normalColor;
        currentCategory = category;
        foreach (var item in categories[currentCategory].parentObjectTransforms)
        {
            item.gameObject.SetActive(true);
        }
        categories[currentCategory].button.button.image.color = categories[currentCategory].button.activeColor;
        UpdateSizeDelta();
    }
}

[System.Serializable]
public struct Category
{
    public string name;
    public List<Item> items;
    [HideInInspector] public List<Transform> parentObjectTransforms;
    [HideInInspector] public ObjectMenuItemSpace button;
}
