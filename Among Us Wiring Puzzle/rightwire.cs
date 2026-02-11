// Goes on each RIGHT wire node
// Receiving Wire Connection
// Sending Connection Info to Wire Start

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WireEnd : UdonSharpBehaviour
{
    public string wireID;
    public Transform snapPoint;

    public WireStart[] allWireStarts;

    public override void Interact()
    {
        foreach (WireStart wire in allWireStarts)
        {
            wire.ConnectTo(this);
        }
    }
}
