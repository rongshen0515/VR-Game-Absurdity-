using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SphereButton : UdonSharpBehaviour
{
    [Header("UI root object to show/hide")]
    public GameObject wiringCanvas;

    [Header("Optional: lock reopening after solved")]
    public bool lockAfterSolved = true;

    [HideInInspector] public bool puzzleSolved = false;

    public override void Interact()
    {
        if (wiringCanvas == null) return;
        if (lockAfterSolved && puzzleSolved) return;

        wiringCanvas.SetActive(true);
    }

    // Puzzle manager can call this when solved
    public void MarkSolved()
    {
        puzzleSolved = true;
    }
}
