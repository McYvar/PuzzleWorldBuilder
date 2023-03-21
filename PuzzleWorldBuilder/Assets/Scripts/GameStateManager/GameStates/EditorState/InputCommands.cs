using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputCommands : AbstractGameEditor, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    /// <summary>
    /// Date: 03/08/2023, By: Yvar
    /// A Class that handles the input from the user, things such as the use of the dropdown menu's,
    /// but also like copy, paste, undo and redo using the keyboard
    /// 
    /// Update: 03/09/23
    /// Changed most things related to clicking under this script for the sake of organization
    /// </summary>

    [SerializeField] int maxUndoAmount = 10;
    CommandManager commandManager;
    public static Dictionary<KeyCode, ICommand> keyCommands = new Dictionary<KeyCode, ICommand>();
    public static List<SceneObject> selectedObjects = new List<SceneObject>();

    [SerializeField] DeleteObjectCommand deleteCommand;
    [SerializeField] SelectObjectCommand selectCommand;
    [SerializeField] DeselectObjectCommand deselectCommand;
    [SerializeField] CopyObjectCommand copyCommand;
    [SerializeField] PasteObjectCommand pasteCommand;
    [SerializeField] CutObjectCommand cutCommand;
    [SerializeField] OverrideSelectObjectCommand overrideSelectCommand;
    [SerializeField] FlipSelectionObjectCommand flipSelectCommand;

    [SerializeField] Canvas mainCanvas;
    [SerializeField] RectTransform selectionLineTop;
    [SerializeField] RectTransform selectionLineBottom;
    [SerializeField] RectTransform selectionLineLeft;
    [SerializeField] RectTransform selectionLineRight;
    [SerializeField] float selectionWidth;

    [SerializeField] Camera mainCamera;

    [SerializeField] Camera overlayCamera;
    [SerializeField] LayerMask overlayLayer;
    MoveToolArrow currentMoveToolArrow;

    Vector2 selectionStartingPoint;
    Vector2 selectionEndingPoint;
    bool isSelecting;

    [SerializeField] GameObject movementToolObject;
    [SerializeField] float movementToolDistance;
    Vector3 centrePoint;

    public override void EditorAwake()
    {
        commandManager = new CommandManager(maxUndoAmount);
    }

    public override void EditorStart()
    {
    }

    public override void EditorUpdate()
    {
        CommandManagerUpdater();
        BasicKeys();
        SelectionProcess();
        MovementTool();

        if (Input.GetKeyDown(KeyCode.D))
        {
            for (int i = 0; i < selectedObjects.Count; i++)
            {
                Debug.Log(i + ": " + selectedObjects[i].name);
            }
            if (selectedObjects.Count == 0)
            {
                Debug.Log("list is empty!");
            }
        }
    }

    #region CommandManager
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
    #endregion

    #region Key commands
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
        else if (Input.GetKeyDown(KeyCode.C))
        {
            Copy();
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            Paste();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            Cut();
        }
        else if (Input.GetKeyDown(KeyCode.Delete))
        {
            Delete();
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
        else if (Input.GetKeyDown(KeyCode.C) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            Copy();
        }
        else if (Input.GetKeyDown(KeyCode.V) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            Paste();
        }
        else if (Input.GetKeyDown(KeyCode.X) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            Cut();
        }
        else if (Input.GetKeyDown(KeyCode.Delete) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            Delete();
        }
#endif
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
    #endregion

    #region Standard edit buttons
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

        if (selectedObjects.Count > 0)
            commandManager.ExecuteCommand(copyCommand);
    }

    public void Paste()
    {
        /// Paste some object, before you can paste a copy has to exist, otherwise nothing happens.
        /// When pasting a copy, the creation of this copy has to go trough the class handling the creation of objects
        /// in the level editor so the creation of it can be undone.
        if (ClipBoard.clipboard.Count > 0)
            commandManager.ExecuteCommand(pasteCommand);
    }

    public void Cut()
    {
        /// Cut some object, upon cutting the object should be removed trough the class handling the deletion of objects
        /// so this action can be undone. Also when cutting, an invisible copy of this object is made and put on the "clipboard".
        if (selectedObjects.Count > 0)
            commandManager.ExecuteCommand(cutCommand);
    }

    public void Delete()
    {
        if (selectedObjects.Count > 0)
            commandManager.ExecuteCommand(deleteCommand);
    }
    #endregion

    #region Selection
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

    bool FoundMoveToolArrow(Vector2 mouseLocation)
    {
        RaycastHit hit;
        Vector3 camForward = overlayCamera.ScreenToWorldPoint(new Vector3(mouseLocation.x, mouseLocation.y, overlayCamera.nearClipPlane));
        Physics.Raycast(overlayCamera.transform.position,
            camForward - overlayCamera.transform.position,
            out hit,
            Mathf.Infinity,
            overlayLayer);

        if (hit.collider != null)
        {

            MoveToolArrow arrow = hit.collider.GetComponent<MoveToolArrow>();
            if (arrow != null)
            {
                currentMoveToolArrow = arrow;
                return true;
            }
        }
        return false;
    }
    void StartSelection(Vector2 mouseLocation)
    {
        selectionStartingPoint = mouseLocation;
        isSelecting = true;
    }

    void FinishSelection()
    {
        isSelecting = false;

        Rect selectionBox = new Rect(selectionStartingPoint, selectionEndingPoint - selectionStartingPoint);

        List<SceneObject> temp = new List<SceneObject>();
        if (selectionBox.size.magnitude < 5)
        {
            RaycastHit hit;
            Vector3 camForward = mainCamera.ScreenToWorldPoint(new Vector3(selectionBox.center.x, selectionBox.center.y, mainCamera.nearClipPlane));
            Physics.Raycast(mainCamera.transform.position,
                camForward - mainCamera.transform.position,
                out hit);

            if (hit.collider != null)
            {
                // means we hit only one object
                SceneObject sceneObject = hit.collider.GetComponent<SceneObject>();
                if (sceneObject != null)
                {
                    temp.Add(sceneObject);
                }
            }
        }
        else
        {
            foreach (SceneObject sceneObject in SceneObject.sceneObjects)
            {
                if (sceneObject == null) continue;
                Vector3 screenPosition = mainCamera.WorldToScreenPoint(sceneObject.transform.position);
                if (selectionBox.Contains(screenPosition, true))
                {
                    temp.Add(sceneObject);
                }
            }
        }

        if (temp.Count == 0)
        {
            if (selectedObjects.Count > 0)
            {
                deselectCommand.InitializePreDeselected(selectedObjects);
                commandManager.ExecuteCommand(deselectCommand);
            }
            return;
        }

        // case: shift is pressed
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            flipSelectCommand.InitializePreSelected(selectedObjects, temp);
            commandManager.ExecuteCommand(flipSelectCommand);
        }
        // case: cntl is pressed or nothing is selected
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || selectedObjects.Count == 0)
        {
            if (selectCommand.InitializePreSelected(selectedObjects, temp) > 0)
                commandManager.ExecuteCommand(selectCommand);
        }
        // case: nothing is pressed and at least one object is selected while selecting another object
        else if (selectedObjects.Count > 0)
        {
            // if the lists contain the same size and elements, we do nothing
            if (selectedObjects.Count == temp.Count)
            {
                bool contains = true;
                foreach (SceneObject sceneObject in selectedObjects)
                {
                    if (temp.Contains(sceneObject)) continue;
                    else
                    {
                        contains = false;
                    }
                }
                if (contains) return;
            }
            overrideSelectCommand.InitializeSelected(selectedObjects, temp);
            commandManager.ExecuteCommand(overrideSelectCommand);
        }
    }

    private void AbortSelection()
    {
        isSelecting = false;
        if (selectedObjects.Count > 0)
        {
            deselectCommand.InitializePreDeselected(selectedObjects);
            commandManager.ExecuteCommand(deselectCommand);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (FoundMoveToolArrow(eventData.position))
            {
                currentMoveToolArrow?.MouseDown(movementToolDistance, centrePoint);
            }
            else
            {
                StartSelection(eventData.position);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right && isSelecting)
        {
            AbortSelection();
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        currentMoveToolArrow?.MouseMove();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        currentMoveToolArrow?.MouseUp();
        currentMoveToolArrow = null;

        if (eventData.button == PointerEventData.InputButton.Left && isSelecting)
        {
            FinishSelection();
        }
    }
    #endregion

    #region MovementTool
    void MovementTool()
    {
        if (selectedObjects.Count > 0)
        {
            // if a movetoolarrow is selected, ignore these options
            if (currentMoveToolArrow != null) return;

            // if there are objects selected, show the movement tool
            if (!movementToolObject.activeInHierarchy) movementToolObject.SetActive(true);
            centrePoint = Vector3.zero;
            foreach (var obj in selectedObjects)
            {
                centrePoint += obj.transform.position;
            }
            centrePoint /= selectedObjects.Count;

            /// Just a mention to whoever reads this note... after writing this line of code I was convinced I was a mathmatical genius for a moment :o
            /// Edit: You should take a look at the MoveToolArrow class
            float angle = Vector3.Angle(mainCamera.transform.forward, centrePoint - mainCamera.transform.position);
            movementToolObject.transform.position = mainCamera.transform.position + (centrePoint - mainCamera.transform.position).normalized * (movementToolDistance / Mathf.Cos(angle * Mathf.Deg2Rad));

            // each seperate arrow has to get a mousedown function
        }
        else
        {
            // hide when there are no more objects selected
            if (movementToolObject.activeInHierarchy) movementToolObject.SetActive(false);
        }
    }
    #endregion

    /*
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Rect selectionBox = new Rect(selectionStartingPoint, selectionEndingPoint - selectionStartingPoint);
        Vector3 camForward = mainCamera.ScreenToWorldPoint(new Vector3(selectionBox.center.x, selectionBox.center.y, mainCamera.nearClipPlane));
        Gizmos.DrawLine(mainCamera.transform.position, mainCamera.transform.position + (camForward - mainCamera.transform.position) * 100);
    }
    */
}