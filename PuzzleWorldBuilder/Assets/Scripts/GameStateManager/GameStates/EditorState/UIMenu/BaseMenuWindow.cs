using UnityEngine;
using UnityEngine.UI;

public class BaseMenuWindow : AbstractGameEditor
{
    [SerializeField] protected RectTransform outlineTop;
    [SerializeField] protected RectTransform outlineBottom;
    [SerializeField] protected RectTransform outlineLeft;
    [SerializeField] protected RectTransform outlineRight;
    [SerializeField] protected float outlineWidth = 0.5f;
    [SerializeField] protected RectTransform rectTransform;

    protected override void OnEnable()
    {
        base.OnEnable();
        rectTransform = GetComponent<RectTransform>();
    }

    protected virtual void Update()
    {
        MenuOutline();
    }

    public override void EditorAwake()
    {
    }

    public override void EditorStart()
    {
    }

    public override void EditorUpdate()
    {
        MenuOutline();
    }

    void MenuOutline()
    {
        outlineTop.sizeDelta = new Vector2(rectTransform.sizeDelta.x + outlineWidth, outlineWidth);
        outlineBottom.sizeDelta = outlineTop.sizeDelta;
        outlineBottom.localPosition = new Vector2(outlineTop.localPosition.x, -rectTransform.sizeDelta.y);
        outlineLeft.sizeDelta = new Vector2(outlineWidth, rectTransform.sizeDelta.y + outlineWidth);
        outlineRight.sizeDelta = outlineLeft.sizeDelta;
        outlineRight.localPosition = new Vector2(rectTransform.sizeDelta.x, outlineLeft.localPosition.y);
    }

    public RectTransform GetRectTransform()
    {
        return rectTransform;
    }

}
