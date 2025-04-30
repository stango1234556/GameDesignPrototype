using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    public Vector2 MovementInputVector { get; private set; }

    public event Action OnJumpButtonPressed;

    public event Action<bool> OnGrabInputChanged;

    private void OnGrab(InputValue inputValue)
    {
        bool isPressed = inputValue.isPressed;
        Debug.Log("Grab input: " + isPressed);
        OnGrabInputChanged?.Invoke(isPressed);
    }
    private void OnMove(InputValue inputValue){
        MovementInputVector = inputValue.Get<Vector2>();
    }

    private void OnJump(InputValue inputValue){
        if(inputValue.isPressed){
            OnJumpButtonPressed?.Invoke();
        }
    }
}
