using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PuzzleManager : UdonSharpBehaviour
{
    [Header("Assign in solved order")]
    public PuzzleTile[] tiles;          // 6 tiles
    public Transform[] slotPoints;      // 6 slot positions in the correct layout

    [Header("Optional")]
    public GameObject winObject;        // for a "You Win" panel, optional

    private PuzzleTile firstSelected;
    private PuzzleTile secondSelected;
    private bool puzzleSolved;

    void Start()
    {
        if (winObject != null)
        {
            winObject.SetActive(false);
        }

        SetupTiles();
        ShuffleTiles();
    }

    private void SetupTiles()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].manager = this;
            tiles[i].correctSlotIndex = i;
        }
    }

    private void ShuffleTiles()
    {
        int[] shuffled = new int[slotPoints.Length];

        for (int i = 0; i < shuffled.Length; i++)
        {
            shuffled[i] = i;
        }

        // Fisher-Yates shuffle
        for (int i = shuffled.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = shuffled[i];
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }

        // Make sure it does not start already solved
        bool alreadySolved = true;
        for (int i = 0; i < shuffled.Length; i++)
        {
            if (shuffled[i] != i)
            {
                alreadySolved = false;
                break;
            }
        }

        if (alreadySolved && shuffled.Length > 1)
        {
            int temp = shuffled[0];
            shuffled[0] = shuffled[1];
            shuffled[1] = temp;
        }

        // Place tiles into shuffled slots
        for (int i = 0; i < tiles.Length; i++)
        {
            int slotIndex = shuffled[i];
            tiles[i].currentSlotIndex = slotIndex;
            tiles[i].transform.position = slotPoints[slotIndex].position;
            tiles[i].transform.rotation = slotPoints[slotIndex].rotation;
        }

        puzzleSolved = false;
        firstSelected = null;
        secondSelected = null;
    }

    public void SelectTile(PuzzleTile clickedTile)
    {
        if (puzzleSolved) return;

        if (firstSelected == null)
        {
            firstSelected = clickedTile;
            return;
        }

        if (clickedTile == firstSelected)
        {
            return;
        }

        secondSelected = clickedTile;
        SwapSelectedTiles();
        CheckWin();

        firstSelected = null;
        secondSelected = null;
    }

    private void SwapSelectedTiles()
    {
        if (firstSelected == null || secondSelected == null) return;

        int firstSlot = firstSelected.currentSlotIndex;
        int secondSlot = secondSelected.currentSlotIndex;

        firstSelected.currentSlotIndex = secondSlot;
        secondSelected.currentSlotIndex = firstSlot;

        firstSelected.transform.position = slotPoints[secondSlot].position;
        firstSelected.transform.rotation = slotPoints[secondSlot].rotation;

        secondSelected.transform.position = slotPoints[firstSlot].position;
        secondSelected.transform.rotation = slotPoints[firstSlot].rotation;
    }

    private void CheckWin()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i].currentSlotIndex != tiles[i].correctSlotIndex)
            {
                return;
            }
        }

        puzzleSolved = true;
        Debug.Log("Puzzle solved! You win!");

        if (winObject != null)
        {
            winObject.SetActive(true);
        }
    }
}
