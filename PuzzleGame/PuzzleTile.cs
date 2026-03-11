using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PuzzleTile : UdonSharpBehaviour
{
    [HideInInspector] public PuzzleManager manager;
    [HideInInspector] public int correctSlotIndex;
    [HideInInspector] public int currentSlotIndex;

    public override void Interact()
    {
        if (manager != null)
        {
            manager.SelectTile(this);
        }
    }
}
