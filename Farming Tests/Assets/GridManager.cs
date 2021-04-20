using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tom.Utility;
using UnityEngine.Events;

//Grid-Based System using no constantly running nested loops! Maths is powerful UwU
[DefaultExecutionOrder(-10)] //Execute this script 10ms before all others
public class GridManager : MonoBehaviour
{
    public static readonly Color kGoodColor = Color.green;
    public static readonly Color kBadColor = Color.red;
    public static readonly Tile kInvalidTile = new Tile()
    {
        status = false
    };
    public static GridManager instance;

    [Header("Grid Settings")]
    public byte width;
    public byte height;
    public float cellSize;

    [Header("Selection")]
    public float rayLength = 80f;
    public LayerMask raycastMask;
    public LayerMask structureHandlingRaycastMask;
    public GameObject tileHighlightMarker;

    public Billboard focusMarker;
    public GameObject fenceBuilderPane;

    [Header("Placement")]
    public bool overrideBuildingY = false;
    public float buildingY = 1.2f;
    public Structure farmHouseStructure;

    [Header("Debug")]
    public Size _selectionSize;
    public UnityEngine.UI.Text debugText;
    public GameObject shopPanel;
    public GameObject sellButton;

    //Private Bois
    private Tile[,] _grid;
    private Camera _camera;
    private RaycastHit[] _hits;
    private Vector3 _predeterminedOffset;
    private MaterialPropertyBlock _selectionMaterialPropertyBlock;
    private Renderer _selectionMarkerRenderer;
    private Tile _currentTile;
    private Rect _gridRect;
    private ShopManager _shopManager;
    public List<Structure> BuiltStructures { get; private set; } = new List<Structure>();
    public bool IsStructureSelected => CurrentStructure;
    private void Awake() => BuiltStructures = new List<Structure>();
    public bool IsSelectionOutOfBounds => CheckIfOutOfBounds(_currentTile, _selectionSize);

    public bool Selecting => CurrentStructure;
    public bool IsGridCreated => _grid != null;
    public static bool Dragging { get; private set; }
    
    #region Init
    private void Start()
    {
        transform.root.localScale = transform.root.localScale.ReplaceXZ(width, height);

        _shopManager = GetComponent<ShopManager>();
        CreateInternalGrid(remakeGrid: false);
        _camera = Camera.main;
        _hits = new RaycastHit[1]; //Preallocate Raycast Hit array with one hit only
        GenerateSelectionHandle(Size.one);
        ToggleGameMode(true);

        //Set up Farm House roughly at the middle of the grid
        HookStructure(farmHouseStructure);
        farmHouseStructure.parentTile = _grid[width / 2, height / 2]; //Roughly middle
        tileHighlightMarker.transform.position = farmHouseStructure.parentTile.worldPosition + _predeterminedOffset;
        BuiltStructures.Add(farmHouseStructure);
        farmHouseStructure.currentTile = farmHouseStructure.parentTile;
        farmHouseStructure.Purchased = true;
        _currentTile = farmHouseStructure.parentTile;
        UpdateOccupiedSpaceForStructure(CurrentStructure);
        MoveCurrentStructureToMarker();
        UnhookCurrentStructure();
    }
    #endregion

    #region Main Update Loop
    private void Update()
    {
        //if (CurrentStructure && CurrentStructure.Focused) {
        //    if (GameSettings.deviceType == DeviceType.Handheld && Input.touchCount > 0)
        //    {
        //        Touch t = Input.GetTouch(0);
        //        if (t.phase == TouchPhase.Began)
        //        {
        //            Vector3 screenPos = _camera.WorldToScreenPoint(focusMarker.transform.position).ReplaceZ(0f);
        //            UnregisterIfOutOfRange(Vector3.Distance(new Vector3(t.position.x, t.position.y, 0f), screenPos));
        //        }
        //    }
        //    else
        //    {
        //        if (Input.GetMouseButtonDown(0)) {
        //            Vector3 screenPos = _camera.WorldToScreenPoint(focusMarker.transform.position).ReplaceZ(0f);
        //            UnregisterIfOutOfRange(Vector3.Distance(Input.mousePosition.ReplaceZ(0f), screenPos));
        //        }  
        //    }
        //}

        //if (Utilities.IsMouseOverUI) return;
        //Vector2 pos = Vector2.zero;
        //if (CurrentStructure && CurrentStructure.Focused)
        //{
        //    bool pressDetected = false;
        //    if (GameSettings.deviceType == DeviceType.Handheld && Input.touchCount > 0)
        //    {
        //        Touch t = Input.GetTouch(0);
        //        if (t.phase == TouchPhase.Began)
        //        {
        //            pos = t.position;
        //            pressDetected = true;
        //        }
        //    }
        //    else
        //    {
        //        if (Input.GetMouseButtonDown(0))
        //        {
        //            pos = Input.mousePosition;
        //            pressDetected = true;
        //        }
        //    }
        //    if (pressDetected)
        //    {
        //        if (Physics.Raycast(_camera.ScreenPointToRay(pos), out RaycastHit hit, rayLength, raycastMask))
        //        {
        //            if (!hit.transform.gameObject.CompareTag("DeselectArea")) return;
        //            if (IsStructureSelected)
        //                DeselectStructure();
        //        }
        //    }
        //    return;
        //}
    }
    #endregion

