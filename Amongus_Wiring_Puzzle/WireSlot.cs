using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class WireSlot : UdonSharpBehaviour
{
    public int correctWireID;
    public bool locked;
    public WiringPuzzleManager puzzleManager;

    private void OnTriggerEnter(Collider other)
    {
        if (locked) return;

        WireEnd wire = other.GetComponent<WireEnd>();
        if (wire == null) return;

        // Ignore already-locked wires
        if (wire.locked) return;

        VRC_Pickup pickup = wire.GetComponent<VRC_Pickup>();
        if (pickup != null && pickup.IsHeld)
        {
            pickup.Drop();
        }

        // ✅ Correct wire
        if (wire.wireID == correctWireID)
        {
            locked = true;
            wire.locked = true;

            // Snap to slot
            wire.transform.position = transform.position;
            wire.transform.rotation = transform.rotation;

            // Lock physics
            Rigidbody rb = wire.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            if (pickup != null) pickup.pickupable = false;

            // Disable collider so nothing else can interact
            Collider col = wire.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            puzzleManager.CheckPuzzle();
        }
        else
        {
            // ❌ Wrong wire
            wire.ResetWire();
        }
    }
}
