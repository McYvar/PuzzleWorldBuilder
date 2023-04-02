using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class InputCommands : AbstractGameEditor, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler, IScrollHandler
{
    /// <summary>
    /// Date: 03/08/2023, By: Yvar
    /// A Class that handles the input from the user, things such as the use of the dropdown menu's,
    /// but also like copy, paste, undo and redo using the keyboard
    /// 
    /// Update: 03/09/23
    /// Changed most things related to clicking under this script for the sake of organization
    /// </summary>

    // command manager
    [SerializeField] int maxUndoAmount = 10;
    CommandManager commandManager;
    public static Dictionary<KeyCode, ICommand> keyCommands = new Dictionary<KeyCode, ICommand>();

    // all object commands
    [SerializeField] DeleteObjectCommand deleteCommand;
    [SerializeField] SelectObjectCommand selectCommand;
    [SerializeField] DeselectObjectCommand deselectCommand;
    [SerializeField] CopyObjectCommand copyCommand;
    [SerializeField] PasteObjectCommand pasteCommand;
    [SerializeField] CutObjectCommand cutCommand;
    [SerializeField] OverrideSelectObjectCommand overrideSelectCommand;
    [SerializeField] FlipSelectionObjectCommand flipSelectCommand;
    [SerializeField] TranslateObjectCommand translateCommand;
    [SerializeField] RemoveSelectionObjectCommand removeSelectionCommand;
    [SerializeField] DuplicateObjectCommand duplicateCommand;

    // all grid specific commands
    [SerializeField] CreateGridTileCommand createGridTileCommand;

    // canvas related
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
    bool isSelecting = false;

    // movement tool
    [SerializeField] GameObject movementToolObject;
    [SerializeField] GameObject mtUp;
    [SerializeField] GameObject mtForward;
    [SerializeField] GameObject mtRight;
    [SerializeField] GameObject mtCentreSphere;
    [SerializeField] float movementToolDistance;
    Vector3 centrePoint;

    // pivot transform
    [SerializeField] Transform camerasPivot;
    Vector3 smoothPivot;
    Vector3 smoothDampRef = Vector3.zero;
    bool doSmooth;

    bool isTranslatingPivot = false;
    float minTranslateAmp = 0;
    [SerializeField, Range(0.0f, 0.2f)] float maxTranslateAmp;
    float translateAmp;

    bool isRotatingPivot = false;
    [SerializeField, Range(0.0f, 1.0f)] float rotateAmp;

    float cameraOffset;
    [SerializeField, Range(0.0f, 50.0f)] float scrollDistBase;
    float scrollAmp;

    bool doRelativeSnap;
    bool doGridSnap;

    public static List<SceneObject> selectedObjects = new List<SceneObject>();

    [SerializeField] GameObject floatingObjectsMenu;

    protected override void OnEnable()
    {
        base.OnEnable();
        cameraOffset = (mainCamera.transform.position - camerasPivot.position).magnitude;
        smoothPivot = camerasPivot.position;
        Zoom(0);
    }

    public override void EditorAwake()
    {
        commandManager = new CommandManager(maxUndoAmount);
    }

    public override void EditorStart()
    {
        doSmooth = true;
    }

    public override void EditorUpdate()
    {
        CommandManagerUpdater();
        BasicKeys();
        SelectionProcess();
        MovementTool();

        if (doSmooth)
        {
            camerasPivot.position = Vector3.SmoothDamp(camerasPivot.position, smoothPivot, ref smoothDampRef, scrollAmp);
            if (Vector3.Distance(camerasPivot.position, smoothPivot) < 0.1f) doSmooth = false;
        }
        else
        {
            smoothPivot = camerasPivot.position;
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
        else if (Input.GetKeyDown(KeyCode.C) && !Input.GetKey(KeyCode.LeftShift))
        {
            Copy();
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            Paste();
        }
        else if (Input.GetKeyDown(KeyCode.X) && !Input.GetKey(KeyCode.LeftShift))
        {
            Cut();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            Duplicate();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            SelectAllTerrainObjects();
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.X))
            ToggleGridSnap();

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.C))
            ToggleRelativeSnap();

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
        else if (Input.GetKeyDown(KeyCode.D) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            Duplicate();
        }
        else if (Input.GetKeyDown(KeyCode.A) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            SelectAllTerrainObjects();
        }
        if (Input.GetKeyDown(KeyCode.X) && !(Input.GetKey(KeyCode.LeftControl)))
            ToggleGridSnap();

        if (Input.GetKeyDown(KeyCode.C) && !(Input.GetKey(KeyCode.LeftControl)))
            ToggleRelativeSnap();
