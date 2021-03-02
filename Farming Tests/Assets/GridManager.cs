using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using 愛 = Tom.Utility.Utilities;
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
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    public byte width;
    public byte height;
    public float cellSize;

    [Header("Selection")]
    public float rayLength = 80f;
    public LayerMask raycastMask;
    public GameObject tileHighlightMarker;

    [Header("Placement")]
    public bool overrideBuildingY = false;
    public float buildingY = 1.2f;


    [Header("Debug")]
    [SerializeField] private bool _useMobileControls;
    public float _selectionSize = 12;
    public UnityEngine.UI.Text debugText;

    //Private Bois
    private Tile[,] _grid;
    private Camera _camera;
    private RaycastHit[] _hits;
    private Vector3 _predeterminedOffset;
    private MaterialPropertyBlock _selectionMaterialPropertyBlock;
    private Renderer _selectionMarkerRenderer;
    private Tile _currentTile;
    private Rect _gridRect;
    private List<Rect> _structureRects;
    public bool IsSelectionOutOfBounds
    {
        get
        {
            if (_currentTile.positionOnGrid == null) return false;
            return !_gridRect.Contains(new Vector2(_currentTile.positionOnGrid.x - ((_selectionSize / 2f) - 1),
                _currentTile.positionOnGrid.y - ((_selectionSize / 2f) - 1))) ||
                !_gridRect.Contains(new Vector2(_currentTile.positionOnGrid.x + (_selectionSize / 2f),
                _currentTile.positionOnGrid.y + (_selectionSize / 2f)));
        }
    }

    public bool Selecting => _markerFollower;
    public bool IsGridCreated => _grid != null;

    #region Init
    private void Start()
    {
        CreateInternalGrid(remakeGrid: false);
        _camera = Camera.main;
        _hits = new RaycastHit[1]; //Preallocate Raycast Hit array with one hit only
        GenerateSelectionHandle(1);
        GameSettings.deviceType = 愛.GetDeviceType();
        ToggleGameMode(true);
    }
    #endregion

    #region Main Update Loop
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
            ToggleGameMode();

        GameSettings.panBlocked = Selecting;
        if (!GameSettings.panBlocked) return;
        UpdateSelectionMarker();

        Vector2 pos = GameSettings.deviceType == DeviceType.Handheld ? Input.touchCount > 0 ?
            Input.GetTouch(0).position : Vector2.negativeInfinity : (Vector2)Input.mousePosition;
        if (pos == Vector2.negativeInfinity)
            return;

        if (GameSettings.deviceType != DeviceType.Handheld && !Input.GetMouseButton(0)) return; //Simulate Dragging Behaviour
        Ray ray = _camera.ScreenPointToRay(pos); //Reduce calls
        if (Physics.RaycastNonAlloc(ray, _hits, rayLength, raycastMask) > 0) //Preallocate Raycast Hit array to save memory
        {
            Debug.DrawRay(ray.origin, ray.direction * _hits[0].distance, Color.green);
            Tile t = FindTileAtPosition(_hits[0].point);
            if (!t.status)
            {
                tileHighlightMarker.SetActive(false);
                return;
            }

            tileHighlightMarker.transform.position = t.worldPosition + _predeterminedOffset;
            _currentTile = t;
            if (IsSelectionOutOfBounds)
                SetMarkerColor(kBadColor);
            else
                SetMarkerColor(kGoodColor);

            tileHighlightMarker.SetActive(true);
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.red);
            tileHighlightMarker.SetActive(false);
        }
    }
    #endregion

    #region Selection Marker
    private byte _lastItemId = 1; //Initiate values differently to guarantee execution
    private byte _currentItemId = 0;
    private Structure _markerFollower;

    public void HookStructure(Structure structure)
    {
        if (_markerFollower)
        {
            OnCurrentStructureDragEnded(_markerFollower);
            UnhookCurrentStructure();
        }
        structure.IsPlaced = true;
        if (!structure.InitialTileSet)
        {
            structure.InitialTileSet = true;
            structure.parentTile = _currentTile;
        }
        GenerateSelectionHandle(structure.size);
        _markerFollower = structure;
        //}
    }

    public void UnhookCurrentStructure() {
        _markerFollower = null;
        tileHighlightMarker.SetActive(false);
    }

    private void GenerateSelectionHandle(byte size = 1)
    {
        _selectionSize = size;
        GameObject highlight = tileHighlightMarker != null ? tileHighlightMarker : GameObject.CreatePrimitive(PrimitiveType.Cube);
        highlight.name = "Tile Highlight";
        highlight.transform.position = Vector3.zero;
        highlight.transform.localScale = new Vector3(size, 0.01f, size);
        _selectionMarkerRenderer = highlight.GetComponent<Renderer>();
        _selectionMaterialPropertyBlock = new MaterialPropertyBlock();
        _selectionMaterialPropertyBlock.SetVector("_MainTex_ST", new Vector4(size / 2f, size / 2f, 0, 0)); // Set the texture offset to fit the grid well.
        _selectionMarkerRenderer.SetPropertyBlock(_selectionMaterialPropertyBlock);
        SetMarkerColor(kGoodColor);
        highlight.SetActive(false);
        tileHighlightMarker = highlight;
        _predeterminedOffset = (_selectionSize % 2 == 0 ? new Vector3(_selectionSize / (_selectionSize * 2f), 0f, _selectionSize / (_selectionSize * 2f)) : Vector3.zero);
    }

    private void SetMarkerColor(Color color)
    {
        _selectionMarkerRenderer.GetPropertyBlock(_selectionMaterialPropertyBlock);
        _selectionMaterialPropertyBlock.SetColor("_Color", color);
        _selectionMarkerRenderer.SetPropertyBlock(_selectionMaterialPropertyBlock);
    }

    private void UpdateSelectionMarker()
    {
        if (_markerFollower.Dragging)
        {
            _markerFollower.transform.position = new Vector3(tileHighlightMarker.transform.position.x, _markerFollower.transform.position.y, tileHighlightMarker.transform.position.z);
        }
    }

    public void OnCurrentStructureDragEnded(Structure structure) {
        if (!IsSelectionOutOfBounds)
            _markerFollower.parentTile = _currentTile;
        if (structure.parentTile.status)
        {
            Vector3 tilePos = structure.parentTile.worldPosition + _predeterminedOffset;
            _markerFollower.transform.position = new Vector3(tilePos.x, _markerFollower.transform.position.y, tilePos.z);
            tileHighlightMarker.transform.position = tilePos;
            SetMarkerColor(kGoodColor);
            UnhookCurrentStructure();
        }
    }
    #endregion

    #region Grid Generation
    public void CreateInternalGrid(bool remakeGrid)
    {
        if (IsGridCreated && !remakeGrid) return;
        if (Instance == null) //Create Singleton
            Instance = this;
        else
            Destroy(gameObject);
        _grid = new Tile[width, height];
        _gridRect = new Rect(0, 0, width, height);
        GeneratePhysicalGrid();
    }

    private void GeneratePhysicalGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tile = new Tile()
                {
                    status = true,
                    occupiedSize = 0,
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
        Vector3 scale = new Vector3(1f, 0f, 1f) * cellSize;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(_grid[x, y].worldPosition, scale);
            }
        }
    }

    private void OnGUI()
    {
        if (_currentTile.positionOnGrid == null) return;
        GUIStyle s = new GUIStyle(GUI.skin.box);
        GUI.Label(new Rect(100, 100, 200, 200), $"X: {_currentTile.positionOnGrid.x} | Y: {_currentTile.positionOnGrid.y} | OOB: {IsSelectionOutOfBounds}", s);
    }
