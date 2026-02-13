using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WiringPuzzleManager : UdonSharpBehaviour
{
    [Header("All wire start scripts in the puzzle")]
    public WireStart[] wires;

    [Header("UI root to close on completion")]
    public GameObject wiringCanvas;

    [Header("Optional: button script to lock after solved")]
    public OpenPuzzleButton openButton;

    [Header("Receiver that will rotate the cuboid")]
    public RotateOnSolved rotator;

    private bool solved = false;

    public void CheckCompletion()
    {
        if (solved) return;
        if (wires == null || wires.Length == 0) return;

        for (int i = 0; i < wires.Length; i++)
        {
            if (wires[i] == null) return;
            if (!wires[i].isConnectedCorrectly) return;
        }

        PuzzleComplete();
    }

    private void PuzzleComplete()
    {
        solved = true;

        if (wiringCanvas != null)
            wiringCanvas.SetActive(false);

        if (openButton != null)
            openButton.MarkSolved();

        if (rotator != null)
            rotator.StartRotating();
    }
}