    #region Selection Marker
    public Structure CurrentStructure { get; private set; }

    public void ConfirmStructurePlacement() {
        if (_shopManager.FinalizePurchaseForCurrentStructure())
        {
            DeselectStructure();
            Dragging = false;
        }
    }

    public void AbortStructurePlacement() {
        if (!CurrentStructure) return;
        if (!CurrentStructure.Purchased && CurrentStructure.type != StructureType.FarmHouse)
            _shopManager.AbortPlacement(CurrentStructure);
        else
        {
            CurrentStructure.currentTile = CurrentStructure.lastTile;
            CurrentStructure.parentTile = CurrentStructure.lastTile;
            OnCurrentStructureDragEnded(true);
            DeselectStructure();
        }
        Dragging = false;
    }

    public void HookStructure(Structure structure)
    {
        if (CurrentStructure)
        {
            //OnCurrentStructureDragEnded();
            UnhookCurrentStructure();
        }
        if (!structure.IsPlaced)
            structure.IsPlaced = true;

        if (!structure.parentTile.status)
            structure.parentTile = _currentTile;
        GenerateSelectionHandle(structure.size);
        CurrentStructure = structure;
        TransferFocusMarker();
        structure.Focused = true;
        sellButton.SetActive(structure.sellable);
        tileHighlightMarker.SetActive(true);
        tileHighlightMarker.transform.position = tileHighlightMarker.transform.position.ReplaceXZ(CurrentStructure.transform.position.x, CurrentStructure.transform.position.z);
        UpdateOccupiedSpaceForStructure(CurrentStructure);
        if (structure.Purchased)
            structure.lastTile = structure.currentTile;

        Debug.Log("Fix");
        //}
    }

    private void TransferFocusMarker()
    {
        focusMarker.gameObject.SetActive(true);
        focusMarker.transform.ResetLocalPosition();
        UpdateFocusMarkerPosition();
    }

    private void UpdateFocusMarkerPosition()
    {
        focusMarker.transform.SetParent(CurrentStructure.transform);
        focusMarker.transform.ResetLocalPosition();
        focusMarker.transform.localPosition += focusMarker.transform.up * 1.2f;
    }

    public void UnhookCurrentStructure() {
        if(CurrentStructure)
            CurrentStructure.Focused = false;
        CurrentStructure = null;
        focusMarker.transform.SetParent(null);
        tileHighlightMarker.SetActive(false);
        focusMarker.gameObject.SetActive(false);
        GameSettings.panBlocked = false;
    }

    private void GenerateSelectionHandle(Size size, bool isEdit = false)
    {
        _selectionSize = size;
        GameObject highlight = tileHighlightMarker != null ? tileHighlightMarker : GameObject.CreatePrimitive(PrimitiveType.Cube);
        highlight.name = "Tile Highlight";
        if (!isEdit)
            highlight.transform.position = Vector3.zero + Vector3.up * 0.15f;
        highlight.transform.localScale = new Vector3(size.x, 0.01f, size.y);
        _selectionMarkerRenderer = highlight.GetComponent<Renderer>();
        _selectionMaterialPropertyBlock = new MaterialPropertyBlock();
        _selectionMaterialPropertyBlock.SetVector("_MainTex_ST", new Vector4(size.x / 2f, size.y / 2f, 0, 0)); // Set the texture offset to fit the grid well.
        _selectionMarkerRenderer.SetPropertyBlock(_selectionMaterialPropertyBlock);
        if (!isEdit)
        {
            SetMarkerColor(kGoodColor);
            highlight.SetActive(false);
            tileHighlightMarker = highlight;
        }

        float xOffset = _selectionSize.x % 2 == 0 ? _selectionSize.x / (_selectionSize.x * 2f) : 0f;
        float zOffset = _selectionSize.y % 2 == 0 ? _selectionSize.y / (_selectionSize.y * 2f) : 0f;
        _predeterminedOffset = new Vector3(xOffset, 0f, zOffset);
        //_predeterminedOffset = (((_selectionSize.x) / 2) % 2 == 0 ? new Vector3(_selectionSize.x / (_selectionSize.x * 2f), 0f, _selectionSize.y / (_selectionSize.y * 2f)) : Vector3.zero);
    }

