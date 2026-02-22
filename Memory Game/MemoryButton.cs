using UdonSharp;
using UnityEngine;
using VRC.Udon;

public class MemoryButton : UdonSharpBehaviour
{
    public MemoryGameController controller;

    [Tooltip("0=Red, 1=Green, 2=Yellow, 3=Blue")]
    public int colorIndex;

    public override void Interact()
    {
        if (controller != null)
        {
            controller.PressColor(colorIndex);
        }
    }
}
