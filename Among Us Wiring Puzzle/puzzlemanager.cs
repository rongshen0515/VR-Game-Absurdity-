using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WiringPuzzleManager : UdonSharpBehaviour
{
    public WireStart[] wires;

    public GameObject wiringCanvas;
    public GameObject objectToDisappear;

    public void CheckCompletion()
    {
        foreach (WireStart wire in wires)
        {
            if (!wire.isConnectedCorrectly)
                return;
        }

        PuzzleComplete();
    }

    private void PuzzleComplete()
    {
        wiringCanvas.SetActive(false);

        if (objectToDisappear != null)
        {
            objectToDisappear.SetActive(false);
        }
    }
}

// Checking if puzzle solved
// Closing UI
// Making another object disappear after puzzle solved (for key)