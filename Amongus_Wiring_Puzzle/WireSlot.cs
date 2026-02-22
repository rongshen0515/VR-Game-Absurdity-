using UdonSharp;
using UnityEngine;

public class WireSlot : UdonSharpBehaviour
{
    public int correctWireID;
    public bool isCorrect = false;
    public WiringPuzzleManager puzzleManager;

    private void OnTriggerEnter(Collider other)
    {
        Wire wire = other.GetComponent<Wire>();
        if (wire == null) return;

        if (wire.wireID == correctWireID)
        {
            wire.placedCorrectly = true;
            isCorrect = true;

            // Snap wire into place
            wire.transform.position = transform.position;
            puzzleManager.CheckPuzzle();
        }
        else
        {
            wire.ResetWire();
        }
    }
}