using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SimpleFlashlight : UdonSharpBehaviour
{
    [Header("Assign the Light component here")]
    public Light flashlightLight;

    [Header("Start ON or OFF")]
    public bool startOn = false;

    private bool isOn;

    void Start()
    {
        isOn = startOn;
        UpdateLightState();
    }

    public override void Interact()
    {
        isOn = !isOn;
        UpdateLightState();
    }

    private void UpdateLightState()
    {
        if (flashlightLight != null)
        {
            flashlightLight.enabled = isOn;
        }
    }
}
