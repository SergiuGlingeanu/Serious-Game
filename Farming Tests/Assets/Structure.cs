using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using 愛 = Tom.Utility.Utilities;

public class Structure : MonoBehaviour
{
    public int id;
    public byte size;
    public Tile parentTile;
    public bool IsPlaced { get; set; }
    public bool InitialTileSet { get; set; }
    public bool Dragging { get; private set; }

    private void Awake()
    {
        id = GetInstanceID(); //Assign a unique ID per structure.
    }

    private void OnMouseUp()
    {
        if (!IsPlaced) return;
        Dragging = false;
        if (GameSettings.currentGameMode == GameMode.BuyMode)
            GridManager.Instance.OnCurrentStructureDragEnded(this);
    }

    private void OnMouseDrag()
    {
        if (!IsPlaced) return;
        Dragging = true;
    }

    private void OnMouseDown()
    {
        if (GameSettings.currentGameMode == GameMode.BuyMode) {
            GridManager.Instance.HookStructure(this);
        }
    }
}
