using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
// Attach this to each wire,  
// Each wire has a unique ID (0-3) that corresponds to the correct slot. 
// The manager checks if the wires are placed correctly.
public class Wire : UdonSharpBehaviour
{
    public int wireID; // 0–3
    [HideInInspector] public bool placedCorrectly = false;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    public void ResetWire()
    {
        transform.position = startPosition;
        placedCorrectly = false;
    }
}