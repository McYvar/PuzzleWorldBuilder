using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectObjectCommand : BaseObjectCommands, IPointerDownHandler, IPointerUpHandler
{
    public static List<SceneObject> selectedObjects = new List<SceneObject>();

    [SerializeField] Canvas mainCanvas;
    [SerializeField] RectTransform selectionLineTop;
    [SerializeField] RectTransform selectionLineBottom;
    [SerializeField] RectTransform selectionLineLeft;
    [SerializeField] RectTransform selectionLineRight;
    [SerializeField] float selectionWidth;

    [SerializeField] Camera mainCamera;

    Vector2 selectionStartingPoint;
    Vector2 selectionEndingPoint;
    bool isSelecting;

    Stack<SceneObject[]> redoStack;

    protected override void OnEnable()
    {
        base.OnEnable();
        redoStack = new Stack<SceneObject[]>();
    }


    public override void Execute()
    {
        foreach (SceneObject sceneObject in selectedObjects)
        {
            sceneObject.OnSelection();
        }
    }

    public override void Undo()
    {
        SceneObject[] redoSceneObjects = new SceneObject[selectedObjects.Count];
        for(int i = 0; i < redoSceneObjects.Length; i++)
        {
            selectedObjects[i].OnDeselection();
            redoSceneObjects[i] = selectedObjects[i];
        }
        selectedObjects.Clear();
        redoStack.Push(redoSceneObjects);
    }

    public override void Redo()
    {
        SceneObject[] redoObjects = redoStack.Pop();
        for(int i = 0;i < redoObjects.Length;i++)
        {
            redoObjects[i].OnSelection();
            selectedObjects.Add(redoObjects[i]);
        }
    }

    public override void EditorUpdate()
    {
        if (isSelecting)
        {
            // draw selection
            selectionEndingPoint = Input.mousePosition;
            if (selectionStartingPoint.x < selectionEndingPoint.x)
            {
                selectionLineTop.position = selectionStartingPoint;
            }
            else
            {
                selectionLineTop.position = selectionStartingPoint + new Vector2(selectionEndingPoint.x - selectionStartingPoint.x, 0);
            }

            if (selectionStartingPoint.y > selectionEndingPoint.y)
            {
                selectionLineLeft.position = selectionStartingPoint;
            }
            else
            {
                selectionLineLeft.position = selectionStartingPoint + new Vector2(0, selectionEndingPoint.y - selectionStartingPoint.y);
            }
            selectionLineBottom.position = new Vector2(selectionLineTop.position.x, selectionEndingPoint.y);
            selectionLineRight.position = new Vector2(selectionEndingPoint.x, selectionLineLeft.position.y);
            selectionLineTop.sizeDelta = new Vector2(Mathf.Abs((selectionEndingPoint.x - selectionStartingPoint.x) / mainCanvas.scaleFactor) + selectionWidth, selectionWidth);
            selectionLineBottom.sizeDelta = selectionLineTop.sizeDelta;
            selectionLineLeft.sizeDelta = new Vector2(selectionWidth, Mathf.Abs((selectionEndingPoint.y - selectionStartingPoint.y) / mainCanvas.scaleFactor) + selectionWidth);
            selectionLineRight.sizeDelta = selectionLineLeft.sizeDelta;
        }
        else
        {
            selectionLineLeft.sizeDelta = Vector2.zero;
            selectionLineRight.sizeDelta= Vector2.zero;
            selectionLineTop.sizeDelta = Vector2.zero;
            selectionLineBottom.sizeDelta = Vector2.zero;
        }
    }

    private void StartSelection(Vector2 mouseLocation)
    {
        selectionStartingPoint = mouseLocation;
        isSelecting = true;
    }

    private void FinishSelection()
    {
        isSelecting = false;

        Rect selectionBox = new Rect(selectionStartingPoint, selectionEndingPoint - selectionStartingPoint);

        foreach(SceneObject obj in selectedObjects)
        {
            obj.OnDeselection();
        }
        selectedObjects.Clear();

        foreach(SceneObject sceneObject in SceneObject.sceneObjects)
        {
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(sceneObject.transform.position);
            if (selectionBox.Contains(screenPosition, true))
            {
                selectedObjects.Add(sceneObject);
            }
        }

        if (selectedObjects.Count == 0) return;
        inputCommands.ExecuteCommand(this);
    }

    private void AbortSelection()
    {
        isSelecting = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            StartSelection(eventData.position);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            AbortSelection();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            FinishSelection();
        }
    }
}
