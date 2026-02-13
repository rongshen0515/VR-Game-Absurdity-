using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class RotateOnSolved : UdonSharpBehaviour
{
    [Header("Cuboid to rotate")]
    public Transform cuboid;

    [Header("Rotate around this axis (local axis)")]
    public Vector3 localAxis = Vector3.up;

    [Header("Degrees to rotate when triggered")]
    public float degreesToRotate = 180f;

    [Header("Rotation speed")]
    public float degreesPerSecond = 90f;

    private bool rotating = false;
    private Quaternion startRot;
    private Quaternion targetRot;

    public void StartRotating()
    {
        if (cuboid == null) return;
        if (rotating) return;

        rotating = true;

        // Save start and compute target rotation
        startRot = cuboid.localRotation;
        targetRot = startRot * Quaternion.AngleAxis(degreesToRotate, localAxis.normalized);
    }

    private void Update()
    {
        if (!rotating || cuboid == null) return;

        float step = degreesPerSecond * Time.deltaTime;

        // Move toward the target rotation
        cuboid.localRotation = Quaternion.RotateTowards(cuboid.localRotation, targetRot, step);

        // Stop when we reach it
        if (Quaternion.Angle(cuboid.localRotation, targetRot) <= 0.01f)
        {
            cuboid.localRotation = targetRot;
            rotating = false;
        }
    }
}
