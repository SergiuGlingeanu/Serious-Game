using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public RotationMode rotationMode;
    public bool flip;

    public bool forceSpecificRotation;
    public Vector3 rotation;

    private void Update() => ApplyBillboardEffect();

    public void ApplyBillboardEffect() {
        if (forceSpecificRotation) {
            transform.eulerAngles = rotation;
            return;
        }

        Vector3 position = Camera.main.transform.position;
        switch (rotationMode)
        {
            case RotationMode.XAxis:
                position.x = transform.position.x;
                break;
            case RotationMode.YAxis:
                position.y = transform.position.y;
                break;
            case RotationMode.ZAxis:
                position.z = transform.position.z;
                break;
        }

        transform.LookAt(flip ? 2f * transform.position - position : position);
    }

    public enum RotationMode
    {
        AllAxis,
        XAxis,
        YAxis,
        ZAxis
    }
}
