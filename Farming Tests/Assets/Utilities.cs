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
        public static bool IsMouseOverUI
        {
            get
            {
                return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            }
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

        public static Vector3 ReplaceXZ(this Vector3 vector, float newX, float newZ) => new Vector3(newX, vector.y, newZ);
        public static Vector3 ReplaceX(this Vector3 vector, float newX) => new Vector3(newX, vector.y, vector.z);
        public static Vector3 ReplaceY(this Vector3 vector, float newY) => new Vector3(vector.x, newY, vector.z);
        public static Vector3 ReplaceZ(this Vector3 vector, float newZ) => new Vector3(vector.x, vector.y, newZ);

        public static void ResetLocalPosition(this Transform transform) => transform.localPosition = Vector3.zero;
        public enum LogLevel : byte
        {
            Info,
            Warning,
            Assert,
            Error
        }
    }

    public enum ChildSearchMode : byte
    {
        Equals,
        Contains,
        StartsWith
    }

}