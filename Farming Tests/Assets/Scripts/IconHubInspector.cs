using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Tom.Utility;

[CustomEditor(typeof(IconHub))]
public class IconHubInspector : Editor
{
    public override void OnInspectorGUI()
    {
        IconHub hub = (IconHub)target;
        base.OnInspectorGUI();
        //if (!Application.isPlaying) return;

        if (hub.pointOfFocus)
        {
            if (GUILayout.Button("Update Camera Position"))
                hub.MoveIconCameraToPointOfFocus();
        }
        else
            return;

        if (!hub.iconCamera.targetTexture) return;
        if (GUILayout.Button("Capture Icon")) {
            RenderTexture oldView = RenderTexture.active;
            hub.iconCamera.Render();
            RenderTexture.active = hub.iconCamera.targetTexture;
            Texture2D iconImage = new Texture2D(RenderTexture.active.width, RenderTexture.active.height);
            iconImage.ReadPixels(new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height), 0, 0);
            iconImage.Apply();
            RenderTexture.active = oldView;

            try
            {
                byte[] iconBytes = iconImage.EncodeToPNG();
                string path = System.IO.Path.Combine(Application.dataPath, "Sprites/Icons", $"{(string.IsNullOrEmpty(hub.fileName) ? $"{System.DateTime.Now}.png" : $"{hub.fileName}.png")}");
                System.IO.File.WriteAllBytes(path, iconBytes);
                Debug.Log($"Written Icon File to [{path}].");
            }
            catch (System.Exception e) {
                Debug.LogError($"Failed to write Icon File: [{e.Message}]");
            }
            if (Application.isPlaying)
                Destroy(iconImage);
            else
                DestroyImmediate(iconImage);
        }
    }
}
