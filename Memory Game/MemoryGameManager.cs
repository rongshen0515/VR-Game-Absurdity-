using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Tablet : UdonSharpBehaviour
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

    [Header("Fail Light Blink")]
    public Color failBlinkColor = Color.red;
    public int failBlinkCount = 3;
    public float failBlinkOnTime = 0.15f;
    public float failBlinkOffTime = 0.12f;

    // Internal fail blink state
    private int failBlinkTogglesRemaining = 0;
    private bool failBlinkState = false;

    // Cache the light's original color so we can restore it
    private Color cachedRoundLightColor = Color.white;
    private bool hasCachedRoundLightColor = false;

    [Header("Round Success Light")]
    public Light roundSuccessLight;   // Assign in Inspector
    public float roundSuccessLightDuration = 0.4f;

    public int finalWinBlinkCount = 3;
    public float finalWinBlinkOnTime = 0.18f;
    public float finalWinBlinkOffTime = 0.12f;

    // Internal final-win blink state
    private int finalWinBlinkTogglesRemaining = 0;
    private bool finalWinBlinkState = false;

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
        TLog("Start() called");

        TLog("buttonRenderers null? " + (buttonRenderers == null));
        TLog("normalColors null? " + (normalColors == null));
        TLog("litColors null? " + (litColors == null));

        if (buttonRenderers != null) TLog("buttonRenderers length = " + buttonRenderers.Length);
        if (normalColors != null) TLog("normalColors length = " + normalColors.Length);
        if (litColors != null) TLog("litColors length = " + litColors.Length);

        InitButtonVisuals();
        if (roundSuccessLight != null)
        {
            roundSuccessLight.enabled = false;
        }

        if (roundSuccessLight != null)
        {
            roundSuccessLight.enabled = false;
            cachedRoundLightColor = roundSuccessLight.color;
            hasCachedRoundLightColor = true;
        }

        int maxLen = startingSequenceLength + totalRounds - 1;
        sequence = new int[maxLen];

        TLog("Sequence array created. maxLen = " + maxLen);

        // QUICK TEST: force one color on for 1 sec to verify renderer/material works
        SetButtonLit(0, true);
        SendCustomEventDelayedSeconds(nameof(DebugTurnOffTestLight), 1f);
    }

    public void DebugTurnOffTestLight()
    {
        SetButtonLit(0, false);
        TLog("Debug test light off");
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

    public void PressColor(int colorIndex)
    {
        TLog("PressColor called with colorIndex=" + colorIndex);

        if (!gameRunning)
        {
            TLog("Press ignored: gameRunning=false");
            return;
        }

        if (!canPlayerInput)
        {
            TLog("Press ignored: canPlayerInput=false");
            return;
        }

        if (isShowingSequence)
        {
            TLog("Press ignored: isShowingSequence=true");
            return;
        }

        FlashPressedButton(colorIndex);

        TLog("Checking input. inputIndex=" + inputIndex +
             " expected=" + sequence[inputIndex] +
             " got=" + colorIndex);

        if (sequence[inputIndex] == colorIndex)
        {
            inputIndex++;
            TLog("Correct input. New inputIndex=" + inputIndex + "/" + currentSequenceLength);

            if (inputIndex >= currentSequenceLength)
            {
                TLog("Round completed. Calling OnRoundSuccess()");
                OnRoundSuccess();
            }
        }
        else
        {
            TLog("Wrong input. Calling OnGameFail()");
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

        // Turn on success light
        if (roundSuccessLight != null)
        {
            roundSuccessLight.enabled = true;
        }

        // Wait, then turn it off + continue
        SendCustomEventDelayedSeconds(nameof(FinishRoundSuccessTransition), roundSuccessLightDuration);
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

        ResetAllLights();

        // Start red blink fail effect
        StartFailBlinkSequence();
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

    public void FinishRoundSuccessTransition()
    {
        // Turn off success light (from normal round-success glow)
        if (roundSuccessLight != null)
        {
            roundSuccessLight.enabled = false;
        }

        // If final round finished, do final blink celebration
        if (currentRound >= totalRounds)
        {
            StartFinalWinBlinkSequence();
            return;
        }

        // Advance round and add one new color
        currentRound++;
        AddOneColorToSequence();

        SendCustomEventDelayedSeconds(nameof(DelayedNextRoundPlayback), 0.2f);
    }

    public void StartFinalWinBlinkSequence()
    {
        TLog("Final win reached. Starting blink celebration.");

        canPlayerInput = false;
        gameRunning = false; // stop normal game loop while celebrating
        ResetAllLights();

        if (roundSuccessLight == null)
        {
            Debug.Log("[MemoryGame] Player won all rounds! (No success light assigned)");
            return;
        }

        // 3 blinks = ON,OFF repeated 3 times = 6 toggles
        finalWinBlinkTogglesRemaining = finalWinBlinkCount * 2;
        finalWinBlinkState = false;
        roundSuccessLight.enabled = false;

        SendCustomEventDelayedSeconds(nameof(FinalWinBlinkStep), 0.01f);
    }

    public void FinalWinBlinkStep()
    {
        if (roundSuccessLight == null) return;

        // Toggle light
        finalWinBlinkState = !finalWinBlinkState;
        roundSuccessLight.enabled = finalWinBlinkState;

        finalWinBlinkTogglesRemaining--;

        // Done blinking
        if (finalWinBlinkTogglesRemaining <= 0)
        {
            roundSuccessLight.enabled = false;
            Debug.Log("[MemoryGame] Player won all rounds!");
            return;
        }

        // Schedule next toggle
        if (finalWinBlinkState)
        {
            SendCustomEventDelayedSeconds(nameof(FinalWinBlinkStep), finalWinBlinkOnTime);
        }
        else
        {
            SendCustomEventDelayedSeconds(nameof(FinalWinBlinkStep), finalWinBlinkOffTime);
        }
    }

    public void StartFailBlinkSequence()
    {
        TLog("Game failed. Starting RED blink.");

        if (roundSuccessLight == null)
        {
            Debug.Log("[MemoryGame] Wrong input. Game over. (No fail light assigned)");
            return;
        }

        // Set fail color
        if (hasCachedRoundLightColor)
        {
            // cachedRoundLightColor is stored already
        }
        roundSuccessLight.color = failBlinkColor;

        // 3 blinks = ON/OFF x3 = 6 toggles
        failBlinkTogglesRemaining = failBlinkCount * 2;
        failBlinkState = false;
        roundSuccessLight.enabled = false;

        SendCustomEventDelayedSeconds(nameof(FailBlinkStep), 0.01f);
    }

    public void FailBlinkStep()
    {
        if (roundSuccessLight == null) return;

        failBlinkState = !failBlinkState;
        roundSuccessLight.enabled = failBlinkState;

        failBlinkTogglesRemaining--;

        if (failBlinkTogglesRemaining <= 0)
        {
            // End of fail blink
            roundSuccessLight.enabled = false;

            // Restore original light color so success/final-win still look correct later
            if (hasCachedRoundLightColor)
            {
                roundSuccessLight.color = cachedRoundLightColor;
            }

            Debug.Log("[MemoryGame] Wrong input. Game over.");
            return;
        }

        if (failBlinkState)
        {
            SendCustomEventDelayedSeconds(nameof(FailBlinkStep), failBlinkOnTime);
        }
        else
        {
            SendCustomEventDelayedSeconds(nameof(FailBlinkStep), failBlinkOffTime);
        }
    }

    private void TLog(string msg)
    {
        Debug.Log("[Tablet] " + msg);
    }
}