using UdonSharp;
using UnityEngine;

public class WiringPuzzleManager : UdonSharpBehaviour
{
    public WireSlot[] slots;
    public Transform safe;

    private bool solved;
    private bool isRotating;
    private Quaternion targetRotation;
    public float rotationSpeed = 90f;

    public void CheckPuzzle()
    {
        if (solved) return;

        foreach (WireSlot slot in slots)
        {
            // 🔑 CHANGE IS HERE
            if (!slot.locked)
                return;
        }

        solved = true;

        // Rotate safe 180 degrees
        targetRotation = safe.rotation * Quaternion.Euler(0f, 180f, 0f);
        isRotating = true;
    }

    void Update()
    {
        if (!isRotating) return;

        safe.rotation = Quaternion.RotateTowards(
            safe.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        if (Quaternion.Angle(safe.rotation, targetRotation) < 0.1f)
        {
            safe.rotation = targetRotation;
            isRotating = false;
        }
    }
}
