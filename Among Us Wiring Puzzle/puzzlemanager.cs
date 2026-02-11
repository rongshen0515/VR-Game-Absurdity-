using UdonSharp;
using UnityEngine;

public class WiringPuzzleManager : UdonSharpBehaviour
{
    public WireStart[] wires;

    public GameObject wiringCanvas;
    public GameObject objectToDisappear;

    public CubeRotator cubeRotator; 
	
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

        if (cubeRotator != null)
        {
            cubeRotator.StartRotation();
        }
    }
}
