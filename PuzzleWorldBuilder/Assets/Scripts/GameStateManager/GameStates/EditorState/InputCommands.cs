using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputCommands : AbstractGameEditor, IPointerDownHandler, IPointerUpHandler
{
    /// <summary>
    /// Date: 03/08/2023, By: Yvar
    /// A Class that handles the input from the user, things such as the use of the dropdown menu's,
    /// but also like copy, paste, undo and redo using the keyboard
    /// </summary>
    [SerializeField] int maxUndoAmount = 10;
    CommandManager commandManager;
    public static Dictionary<KeyCode, ICommand> keyCommands = new Dictionary<KeyCode, ICommand>();
    public static List<SceneObject> selectedObjects = new List<SceneObject>();

    [SerializeField] DeleteObjectCommand deleteCommand;
    [SerializeField] SelectObjectCommand selectCommand;
    [SerializeField] DeSelectCommand deSelectCommand;

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

    private void Start()
    {
        commandManager = new CommandManager(maxUndoAmount);
    }

    public override void EditorUpdate()
    {
        CommandManagerUpdater();
        BasicKeys();
        SelectionProcess();
    }

    public CommandManager GetCommandManager()
    {
        return commandManager;
    }

    void CommandManagerUpdater()
    {
        foreach (KeyCode keyCode in keyCommands.Keys)
        {
            if (Input.GetKeyDown(keyCode))
            {
                ICommand command = keyCommands[keyCode];
                commandManager.ExecuteCommand(command);
            }
        }
    }

    void BasicKeys()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Undo();
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            Redo();
        }
#else
        if (Input.GetKeyDown(KeyCode.Z) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            Undo();
        }
        else if (Input.GetKeyDown(KeyCode.Y) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            Redo();
        }
#endif
    }

    void SelectionProcess()
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
            selectionLineRight.sizeDelta = Vector2.zero;
            selectionLineTop.sizeDelta = Vector2.zero;
            selectionLineBottom.sizeDelta = Vector2.zero;
        }
    }

    public static void AddKeyCommand(KeyCode keyCode, ICommand command)
    {
        if (!keyCommands.ContainsKey(keyCode))
        keyCommands.Add(keyCode, command);
    }

    public static void ChangeKeyCommandKey(KeyCode oldKeyCode, KeyCode newKeyCode)
    {
        ICommand temp;
        keyCommands.Remove(oldKeyCode, out temp);
        keyCommands.Add(newKeyCode, temp);
    }

    public static void RemoveKeyCommand(KeyCode keyCode)
    {
        keyCommands.Remove(keyCode);
    }

    public void Undo()
    {
        commandManager.UndoCommand();
    }

    public void Redo()
    {
        commandManager.RedoCommand();
    }

    public void Copy()
    {
        /// Copy some object, when making a copy of an object instantiate some invisible copy of it and put in on the "clipboard".
        /// When there is an object already on the "clipboard", delte this object, and replace it with the new copy.
        /// Upon copying a bigger group of objects, I was thinking of parenting it under one gameobject so the object creation class
        /// doesn't have to be rewritten
        /// 03/08/2023 Update: did that anyway
    }

    public void Paste()
    {
        /// Paste some object, before you can paste a copy has to exist, otherwise nothing happens.
        /// When pasting a copy, the creation of this copy has to go trough the class handling the creation of objects
        /// in the level editor so the creation of it can be undone.
    }

    public void Cut()
    {
        /// Cut some object, upon cutting the object should be removed trough the class handling the deletion of objects
        /// so this action can be undone. Also when cutting, an invisible copy of this object is made and put on the "clipboard".
    }

    public void Delete()
    {
        if (selectedObjects.Count > 0)
            commandManager.ExecuteCommand(deleteCommand);
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
        if (selectionBox.size.magnitude < 5)
        {
            RaycastHit hit;
            Vector3 camForward = mainCamera.ScreenToWorldPoint(new Vector3(selectionBox.center.x, selectionBox.center.y, mainCamera.nearClipPlane));
            Physics.Raycast(mainCamera.transform.position,
                camForward - mainCamera.transform.position,
                out hit);

            if (hit.collider != null)
            {
                SceneObject sceneObject = hit.collider.GetComponent<SceneObject>();
                if (sceneObject != null)
                {
                    foreach (SceneObject obj in selectedObjects)
                    {
                        obj.OnDeselection();
                    }
                    selectedObjects.Clear();
                    selectedObjects.Add(sceneObject);
                    commandManager.ExecuteCommand(selectCommand);
                }
                return;
            }
        }

        List<SceneObject> temp = new List<SceneObject>();
        foreach (SceneObject sceneObject in SceneObject.sceneObjects)
        {
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(sceneObject.transform.position);
            if (selectionBox.Contains(screenPosition, true))
            {
                temp.Add(sceneObject);
            }
        }
        
        if (temp.Count > 0)
        {
            foreach (SceneObject obj in selectedObjects)
            {
                obj.OnDeselection();
            }
            selectedObjects = temp;
            commandManager.ExecuteCommand(selectCommand);
        }
        else if (selectedObjects.Count > 0)
        {
            commandManager.ExecuteCommand(deSelectCommand);
        }
    }

    private void AbortSelection()
    {
        isSelecting = false;
        if (selectedObjects.Count == 0) return;
        else commandManager.ExecuteCommand(deSelectCommand);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            StartSelection(eventData.position);
        }
        else if (eventData.button == PointerEventData.InputButton.Right && isSelecting)
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Rect selectionBox = new Rect(selectionStartingPoint, selectionEndingPoint - selectionStartingPoint);
        Vector3 camForward = mainCamera.ScreenToWorldPoint(new Vector3(selectionBox.center.x, selectionBox.center.y, mainCamera.nearClipPlane));
        Gizmos.DrawLine(mainCamera.transform.position, mainCamera.transform.position + (camForward - mainCamera.transform.position) * 100);
    }
}