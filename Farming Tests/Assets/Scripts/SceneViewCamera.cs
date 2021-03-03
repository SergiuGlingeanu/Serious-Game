using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneViewCamera : MonoBehaviour
{
    private Vector2 _axes;
    [Range(0.01f, 100f)] public float sensitivity = 1f;
    void Update()
    {
        _axes = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(2))
        {
            transform.position -= Vector3.right * _axes.x * sensitivity * Time.deltaTime;
            transform.position -= Vector3.up * _axes.y * sensitivity * Time.deltaTime;
        }
    }
}
