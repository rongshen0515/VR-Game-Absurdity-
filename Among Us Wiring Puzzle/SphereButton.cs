using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SphereButton : UdonSharpBehaviour
{
    [Header("Drag your cuboid here")]
    public Transform cuboid;

    [Header("Rotation")]
    public Vector3 axis = Vector3.up;     // (0,1,0) rotates around Y
    public float degreesPerSecond = 90f;

    [Header("Button behavior")]
    public bool toggle = true;            // true = press once to start, press again to stop
    public float rotateSeconds = 1.0f;    // used only if toggle = false

    private bool rotating = false;
    private float stopTime = 0f;

    public override void Interact()
    {
        if (cuboid == null) return;

        if (toggle)
        {
            rotating = !rotating;
        }
        else
        {
            rotating = true;
            stopTime = Time.time + rotateSeconds;
        }
    }

    private void Update()
    {
        if (!rotating || cuboid == null) return;

        // rotate in world space
        cuboid.Rotate(axis.normalized * degreesPerSecond * Time.deltaTime, Space.World);

        // if momentary mode, stop after time
        if (!toggle && Time.time >= stopTime)
        {
            rotating = false;
        }
    }
}
