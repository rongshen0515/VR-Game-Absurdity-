using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
// This script opens the puzzle once player clicks the button
public class PuzzleButton : UdonSharpBehaviour
{
    public GameObject wiringPanel;

    public override void Interact()
    {
        wiringPanel.SetActive(true);
    }
}