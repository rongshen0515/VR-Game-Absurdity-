using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class CubeRotator : UdonSharpBehaviour
{
    [Header("Rotation Settings")]
    public Vector3 rotationAxis = Vector3.up;
    public float degreesPerSecond = 90f;

    [UdonSynced] private bool _rotating;

    public void StartRotation()
    {
        _rotating = true;
        RequestSerialization();
    }

    public void StopRotation()
    {
        _rotating = false;
        RequestSerialization();
    }

    private void Update()
    {
        if (!_rotating) return;

        transform.Rotate(rotationAxis.normalized, degreesPerSecond * Time.deltaTime, Space.Self);
    }
}
