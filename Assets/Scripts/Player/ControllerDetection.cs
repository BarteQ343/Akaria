using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class ControllerDetection : MonoBehaviour
{
    public TMP_Text ControlsText;
    public static InputDevice LastUsedDevice { get; private set; }
    
    private void OnEnable()
    {
        InputSystem.onEvent += OnInputEvent;
    }

    private void OnDisable()
    {
        InputSystem.onEvent -= OnInputEvent;
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        // Pomijamy "noise" z urz¹dzeñ, które nie s¹ aktywne
        if (device != null && eventPtr.IsA<StateEvent>() || eventPtr.IsA<DeltaStateEvent>())
        {
            LastUsedDevice = device;
            // Mo¿na dodaæ debug
            Debug.Log("Ostatnio u¿yte urz¹dzenie: " + device.displayName);
        }

        if (device.displayName == "Keyboard" || device.displayName == "Mouse")
        {
            ControlsText.text = "Controls:\nAttack:Left Click";
        } else if (device.displayName == "Wireless Controller" || device.displayName == "DualSense Wireless Controller")
        {
            ControlsText.text = "Controls:\nAttack:Square";
        } else if (device.displayName.Contains("Xbox"))
        {
            ControlsText.text = "Controls:\nAttack:X";
        }
    }
}