    private Color _lastMarkerColor;

    private void SetMarkerColor(Color color)
    {
        if (_lastMarkerColor == color) return;
        _lastMarkerColor = color;
        _selectionMarkerRenderer.GetPropertyBlock(_selectionMaterialPropertyBlock);
        _selectionMaterialPropertyBlock.SetColor("_Color", color);
        _selectionMarkerRenderer.SetPropertyBlock(_selectionMaterialPropertyBlock);
    }

    private void MoveCurrentStructureToMarker() => CurrentStructure.transform.position = new Vector3(tileHighlightMarker.transform.position.x, CurrentStructure.transform.position.y, tileHighlightMarker.transform.position.z);

    public void OnCurrentStructureDragEnded(bool overridePlacementCheck = false) {
        Dragging = false;
        if (!CurrentStructure) return;
        bool incorrect = CheckForInvalidPlacement(CurrentStructure);
        if (!incorrect)
        {
            if (!overridePlacementCheck)
            {
                CurrentStructure.parentTile = _currentTile;
                CurrentStructure.currentTile = _currentTile;
            }
        }

        if (CurrentStructure.parentTile.status)
        {
            Vector3 tilePos = CurrentStructure.parentTile.worldPosition + _predeterminedOffset;
            CurrentStructure.transform.position = new Vector3(tilePos.x, CurrentStructure.transform.position.y, tilePos.z);
            tileHighlightMarker.transform.position = tilePos;
            _currentTile = CurrentStructure.parentTile;
            CurrentStructure.currentTile = CurrentStructure.parentTile;
        }
        UpdateOccupiedSpaceForStructure(CurrentStructure);
        incorrect = CheckForInvalidPlacement(CurrentStructure);
        SetMarkerColor(incorrect ? kBadColor : kGoodColor);
        //UnhookCurrentStructure();
    }

    private Vector2 _lastFramePosition;

