using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MemoryGameManager : UdonSharpBehaviour
{
    public Renderer tabletRenderer;

    public Material defaultMat;
    public Material redMat;
    public Material greenMat;
    public Material blueMat;
    public Material yellowMat;

    public float flashTime = 1f;
    public float delayBetweenFlashes = 0.5f;

    private int[] sequence = new int[50];
    private int currentRound = 0;
    private int showIndex = 0;
    private int playerIndex = 0;
    private bool isShowing = false;

    void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        currentRound = 0;
        AddColor();
        SendCustomEventDelayedSeconds(nameof(PlaySequence), 1f);
    }

    void AddColor()
    {
        sequence[currentRound] = Random.Range(0, 4);
        currentRound++;
    }

    public void PlaySequence()
    {
        isShowing = true;
        showIndex = 0;
        playerIndex = 0;
        ShowNextColor();
    }

    public void ShowNextColor()
    {
        if (showIndex < currentRound)
        {
            SetColor(sequence[showIndex]);
            SendCustomEventDelayedSeconds(nameof(HideColor), flashTime);
        }
        else
        {
            isShowing = false;
        }
    }

    public void HideColor()
    {
        tabletRenderer.material = defaultMat;
        showIndex++;
        SendCustomEventDelayedSeconds(nameof(ShowNextColor), delayBetweenFlashes);
    }

    void SetColor(int index)
    {
        if (index == 0) tabletRenderer.material = redMat;
        if (index == 1) tabletRenderer.material = greenMat;
        if (index == 2) tabletRenderer.material = blueMat;
        if (index == 3) tabletRenderer.material = yellowMat;
    }

    public void PlayerInput(int index)
    {
        if (isShowing) return;

        if (sequence[playerIndex] == index)
        {
            playerIndex++;

            if (playerIndex >= currentRound)
            {
                AddColor();
                SendCustomEventDelayedSeconds(nameof(PlaySequence), 1f);
            }
        }
        else
        {
            StartGame();
        }
    }
}