#endif
    #endregion

    #region Helper Methods
    public static float MapToRange(float value, float start1, float stop1, float start2, float stop2) => start2 + (stop2 - start2) * ((value - start1) / (stop1 - start1));
    public Tile FindTileAtPosition(Vector3 position)
    {
        Vector3 gridCoordinates = position - transform.position;
        gridCoordinates.x = Mathf.FloorToInt(gridCoordinates.x);
        gridCoordinates.y = Mathf.FloorToInt(gridCoordinates.z);
        if (gridCoordinates.x >= width || gridCoordinates.z >= height || gridCoordinates.x < 0 || gridCoordinates.z < 0) return kInvalidTile;
        return _grid[(int)gridCoordinates.x, (int)gridCoordinates.z];
    }

    public void ToggleGameMode(bool dontUpdate = false)
    {
        if(!dontUpdate)
            GameSettings.currentGameMode = (GameMode)((byte)GameSettings.currentGameMode + 1 > 1 ? 0 : 1);
        debugText.text = $"GameMode: {GameSettings.currentGameMode}";
    }
    #endregion
}
[System.Serializable]
public struct Tile
{
    public bool status;
    public byte occupiedSize;
    public Vector2Int positionOnGrid;
    public Vector3 worldPosition;
    public bool Occupied => occupiedSize > 0;
}
[System.Serializable]
public class PlacementEvent : UnityEvent<Structure> { }