    public void OnCurrentStructureDragged()
    {
        if (!CurrentStructure) return;
        GameSettings.panBlocked = Selecting;
        if (!GameSettings.panBlocked) return;
        Vector2 pos;
        pos = GameSettings.deviceType == DeviceType.Handheld ? Input.touchCount > 0 ?
        Input.GetTouch(0).position : Vector2.negativeInfinity : (Vector2)Input.mousePosition;
        if (pos == Vector2.negativeInfinity)
            return;

        Dragging = true;
        if (pos == _lastFramePosition) return; //Calls are reduced
        _lastFramePosition = pos;

        if (GameSettings.deviceType != DeviceType.Handheld && !Input.GetMouseButton(0)) return; //Simulate Dragging Behaviour
        Ray ray = _camera.ScreenPointToRay(pos); //Reduce calls

        if (Physics.RaycastNonAlloc(ray, _hits, rayLength, structureHandlingRaycastMask) > 0) //Preallocate Raycast Hit array to save memory
        {
#if UNITY_EDITOR
            Debug.DrawRay(ray.origin, ray.direction * _hits[0].distance, Color.green);
#endif
            Tile t = FindTileAtPosition(_hits[0].point + GameSettings.kDragPointDirection * GameSettings.kDragPointPositionOffset);
            if (!t.status)
                tileHighlightMarker.transform.position = tileHighlightMarker.transform.position.ReplaceXZ(_hits[0].point.x, _hits[0].point.z);
            else
            {
                tileHighlightMarker.transform.position = t.worldPosition + _predeterminedOffset;
                _currentTile = t;
                CurrentStructure.currentTile = t;
            }
            MoveCurrentStructureToMarker();
            UpdateOccupiedSpaceForStructure(CurrentStructure);

            SetMarkerColor(CheckForInvalidPlacement(CurrentStructure) ? kBadColor : kGoodColor);
            //UpdateFocusMarkerPosition();

            tileHighlightMarker.SetActive(true);
        }
        else
        {
#if UNITY_EDITOR
            Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.red);
#endif
            tileHighlightMarker.SetActive(false);
        }
    }
    #endregion

    #region Grid Generation
    public void CreateInternalGrid(bool remakeGrid)
    {
        if (IsGridCreated && !remakeGrid) return;
        instance = Tom.Utility.Utilities.CreateSingleton(instance, this) as GridManager;
        _grid = new Tile[width, height];
        _gridRect = new Rect(0, 0, width, height);
        GeneratePhysicalGrid();
    }

    private void GeneratePhysicalGrid() //This only runs once
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tile = new Tile()
                {
                    status = true,
                    positionOnGrid = new Vector2Int(x, y),
                    worldPosition = transform.position + new Vector3(x + cellSize / 2f, 0f, y + cellSize / 2f)
                };
                _grid[x, y] = tile;
            }
        }
    }
    #endregion

    #region Non-Compile-Time Methods
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!IsGridCreated) return;
        Vector3 scale = GameSettings.kDragPointDirection * cellSize;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (_grid[x, y] != _currentTile)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireCube(_grid[x, y].worldPosition, scale);
                }
                else
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireCube(_grid[x, y].worldPosition, Vector3.one * cellSize);
                }
                
            }
        }
        //int xSub = CurrentStructure.size.x.IsEven() ? 1 : 0;
        //int ySub = CurrentStructure.size.y.IsEven() ? 1 : 0;

        //for (int r = -((CurrentStructure.size.x / 2) - xSub); r < (CurrentStructure.size.x / 2) + 1; r++)
        //{
        //    for (int c = -((CurrentStructure.size.y / 2) - ySub); c < (CurrentStructure.size.y / 2) + 1; c++)
        //    {
        //        Gizmos.color = Color.blue;
        //        Gizmos.DrawWireCube(_grid[_currentTile.positionOnGrid.x + r, _currentTile.positionOnGrid.y + c].worldPosition, Vector3.one * cellSize);
        //    }
        //}
        //Vector3 pos = _grid[_currentTile.positionOnGrid.x, _currentTile.positionOnGrid.y].worldPosition + 
        //    new Vector3(CurrentStructure.size.x.IsEven() ? 0.5f : 0f, 0, CurrentStructure.size.y.IsEven() ? 0.5f : 0);
        //Vector3 size = new Vector3(CurrentStructure.size.x, 1, CurrentStructure.size.y);
        if (BuiltStructures == null) return;
        Gizmos.color = Color.blue;
        foreach (Structure structure in BuiltStructures)
            DrawAccurateGizmoCube(structure.occupiedSpace);
        //Gizmos.DrawWireCube(structure.occupiedSpace.Position, structure.occupiedSpace.Size);
    }
    //private void OnGUI()
    //{
    //    if (_currentTile.positionOnGrid == null) return;
    //    GUIStyle s = new GUIStyle(GUI.skin.box);
    //    GUI.Label(new Rect(0, Screen.height - 200, 200, 200), $"X: {_currentTile.positionOnGrid.x} | Y: {_currentTile.positionOnGrid.y}\n" +
    //        $"Tile Info: {_currentTile}", s);
    //}

    private void DrawAccurateGizmoCube(Cube cube) {
        for (int c = 1; c < 5; c++)
        {
            if (c < 4)
            {
                if (c == 3)
                    Gizmos.DrawLine(cube.Corners[c], cube.Corners[0]);
                Gizmos.DrawLine(cube.Corners[c - 1], cube.Corners[c]);
            }

            Gizmos.DrawLine(cube.Corners[c - 1], cube.Corners[(c - 1) + 4]);
        }
        for (int c = 1; c < 4; c++)
        {
            int cc = c + 4;
            if (c == 3)
                Gizmos.DrawLine(cube.Corners[cc], cube.Corners[4]);
            Gizmos.DrawLine(cube.Corners[cc - 1], cube.Corners[cc]);
        }
    }
