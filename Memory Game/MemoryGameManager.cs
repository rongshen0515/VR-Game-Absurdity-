using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MemoryGameController : UdonSharpBehaviour
{
    [Header("Tablet Display")]
    public Renderer tabletRenderer;
    public Material defaultMat;
    public Material redMat;
    public Material greenMat;
    public Material blueMat;
    public Material yellowMat;

    [Header("Game Settings")]
    public float flashTime = 0.8f;
    public float delayBetweenFlashes = 0.4f;
    public int maxRounds = 10;

    private int[] sequence = new int[50];
    private int currentRound = 0;
    private int playerIndex = 0;
    private bool isShowingSequence = false;

    void Start()
    {
        InitButtonVisuals();

        // Max possible length = starting + (rounds - 1)
        int maxLen = startingSequenceLength + totalRounds - 1;
        sequence = new int[maxLen];
    }

    private void InitButtonVisuals()
    {
        if (buttonRenderers == null || buttonRenderers.Length < 4) return;
        for (int i = 0; i < 4; i++)
        {
            SetButtonLit(i, false);
        }
    }

    public override void Interact()
    {
        // You can put this script on the tablet root object
        // and pressing the tablet starts/restarts the game.
        StartGame();
    }

    public void StartGame()
    {
        currentRound = 0;
        playerIndex = 0;
        AddColor();
        SendCustomEventDelayedSeconds(nameof(PlaySequence), 1f);
    }

    void AddColor()
    {
        sequence[currentSequenceLength] = Random.Range(0, 4);
        currentSequenceLength++;
    }

    private void BeginPlayback()
    {
        isShowingSequence = true;
        playerIndex = 0;
        StartCoroutine(FlashSequence());
    }

    System.Collections.IEnumerator FlashSequence()
    {
        for (int i = 0; i < currentRound; i++)
        {
            ShowColor(sequence[i]);
            yield return new WaitForSeconds(flashTime);
            tabletRenderer.material = defaultMat;
            yield return new WaitForSeconds(delayBetweenFlashes);
        }
        isShowingSequence = false;
    }

    void ShowColor(int colorIndex)
    {
        switch (colorIndex)
        {
            case 0: tabletRenderer.material = redMat; break;
            case 1: tabletRenderer.material = greenMat; break;
            case 2: tabletRenderer.material = blueMat; break;
            case 3: tabletRenderer.material = yellowMat; break;
        }
    }

    public void PlayerInput(int colorIndex)
    {
        if (isShowingSequence) return;

        if (sequence[playerIndex] == colorIndex)
        {
            playerIndex++;

            if (playerIndex >= currentRound)
            {
                if (currentRound >= maxRounds)
                {
                    Debug.Log("You Win!");
                    return;
                }

                AddColor();
                SendCustomEventDelayedSeconds(nameof(PlaySequence), 1f);
            }
        }
        else
        {
            Debug.Log("Wrong! Restarting...");
            StartGame();
        }
    }
}

