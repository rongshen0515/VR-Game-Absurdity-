// Goes on each LEFT wire node
// Dragging Wire
// Drawing Wire visually
// Checking correct match


// Each Left Wire MUST HAVE
// LineRenderer, Manager, StartPoint assigned
using UdonSharp;
using UnityEngine;

public class WireStart : UdonSharpBehaviour
{
    public string wireID;
    public WiringPuzzleManager manager;

    public LineRenderer line;
    public Transform startPoint;

    public bool isConnectedCorrectly = false;

    private bool dragging = false;

    void Update()
    {
        if (dragging)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 2f;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

            line.SetPosition(1, worldPos);
        }
    }

    public override void Interact()
    {
        dragging = true;
        line.enabled = true;

        line.SetPosition(0, startPoint.position);
        line.SetPosition(1, startPoint.position);
    }

    public void ConnectTo(WireEnd endpoint)
    {
        if (!dragging) return;

        dragging = false;

        if (endpoint.wireID == wireID)
        {
            isConnectedCorrectly = true;
            line.SetPosition(1, endpoint.snapPoint.position);
        }
        else
        {
            isConnectedCorrectly = false;
            ResetWire();
        }

        manager.CheckCompletion();
    }

    public void ResetWire()
    {
        line.SetPosition(1, startPoint.position);
    }
}
