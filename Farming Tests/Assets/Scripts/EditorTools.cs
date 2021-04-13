using UnityEditor;

#pragma warning disable IDE0051
public class EditorTools : Editor
{
#if UNITY_WEBGL
    [MenuItem("Editor Tools/WebGL/Enable ASM")]
    private static void EnableWebGLASMCompilation()
    {
        if (PlayerSettings.WebGL.linkerTarget == WebGLLinkerTarget.Wasm)
        {
            Tom.Utility.Utilities.Print("ASM Compilation enabled.", Tom.Utility.Utilities.LogLevel.Info);
            Tom.Utility.Utilities.Print("ASM Compilation is deprecated. Only use this if compatibility with iOS devices is poor!", Tom.Utility.Utilities.LogLevel.Warning);
            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Both;
        }
        else
            Tom.Utility.Utilities.Print("ASM Compilation is already enabled!", Tom.Utility.Utilities.LogLevel.Info);
    }

    [MenuItem("Editor Tools/WebGL/Disable ASM")]
    private static void DisableWebGLASMCompilation()
    {
        if (PlayerSettings.WebGL.linkerTarget != WebGLLinkerTarget.Wasm)
        {
            Tom.Utility.Utilities.Print("ASM Compilation disabled.", Tom.Utility.Utilities.LogLevel.Info);
            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        }
        else
            Tom.Utility.Utilities.Print("ASM Compilation is not enabled!", Tom.Utility.Utilities.LogLevel.Info);
    }
#endif
}
