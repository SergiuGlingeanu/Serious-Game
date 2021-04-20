using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageEffect : MonoBehaviour
{
    public Material imageEffectMaterial;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (imageEffectMaterial)
            Graphics.Blit(source, destination, imageEffectMaterial);
        else
            Graphics.Blit(source, destination);
    }
}