#endif
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (selectedObjects.Count > 0)
            {
                doSmooth = true;
                SetPivot(centrePoint);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Delete))
        {
            Delete();
        }
    }

    void ToggleGridSnap()
    {
        // later add visual
        doGridSnap = !doGridSnap;
    }

    void ToggleRelativeSnap()
    {
        // later add visual
        doRelativeSnap = !doRelativeSnap;
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
            if (selectedObjects[0] as GridObject) return;
            else commandManager.ExecuteCommand(copyCommand);
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
            if (selectedObjects[0] as GridObject) return;
            else commandManager.ExecuteCommand(cutCommand);
    }

    public void Delete()
    {
        if (selectedObjects.Count > 0)
            commandManager.ExecuteCommand(deleteCommand);
    }

    public void Duplicate()
    {
        if (selectedObjects.Count > 0)
            if (selectedObjects[0] as GridObject) return;
            else commandManager.ExecuteCommand(duplicateCommand);
    }

    public void SelectAllTerrainObjects()
    {
        List<SceneObject> terrainObjects = new List<SceneObject>();
        terrainObjects.AddRange(TerrainObject.terrainObjects);
        selectCommand.InitializePreSelected(selectedObjects, terrainObjects);
        commandManager.ExecuteCommand(selectCommand);
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
        List<GridObject> gridObjectsToCreate = new List<GridObject>();
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
                // also we first search for terrain objects
                TerrainObject terrainObject = hit.collider.GetComponent<TerrainObject>();
                if (terrainObject != null)
                {
                    temp.Add(terrainObject);
                }

                // then we seek for a single gridobject
                if (temp.Count == 0)
                {
                    GridObject gridObject = hit.collider.GetComponent<GridObject>();
                    if (gridObject != null)
                    {
                        if (!gridObject.isCreated)
                            gridObjectsToCreate.Add(gridObject);
                        else temp.Add(gridObject);
                    }
                }
            }
        }
        else
        {
            // here we seek for multiple terrain objects
            foreach (TerrainObject terrainObject in TerrainObject.terrainObjects)
            {
                if (terrainObject == null) continue;
                Vector3 screenPosition = mainCamera.WorldToScreenPoint(terrainObject.transform.position);
                if (selectionBox.Contains(screenPosition, true))
                {
                    temp.Add(terrainObject);
                }
            }

            // if the list size is 0 then we look for gridobjects
            if (temp.Count == 0)
            {
                foreach (GridObject gridObject in GridObject.gridObjects)
                {
                    if (gridObject == null) continue;
                    Vector3 screenPosition = mainCamera.WorldToScreenPoint(gridObject.transform.position);
                    if (selectionBox.Contains(screenPosition, true))
                    {
                        if (!gridObject.isCreated)
                            gridObjectsToCreate.Add(gridObject);
                        else temp.Add(gridObject);
                    }
                }
            }
        }

        if (temp.Count == 0 && gridObjectsToCreate.Count == 0)
        {
            if (selectedObjects.Count > 0)
            {
                deselectCommand.InitializePreDeselected(selectedObjects);
                commandManager.ExecuteCommand(deselectCommand);
            }
            return;
        }
        // case: both sift and control are pressed
        if ((Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl)) ||
            (Input.GetKey(KeyCode.RightShift) && Input.GetKey(KeyCode.RightControl)) ||
            (Input.GetKey(KeyCode.RightShift) && Input.GetKey(KeyCode.LeftControl)) ||
            (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.RightControl)))
        {
            flipSelectCommand.InitializePreSelected(selectedObjects, temp);
            commandManager.ExecuteCommand(flipSelectCommand);
        }
        // case: cntl is pressed
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if (selectedObjects.Count > 0)
            {
                removeSelectionCommand.InitializePreSelected(selectedObjects, temp);
                commandManager.ExecuteCommand(removeSelectionCommand);
            }
            return;
        }
        // case: shift is pressed or nothing is selected
        else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || selectedObjects.Count == 0)
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
                foreach (SceneObject sceneObjects in selectedObjects)
                {
                    if (temp.Contains(sceneObjects)) continue;
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

        if (gridObjectsToCreate.Count > 0)
        {
            createGridTileCommand.InitializeTiles(gridObjectsToCreate.ToArray());
            commandManager.ExecuteCommand(createGridTileCommand);
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
    #endregion

    #region MovementTool
    void MovementTool()
    {
        if (selectedObjects.Count > 0)
        {
            // if a movetoolarrow is selected, ignore these options
            if (currentMoveToolArrow != null) return;

            // if there are objects selected, show the movement tool
            if (selectedObjects[0] as TerrainObject)
            {
                mtUp.SetActive(true);
                mtForward.SetActive(true);
                mtRight.SetActive(true);
                mtCentreSphere.SetActive(true);
            }
            else if (selectedObjects[0] as GridObject)
            {
                mtUp.SetActive(true);
            }

            centrePoint = Vector3.zero;
            foreach (SceneObject obj in selectedObjects)
            {
                centrePoint += obj.transform.position;
            }
            centrePoint /= selectedObjects.Count;

            /// Just a mention to whoever reads this note... after writing this line of code I was convinced I was a mathmatical genius for a moment :o
            /// Edit: You should take a look at the MoveToolArrow class
            float angle = Vector3.Angle(mainCamera.transform.forward, centrePoint - mainCamera.transform.position);
            movementToolObject.transform.position = mainCamera.transform.position + (centrePoint - mainCamera.transform.position).normalized * (movementToolDistance / Mathf.Cos(angle * Mathf.Deg2Rad));
        }
        else
        {
            // hide the tool
            mtUp.SetActive(false);
            mtForward.SetActive(false);
            mtRight.SetActive(false);
            mtCentreSphere.SetActive(false);
        }
    }
    #endregion

    #region InputEvents
    public void OnPointerDown(PointerEventData eventData)
    {
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                isRotatingPivot = true;
                isTranslatingPivot = false;
            }
        }
        else
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // check if we hit a movement tool
                if (FoundMoveToolArrow(eventData.position))
                {
                    currentMoveToolArrow?.MouseDown(movementToolDistance, centrePoint, doRelativeSnap, doGridSnap, 1);
                }
                // if we only click down on the background
                else
                {
                    StartSelection(eventData.position);
                }
            }

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (isSelecting) AbortSelection();
            }
        }

        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            doSmooth = false;
            isRotatingPivot = false;
            isTranslatingPivot = true;
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (isRotatingPivot && currentMoveToolArrow == null)
        {
            RotatePivot(eventData.delta);
        }
        else if (isTranslatingPivot && currentMoveToolArrow == null)
        {
            TranslatePivot(eventData.delta);
        }
        else if (!isTranslatingPivot)
        {
            currentMoveToolArrow?.MouseMove();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (currentMoveToolArrow != null)
        {
            translateCommand.InitializeNewPostition(currentMoveToolArrow.MouseUp());
            commandManager.ExecuteCommand(translateCommand);
            currentMoveToolArrow = null;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (isSelecting) FinishSelection();
            if (isRotatingPivot) isRotatingPivot = false;
        }

        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            if (isTranslatingPivot) isTranslatingPivot = false;
        }
    }

    public void OnScroll(PointerEventData eventData)
    {
        Zoom(eventData.scrollDelta.y);
    }
    #endregion

    #region Pivot
    void RotatePivot(Vector2 mouseDelta)
    {
        float xRot = camerasPivot.localEulerAngles.x;
        if (xRot > 90 && xRot < 360)
        {
            xRot -= 360;
        }
        camerasPivot.localRotation = Quaternion.Euler(
            Mathf.Clamp(xRot + -mouseDelta.y * rotateAmp, -90, 90),
            camerasPivot.localEulerAngles.y + mouseDelta.x * rotateAmp,
            0);
    }

    void SetPivot(Vector3 newLocation)
    {
        smoothPivot = newLocation;
    }

    void TranslatePivot(Vector2 mouseDelta)
    {
        camerasPivot.position += (camerasPivot.right * -mouseDelta.x + camerasPivot.up * -mouseDelta.y) * translateAmp;
    }

    void Zoom(float scrollDelta)
    {
        cameraOffset -= scrollDelta * scrollAmp;
        if (cameraOffset < 1) cameraOffset = 1;
        mainCamera.transform.position = camerasPivot.position + -camerasPivot.forward * cameraOffset;
        scrollAmp = Mathf.InverseLerp(1, scrollDistBase, Vector3.Distance(mainCamera.transform.position, camerasPivot.position));
        translateAmp = Mathf.Lerp(minTranslateAmp, maxTranslateAmp, scrollAmp);
    }
    #endregion

    public void ResetTool()
    {
        commandManager.ClearAll();
        selectedObjects.Clear();
        floatingObjectsMenu.transform.localPosition = new Vector3((-Screen.width / 2) + 32, (Screen.height / 2) - 24, 0);
    }
}