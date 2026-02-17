using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MemoryButton : UdonSharpBehaviour
{
    public MemoryGameManager gameManager;

    [Tooltip("0=Red, 1=Green, 2=Blue, 3=Yellow")]
    public int colorIndex;

    public override void Interact()
    {
        gameManager.PlayerInput(colorIndex);
    }
}
