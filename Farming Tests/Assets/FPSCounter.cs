using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class FPSCounter : MonoBehaviour
{
    private Text _text;
    private float _timer = 1f;
    private void Start() => _text = GetComponent<Text>();
    void Update()
    {
        if (_timer >= 1f)
        {
            _timer = 0f;
            _text.text = $"FPS: {(int)(1f / Time.unscaledDeltaTime)}";
        }
        else
            _timer += Time.deltaTime;
    }
}
