using UdonSharp;
using UnityEngine;

public class WiringPuzzleManager : UdonSharpBehaviour
{
    public WireSlot[] slots;
    public Transform safe;
    public bool solved = false;

    public void CheckPuzzle()
    {
        if (solved) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].isCorrect)
                return;
        }

        solved = true;
        RotateSafe();
    }

    private void RotateSafe()
    {
        safe.Rotate(0f, 180f, 0f);
    }
}