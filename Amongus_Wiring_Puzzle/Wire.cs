using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class WireEnd : UdonSharpBehaviour
{
    public int wireID;
    public Transform startPoint;
    public LineRenderer line;

    private Vector3 startWorldPos;
    public bool locked; // NEW

    void Start()
    {
        // Store WORLD position, not local
        startWorldPos = transform.position;
    }

    void Update()
    {
        if (line != null && startPoint != null)
        {
            line.SetPosition(0, startPoint.position);
            line.SetPosition(1, transform.position);
        }
    }

    public void ResetWire()
    {
        // DO NOTHING if this wire is already correctly placed
        if (locked) return;

        transform.position = startWorldPos;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
