using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tom.Utility;

[DefaultExecutionOrder(-30)]
public class IconHub : MonoBehaviour
{
    public static IconHub instance;
    private void Awake() => instance = Utilities.CreateSingleton(instance, this) as IconHub;

    [Header("References")]
    public Texture2D[] icons; //Enum order must be kept!

    [Header("Icon Creation")]
    public string fileName;
    public Camera iconCamera;
    public Transform pointOfFocus;

    public static Texture QueryStructureIcon(StructureType type) { //Relies on enum order
        if (!instance) return null;
        byte index = (byte)type;
        if (index < instance.icons.Length)
            return instance.icons[index];
        return null;
    }

    public void MoveIconCameraToPointOfFocus() {
        if (pointOfFocus)
            iconCamera.transform.position = iconCamera.transform.position.ReplaceX(pointOfFocus.position.x);
    }
}
