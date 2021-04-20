using UnityEngine;

//Universal Utility class created by Tomasz Galka.
namespace Tom.Utility
{
    public static class Utilities
    {
        public static readonly Vector3 kYRightAngle = Vector3.up * 90f;
        public static bool verboseMode = false;
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern int GetCurrentDevice();
        public static bool IsMousseOverUI
        {
            get
            {
                return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            }
        }

        public static bool IsPointerOverUIMobileFriendly() {
            UnityEngine.EventSystems.EventSystem currentEventSystem = UnityEngine.EventSystems.EventSystem.current;
            if (GameSettings.deviceType == DeviceType.Handheld) {
                foreach (Touch t in Input.touches) {
                    if (currentEventSystem.IsPointerOverGameObject(t.fingerId))
                        return true;
                }
                return false;
            }
            return currentEventSystem.IsPointerOverGameObject();
        }

        public static Vector3 GetPlatformSpecificPointerPosition() {
            if (GameSettings.deviceType == DeviceType.Handheld)
            {
                if (Input.touchCount > 0)
                    return Input.GetTouch(0).position;
                else
                    return Vector3.zero;
            }
            else
                return Input.mousePosition;
        }

        public static MonoBehaviour CreateSingleton(MonoBehaviour obj, MonoBehaviour value)
        {
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

        //public static string V(string s) => System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(s));

        public static void Print(object log, LogLevel level)
        {
            if (!verboseMode) return;
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace();
            System.Diagnostics.StackFrame[] stackFrames = trace.GetFrames();
            string caller = stackFrames[1].GetMethod().Name;
            for (int i = 0; i < stackFrames.Length; i++)
            {
                System.Type reflectedType = stackFrames[i].GetMethod().ReflectedType;
                if (builder.ToString().IndexOf(reflectedType.FullName) == -1 && reflectedType.FullName.Length < 32)
                    builder.Append($"{reflectedType.FullName}{(i == stackFrames.Length - 1 ? "" : ">>")}");
            }
            if (builder.ToString().EndsWith(">>"))
                builder = builder.Remove(builder.Length - 2, 2);

            switch (level)
            {
                case LogLevel.Warning:
                    Debug.LogWarning($"[{builder}:{caller}] -> {log}");
                    break;
                case LogLevel.Assert:
                    Debug.LogAssertion($"[{builder}:{caller}] -> {log}");
                    break;
                case LogLevel.Error:
                    Debug.LogError($"[{builder}:{caller}] -> {log}");
                    break;
                default:
                    Debug.Log($"[{builder}:{caller}] -> {log}");
                    break;
            }
        }

        public static Transform FindChildInChildrenByName(this Transform transform, string name, ChildSearchMode searchMode = ChildSearchMode.Equals)
        {
            name = name.ToLower();
            foreach (Transform t in transform.GetComponentsInChildren<Transform>())
            {
                string n = t.name.ToLower();
                if (!t) continue;
                switch (searchMode)
                {
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

        public static SkinnedMeshRenderer IsRig(this GameObject gameObject)
        {
            return gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
        }

#pragma warning disable CS0162
        public static DeviceType GetDeviceType() {
#if UNITY_WEBGL
                return (DeviceType)GetCurrentDevice();
#endif
            return Application.isMobilePlatform ? DeviceType.Handheld : DeviceType.Desktop;
        }

        public static void SetBlendMode(Material material, BlendMode blendMode) { //Taken from StandardShaderGUI.cs by Unity.
            int minRenderQueue = -1;
            int maxRenderQueue = 5000;
            int defaultRenderQueue = -1;
            
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    material.SetOverrideTag("RenderType", "");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    minRenderQueue = -1;
                    maxRenderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest - 1;
                    defaultRenderQueue = -1;
                    break;
                case BlendMode.Cutout:
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    minRenderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    maxRenderQueue = (int)UnityEngine.Rendering.RenderQueue.GeometryLast;
                    defaultRenderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    break;
                case BlendMode.Fade:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    minRenderQueue = (int)UnityEngine.Rendering.RenderQueue.GeometryLast + 1;
                    maxRenderQueue = (int)UnityEngine.Rendering.RenderQueue.Overlay - 1;
                    defaultRenderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Transparent:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    minRenderQueue = (int)UnityEngine.Rendering.RenderQueue.GeometryLast + 1;
                    maxRenderQueue = (int)UnityEngine.Rendering.RenderQueue.Overlay - 1;
                    defaultRenderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
            }
            if (material.renderQueue < minRenderQueue || material.renderQueue > maxRenderQueue)
                material.renderQueue = defaultRenderQueue;
        }

        public static Vector3 ReplaceXZ(this Vector3 vector, float newX, float newZ) => new Vector3(newX, vector.y, newZ);
        public static Vector3 ReplaceX(this Vector3 vector, float newX) => new Vector3(newX, vector.y, vector.z);
        public static Vector3 ReplaceY(this Vector3 vector, float newY) => new Vector3(vector.x, newY, vector.z);
        public static Vector3 ReplaceZ(this Vector3 vector, float newZ) => new Vector3(vector.x, vector.y, newZ);
        public static bool IsEven(this byte num) => num % 2 == 0;

        public static void ResetLocalPosition(this Transform transform) => transform.localPosition = Vector3.zero;
        public enum LogLevel : byte
        {
            Info,
            Warning,
            Assert,
            Error
        }
    }

    public enum BlendMode { 
        Opaque,
        Cutout,
        Fade,
        Transparent
    }

    public enum ChildSearchMode : byte
    {
        Equals,
        Contains,
        StartsWith
    }
}

public struct Cube {
    public float x;
    public float y;
    public float z;

    public float MaxX => x + width;
    public float MaxY => y + height;
    public float MaxZ => z + length;

    public float width;
    public float height;
    public float length;

    public Vector3 Position => new Vector3(x, y, z);
    public Vector3 Size => new Vector3(width, height, length);

    public Vector3[] Corners { get; private set; }

    //Simple AABB collision detection
    public bool Contains(Cube cube) {
        return MaxX > cube.x && x < cube.MaxX &&
            MaxY > cube.y && y < cube.MaxY &&
            MaxZ > cube.z && z < cube.MaxZ;
    }

    public Cube(float x, float y, float z, float width, float height, float length) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.width = width;
        this.height = height;
        this.length = length;
        Corners = new Vector3[8];
        Corners[0] = new Vector3(x, y, z);
        Corners[1] = new Vector3(MaxX, y, z);
        Corners[2] = new Vector3(MaxX, y, MaxZ);
        Corners[3] = new Vector3(x, y, MaxZ);
        Corners[4] = new Vector3(x, MaxY, z);
        Corners[5] = new Vector3(MaxX, MaxY, z);
        Corners[6] = new Vector3(MaxX, MaxY, MaxZ);
        Corners[7] = new Vector3(x, MaxY, MaxZ);
    }

    public Cube(Vector3 position, Vector3 size) {
        x = position.x;
        y = position.y;
        z = position.z;
        width = size.x;
        height = size.y;
        length = size.z;
        Corners = new Vector3[8];
        Corners[0] = new Vector3(x, y, z);
        Corners[1] = new Vector3(MaxX, y, z);
        Corners[2] = new Vector3(MaxX, y, MaxZ);
        Corners[3] = new Vector3(x, y, MaxZ);
        Corners[4] = new Vector3(x, MaxY, z);
        Corners[5] = new Vector3(MaxX, MaxY, z);
        Corners[6] = new Vector3(MaxX, MaxY, MaxZ);
        Corners[7] = new Vector3(x, MaxY, MaxZ);
    }

    public static Cube Copy(Cube toCopy) {
        return new Cube()
        {
            x = toCopy.x,
            y = toCopy.y,
            z = toCopy.z,
            width = toCopy.width,
            height = toCopy.height,
            length = toCopy.length,
            Corners = toCopy.Corners
        };
    }

    public override string ToString() => $"Cube: [X: {x} | Y: {y} | Z: {z} | W: {width} | H: {height} | L: {length}]";
}