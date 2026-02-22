using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PinataTarget : UdonSharpBehaviour
{
    [Header("Pinata HP")]
    public int hitsToBreak = 5;

    [Header("Objects")]
    public GameObject intactPinata;   // the normal pinata object
    public GameObject brokenPinata;   // broken version (disabled at start)
    public GameObject[] candyObjects; // optional candies (disabled at start)

    [Header("Optional effects")]
    public AudioSource audioSource;
    public AudioClip hitClip;
    public AudioClip breakClip;
    public ParticleSystem breakParticles;

    [Header("Physics after break (optional)")]
    public Rigidbody[] candyBodies;
    public float candyPopForce = 2.5f;

    [UdonSynced] private int currentHits = 0;
    [UdonSynced] private bool isBroken = false;

    void Start()
    {
        ApplyVisualState();
    }

    public void RegisterHit()
    {
        if (isBroken) return;

        // Owner handles state changes (multiplayer safer)
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        currentHits++;

        if (currentHits >= hitsToBreak)
        {
            BreakPinata();
        }
        else
        {
            PlayHitFeedback();
        }

        RequestSerialization();
    }

    private void BreakPinata()
    {
        if (isBroken) return;

        isBroken = true;
        ApplyVisualState();

        if (audioSource != null && breakClip != null)
            audioSource.PlayOneShot(breakClip);

        if (breakParticles != null)
            breakParticles.Play();

        // Pop candies outward a little
        if (candyBodies != null)
        {
            for (int i = 0; i < candyBodies.Length; i++)
            {
                Rigidbody rb = candyBodies[i];
                if (rb != null)
                {
                    rb.isKinematic = false;
                    Vector3 dir = (rb.transform.position - transform.position).normalized;
                    if (dir == Vector3.zero) dir = Random.onUnitSphere;
                    rb.AddForce(dir * candyPopForce, ForceMode.Impulse);
                }
            }
        }
    }

    private void PlayHitFeedback()
    {
        if (audioSource != null && hitClip != null)
            audioSource.PlayOneShot(hitClip);
    }

    private void ApplyVisualState()
    {
        if (intactPinata != null) intactPinata.SetActive(!isBroken);
        if (brokenPinata != null) brokenPinata.SetActive(isBroken);

        if (candyObjects != null)
        {
            for (int i = 0; i < candyObjects.Length; i++)
            {
                if (candyObjects[i] != null)
                    candyObjects[i].SetActive(isBroken);
            }
        }
    }

    public override void OnDeserialization()
    {
        ApplyVisualState();
    }
}
