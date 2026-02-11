using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WiringPuzzleTrigger : UdonSharpBehaviour
{
    public GameObject wiringScreen;

    public override void Interact()
    {
        wiringScreen.SetActive(true);
    }
}

// This script is attached to object that 
// player initially clicks