using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AxeHitbox : UdonSharpBehaviour
{
    [Header("Hit cooldown so one swing doesn't register 20 hits")]
    public float hitCooldown = 0.35f;

    [Header("Minimum speed to count as a hit")]
    public float minHitSpeed = 0.8f;

    private float lastHitTime = -999f;
    private Vector3 lastPos;
    private float currentSpeed;

    void Start()
    {
        lastPos = transform.position;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (dt > 0f)
        {
            currentSpeed = Vector3.Distance(transform.position, lastPos) / dt;
            lastPos = transform.position;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryHit(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    private void TryHit(Collider other)
    {
        if (Time.time - lastHitTime < hitCooldown) return;
        if (currentSpeed < minHitSpeed) return;

        PinataTarget pinata = (PinataTarget)other.GetComponent(typeof(PinataTarget));
        if (pinata == null)
        {
            // In case the collider is on a child object
            pinata = (PinataTarget)other.GetComponentInParent(typeof(PinataTarget));
        }

        if (pinata != null)
        {
            lastHitTime = Time.time;
            pinata.RegisterHit();
        }
    }
}
