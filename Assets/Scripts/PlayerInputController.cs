using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    public Vector2 MovementInputVector { get; private set; }
    public Vector2 RightStickInputVector { get; private set; }

    public event Action OnJumpButtonPressed;
    public event Action<bool> OnGrabInputChanged;
    public event Action<bool> OnTetherControlChanged;

    private bool _leftHeld = false;
    private bool _rightHeld = false;
    private PlayerInput _playerInput;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    private void OnMove(InputValue inputValue)
    {
        MovementInputVector = inputValue.Get<Vector2>();
    }

    private void OnJump(InputValue inputValue)
    {
        if (inputValue.isPressed)
        {
            OnJumpButtonPressed?.Invoke();
        }
    }

    private void OnGrab(InputValue inputValue)
    {
        OnGrabInputChanged?.Invoke(inputValue.isPressed);
    }

    private void OnRightStick(InputValue inputValue)
    {
        RightStickInputVector = inputValue.Get<Vector2>();
    }

    private void OnLeftShoulder(InputValue inputValue)
    {
        _leftHeld = inputValue.isPressed;
        CheckTetherControlState();
    }

    private void OnRightShoulder(InputValue inputValue)
    {
        _rightHeld = inputValue.isPressed;
        CheckTetherControlState();
    }

    private void CheckTetherControlState()
    {
        var gamepad = _playerInput.devices[0] as Gamepad;
        bool bothHeld = _leftHeld && _rightHeld;
        
        OnTetherControlChanged?.Invoke(bothHeld);
    }

    public event Action OnLaunchPressed;

    private void OnWest(InputValue value)
    {
        if (value.isPressed)
        {
            OnLaunchPressed?.Invoke();
        }
    }
}
