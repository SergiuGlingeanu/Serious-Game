using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static bool verboseMode = true;
    public static bool IsMouseOverUI {
        get {
            return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }
    }
    public static MonoBehaviour CreateSingleton(MonoBehaviour obj, MonoBehaviour value) {
        if (obj == null)
        {
            Print($"[CreateSingleton:{value.GetType().FullName}] -> Assigned new instance to Singleton.", LogLevel.Info);
            return value;
        }
        else
        {
            Print($"[CreateSingleton:{obj.GetType().FullName}] -> Singleton instance already exists ({obj.name}). '{value.name}' will be destroyed.", LogLevel.Warning);
            Object.Destroy(value);
        }
        return obj;
    }

    public static void Print(object log, LogLevel level) {
        if(verboseMode)
        switch (level) {
            case LogLevel.Warning:
                    Debug.LogWarning(log);
                break;
            case LogLevel.Assert:
                    Debug.LogAssertion(log);
                break;
            case LogLevel.Error:
                    Debug.LogError(log);
                break;
            default:
                    Debug.Log(log);
                break;
        }
    }

    public static Transform FindChildInChildrenByName(this Transform transform, string name, ChildSearchMode searchMode = ChildSearchMode.Equals) {
        name = name.ToLower();
        foreach (Transform t in transform.GetComponentsInChildren<Transform>()) {
            string n = t.name.ToLower();
            if (!t) continue;
            switch (searchMode) {
                case ChildSearchMode.Contains:
                    if (n.Contains(name))
                        return t;
                    break;
                case ChildSearchMode.StartsWith:
                    if (n.StartsWith(name))
                        return t;
                    break;
                default:
                    if (n.ToLower() == name)
                        return t;
                    break;
            }
        }
        return null;
    }

    public static SkinnedMeshRenderer IsRig(this GameObject gameObject) {
        return gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
    }

    public enum LogLevel : byte { 
        Info,
        Warning,
        Assert,
        Error
    }
}

public enum ChildSearchMode : byte {  
    Equals,
    Contains,
    StartsWith
}
