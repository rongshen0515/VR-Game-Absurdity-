using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CuboidRotator : UdonSharpBehaviour
{
    [Header("Rotation Settings")]
    public Vector3 rotationAxis = new Vector3(0f, 1f, 0f); // Y axis by default
    public float degreesPerSecond = 90f;

    [Header("State")]
    [UdonSynced] private bool isRotating = false;

    private void Update()
    {
        if (!isRotating) return;

        // Rotate in world space (change to Space.Self if you want local rotation)
        transform.Rotate(rotationAxis.normalized * degreesPerSecond * Time.deltaTime, Space.World);
    }

    // Called by the button
    public void SetRotating(bool value)
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        isRotating = value;
        RequestSerialization();
    }

    // Called by the button (toggle mode)
    public void ToggleRotating()
    {
        SetRotating(!isRotating);
    }

    public bool GetRotating()
    {
        return isRotating;
    }
}