#endif
    #endregion

    #region Helper Methods
    public static float MapToRange(float value, float start1, float stop1, float start2, float stop2) => start2 + (stop2 - start2) * ((value - start1) / (stop1 - start1));
    public Tile FindTileAtPosition(Vector3 position)
    {
        Vector3 gridCoordinates = position - transform.position;
        if (gridCoordinates.x >= width || gridCoordinates.z >= height || gridCoordinates.x < 0 || gridCoordinates.z < 0) return kInvalidTile;
        return _grid[Mathf.FloorToInt(gridCoordinates.x), Mathf.FloorToInt(gridCoordinates.z)];
    }

    public void ToggleGameMode(bool dontUpdate = false)
    {
        if (!dontUpdate)
            if ((GameSettings.currentGameMode = (GameMode)((byte)GameSettings.currentGameMode + 1 > 1 ? 0 : 1)) == GameMode.Regular)
            {
                _shopManager.VerifyStructureStatuses();
                if (CurrentStructure)
                    DeselectStructure(true);
                else
                    shopPanel.SetActive(false);
                Dragging = false;
            }
            else
                shopPanel.SetActive(true);

        debugText.text = $"GameMode: {GameSettings.currentGameMode}";
    }

    public void DeselectStructure(bool hideShop = false) {
        UnhookCurrentStructure();
        sellButton.SetActive(false);
        shopPanel.SetActive(!hideShop);
    }

    public void FlipCurrentStructure() {
        if (!CurrentStructure) return;
        UpdateOccupiedSpaceForStructure(CurrentStructure);
        Size tempSize = CurrentStructure.TestFlip();
        if (CheckIfOutOfBounds(_currentTile, tempSize) || CheckStructureForOverlapWithCustomSize(CurrentStructure, tempSize))
            return;
        CurrentStructure.Flip();
        UpdateOccupiedSpaceForStructure(CurrentStructure);
        
        //CurrentStructure.Flip();
        GenerateSelectionHandle(CurrentStructure.size, true);
        //focusMarker.transform.localEulerAngles = focusMarker.transform.localEulerAngles.ReplaceY(45f * (CurrentStructure.size.IsFlipped ? -1f : 1f));
        SetMarkerColor(kGoodColor);
    }

    private bool CheckIfOutOfBounds(Tile tile, Size size) {
        if (tile.positionOnGrid == null) return false;
        return !_gridRect.Contains(new Vector2(tile.positionOnGrid.x - ((size.x / 2f) - 1),
            tile.positionOnGrid.y - ((size.y / 2f) - 1))) ||
            !_gridRect.Contains(new Vector2(tile.positionOnGrid.x + (size.x / 2f),
            tile.positionOnGrid.y + (size.y / 2f)));
    }

    private void UpdateOccupiedSpaceForStructure(Structure structure) {
        if (!structure) return;
        structure.occupiedSpace = new Cube(structure.currentTile.worldPosition +
            new Vector3(
            (structure.size.x.IsEven() ? 0.5f : 0f) - structure.size.x / 2f,
            0,
            (structure.size.y.IsEven() ? 0.5f : 0f) - structure.size.y / 2f),
            new Vector3(structure.size.x, GameSettings.kDefaultOccupiedSpaceHeight, structure.size.y));
    }

    private bool CheckStructureForOverlap(Structure structure) {
        if (!structure) return false;
        foreach (Structure builtStructure in BuiltStructures)
        {
            if (builtStructure == structure) continue; //Skip itself. 
            if (structure.occupiedSpace.Contains(builtStructure.occupiedSpace))
                return true;
        }
        return false;
    }

    private bool CheckStructureForOverlapWithCustomSize(Structure structure, Size size) {
        if (!structure) return false;
        Cube cube = Cube.Copy(structure.occupiedSpace);
        cube.width = size.x;
        cube.length = size.y;
        foreach (Structure builtStructure in BuiltStructures)
        {
            if (builtStructure == structure) continue; //Skip itself. 
            if (cube.Contains(builtStructure.occupiedSpace))
                return true;
        }
        return false;
    }

    public bool CheckForInvalidPlacement(Structure structure)
    {
        if (!structure) return true;
        return CheckIfOutOfBounds(structure.currentTile, structure.size) || CheckStructureForOverlap(structure);
    }

    //private bool IsNeighbourOf(Tile tile) {
    //    if (!CurrentStructure) return false;
    //    for (int r = 0; r < CurrentStructure.size.x; r++) {
    //        for (int c = 0; c < CurrentStructure.size.y; c++) { 
    //            if(tile.positionOnGrid.x + r)
    //        }
    //    }
    //}
    #endregion
}
[System.Serializable]
public struct Tile
{
    public bool status;
    public Vector2Int positionOnGrid;
    public Vector3 worldPosition;

    public override string ToString()
    {
        return $"Grid Position: {positionOnGrid}\n" +
            $"World Position: {worldPosition}\n" +
            $"Status: {status}";
    }

    public static bool operator ==(Tile t, Tile comp) => t.positionOnGrid == comp.positionOnGrid;
    public static bool operator !=(Tile t, Tile comp) => !(t == comp);

    public override bool Equals(object obj) => base.Equals(obj);

    public override int GetHashCode() => positionOnGrid.x ^ positionOnGrid.y;
}

public enum Direction { 
    Forward,
    Backwards,
    Left,
    Right
}

[System.Serializable]
public class PlacementEvent : UnityEvent<Structure> { }