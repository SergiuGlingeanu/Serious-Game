﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Tom.Utility;

[RequireComponent(typeof(BoxCollider))]
public class Structure : MonoBehaviour
{
    public StructureType type;
    public int id;
    public Size size;
    public Tile parentTile;
    public bool sellable;
    public GameObject accidentNotification;
    public bool IsPlaced { get; set; }
    public Stats stats;
    public bool Focused { get; set; }

    private void Awake()
    {
        id = GetInstanceID(); //Assign a unique ID per structure.
        parentTile = GridManager.kInvalidTile;
        if(accidentNotification)
            accidentNotification.SetActive(false);
    }

    public void CheckForAccident() {
        if (accidentNotification.activeInHierarchy) return;
        if (Random.Range(0f, 100f) >= stats.accidentRatePerAccidentInterval)
            accidentNotification.SetActive(true);
    }

    private void OnMouseUp()
    {
        //if (!IsPlaced /*|| !CanDrag*/) return;
        //Dragging = false;
        //if (GameSettings.currentGameMode == GameMode.BuyMode)
            //GridManager.instance.OnCurrentStructureDragEnded(this);
    }

    //private void OnMouseDrag() //Update for Touch
    //{
    //    if (Utilities.IsMouseOverUI) {
    //        Dragging = false;
    //        return;
    //    }
    //    if (!IsPlaced/* || !CanDrag*/) return;
    //    if (Input.mousePosition != _lastFrameMousePosition) {
    //        _lastFrameMousePosition = Input.mousePosition;
    //        Debug.Log("DRAG");
    //        Dragging = true;
    //    }
    //}

    //private void OnMouseDown()
    //{
    //    //if (Utilities.IsMouseOverUI) return;
    //    if (GameSettings.currentGameMode == GameMode.BuyMode)
    //        GridManager.instance.HookStructure(this);
    //}

    private void OnMouseDown()
    {
        Debug.Log($"MOUSEDOWN: {name}");
        if (Focused) return;
        if (GameSettings.currentGameMode == GameMode.BuyMode)
            GridManager.instance.HookStructure(this);
    }

    public void Flip() => transform.localEulerAngles += size.Flip() ? Utilities.kYRightAngle : -Utilities.kYRightAngle;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(transform.position, new Vector3(size.x, 0, size.y));
    }
#endif
}

[System.Serializable]
public struct Size { //UwU
    public static readonly Size zero = new Size() { x = 0, y = 0 };
    public static readonly Size one = new Size() { x = 1, y = 1 };
    public byte x;
    public byte y;
    public bool IsFlipped { get; private set; }

    public bool Flip() {
        byte oX = x;
        x = y;
        y = oX;
        return IsFlipped = !IsFlipped;
    }

    public static Size operator +(Size s, Size ss) => new Size() { x = (byte)(s.x + ss.x), y = (byte)(s.y + ss.y) };
    public static Size operator -(Size s, Size ss) => new Size() { x = (byte)(s.x - ss.x), y = (byte)(s.y - ss.y) };
    public static Size operator *(byte mult, Size s) => new Size() { x = (byte)(s.x * mult), y = (byte)(s.y * mult) };
    public static Size operator *(Size s, byte mult) => new Size() { x = (byte)(s.x * mult), y = (byte)(s.y * mult) };
    public static Size operator /(Size s, byte div) => new Size() { x = (byte)(s.x / div), y = (byte)(s.y / div) };
    public static Size operator /(byte div, Size s) => new Size() { x = (byte)(div / s.x), y = (byte)(div / s.y) };
}