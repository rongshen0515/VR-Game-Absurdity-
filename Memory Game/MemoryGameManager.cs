using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MemoryGameController : UdonSharpBehaviour
{
    [Header("Game Settings")]
    public int totalRounds = 3;
    public int startingSequenceLength = 3;

    [Tooltip("How long each color stays lit during playback")]
    public float flashDuration = 0.5f;

    [Tooltip("Gap between flashes")]
    public float gapDuration = 0.25f;

    [Header("Button Renderers (0=Red, 1=Green, 2=Yellow, 3=Blue)")]
    public Renderer[] buttonRenderers; // Assign 4 renderers in inspector

    [Header("Button Colors")]
    public Color[] normalColors; // 4 colors
    public Color[] litColors;    // 4 brighter versions

    [Header("Optional")]
    public AudioSource audioSource;
    public AudioClip successClip;
    public AudioClip failClip;
    public AudioClip roundStartClip;

    // Internal state
    private int[] sequence;
    private int currentRound = 1;
    private int currentSequenceLength;
    private int inputIndex;

    private bool gameRunning = false;
    private bool isShowingSequence = false;
    private bool canPlayerInput = false;

    // Playback state machine
    private int playbackIndex = 0;
    private bool flashOn = false;
    private float timer = 0f;
    private int flashingButton = -1;

    // Optional lock so only owner controls logic
    // (Good for multiplayer consistency)
    private bool ownerOnly = true;

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
        if (ownerOnly && !Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        ResetAllLights();

        currentRound = 1;
        currentSequenceLength = startingSequenceLength;
        inputIndex = 0;

        BuildSequenceUpTo(currentSequenceLength);

        gameRunning = true;
        canPlayerInput = false;
        BeginPlayback();
    }

    private void BuildSequenceUpTo(int length)
    {
        for (int i = 0; i < length; i++)
        {
            // 0=Red, 1=Green, 2=Yellow, 3=Blue
            sequence[i] = Random.Range(0, 4);
        }
    }

    private void AddOneColorToSequence()
    {
        sequence[currentSequenceLength] = Random.Range(0, 4);
        currentSequenceLength++;
    }

    private void BeginPlayback()
    {
        isShowingSequence = true;
        canPlayerInput = false;
        inputIndex = 0;

        playbackIndex = 0;
        flashOn = false;
        timer = 0f;
        flashingButton = -1;

        ResetAllLights();

        if (audioSource != null && roundStartClip != null)
        {
            audioSource.PlayOneShot(roundStartClip);
        }
    }

    void Update()
    {
        if (!gameRunning) return;

        if (isShowingSequence)
        {
            HandleSequencePlayback();
        }
    }

    private void HandleSequencePlayback()
    {
        timer += Time.deltaTime;

        if (playbackIndex >= currentSequenceLength)
        {
            // Done showing sequence
            isShowingSequence = false;
            canPlayerInput = true;
            ResetAllLights();
            return;
        }

        int colorIndex = sequence[playbackIndex];

        if (!flashOn)
        {
            // Wait for gap, then turn on
            if (timer >= gapDuration)
            {
                timer = 0f;
                flashOn = true;
                flashingButton = colorIndex;
                SetButtonLit(flashingButton, true);
            }
        }
        else
        {
            // Light is on, wait flash duration, then turn off
            if (timer >= flashDuration)
            {
                timer = 0f;
                flashOn = false;
                SetButtonLit(flashingButton, false);
                flashingButton = -1;
                playbackIndex++;
            }
        }
    }

    // Called by MemoryButton.cs
    public void PressColor(int colorIndex)
    {
        if (!gameRunning) return;
        if (!canPlayerInput) return;
        if (isShowingSequence) return;

        // Optional mini flash on press
        FlashPressedButton(colorIndex);

        // Check input
        if (sequence[inputIndex] == colorIndex)
        {
            inputIndex++;

            // Player completed current round
            if (inputIndex >= currentSequenceLength)
            {
                OnRoundSuccess();
            }
        }
        else
        {
            OnGameFail();
        }
    }

    private void OnRoundSuccess()
    {
        canPlayerInput = false;

        if (audioSource != null && successClip != null)
        {
            audioSource.PlayOneShot(successClip);
        }

        // If final round finished
        if (currentRound >= totalRounds)
        {
            Debug.Log("[MemoryGame] Player won all rounds!");
            gameRunning = false;
            ResetAllLights();
            return;
        }

        // Advance round and add one new color
        currentRound++;
        AddOneColorToSequence();

        // Replay full sequence (previous part maintained)
        SendCustomEventDelayedSeconds(nameof(DelayedNextRoundPlayback), 0.75f);
    }

    public void DelayedNextRoundPlayback()
    {
        BeginPlayback();
    }

    private void OnGameFail()
    {
        canPlayerInput = false;
        gameRunning = false;

        if (audioSource != null && failClip != null)
        {
            audioSource.PlayOneShot(failClip);
        }

        Debug.Log("[MemoryGame] Wrong input. Game over.");
        ResetAllLights();
    }

    private void FlashPressedButton(int colorIndex)
    {
        // Quick visual feedback for player press
        SetButtonLit(colorIndex, true);
        flashingButton = colorIndex;
        SendCustomEventDelayedSeconds(nameof(TurnOffPressedFlash), 0.15f);
    }

    public void TurnOffPressedFlash()
    {
        if (flashingButton >= 0 && flashingButton < 4)
        {
            SetButtonLit(flashingButton, false);
            flashingButton = -1;
        }
    }

    private void ResetAllLights()
    {
        for (int i = 0; i < 4; i++)
        {
            SetButtonLit(i, false);
        }
    }

    private void SetButtonLit(int index, bool lit)
    {
        if (buttonRenderers == null || index < 0 || index >= buttonRenderers.Length) return;
        if (buttonRenderers[index] == null) return;

        Renderer r = buttonRenderers[index];

        // Use material.color (simple version)
        // If you use emission materials, I can give an emission version too
        if (lit)
        {
            r.material.color = litColors[index];
        }
        else
        {
            r.material.color = normalColors[index];
        }
    }
}
