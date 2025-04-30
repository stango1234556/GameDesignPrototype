using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;

    [SerializeField] private float _speed;
    [SerializeField] private float _jumpSpeed;
    [SerializeField] private float _turnSpeed;

    private int _jumpCount;
    [SerializeField] private int _maxJumpCount;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundCheckDistance;
    private Rigidbody _rigidbody;
    private Vector3 _input;
    private bool _jumpTriggered;
    private PlayerInputController _playerInputController;

    [SerializeField] private Transform grabPoint;
    [SerializeField] private float grabRange = 1.5f;
    [SerializeField] private LayerMask grabbableLayer;
    private FixedJoint grabJoint;
    private Rigidbody grabbedRb;

    // New: reference to the current platform
    private MovingPlatform _currentPlatform;

    private void Awake()
    {
        _playerInputController = GetComponent<PlayerInputController>();
        _rigidbody = GetComponent<Rigidbody>();

        _playerInputController.OnJumpButtonPressed += JumpButtonPressed;
        _playerInputController.OnGrabInputChanged += HandleGrab;
    }

    void Update()
    {
        Input.GetJoystickNames();
    }

    void FixedUpdate()
    {
        Move();
        GatherInput();
        Look();

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, _groundCheckDistance, _groundLayer))
        {
            _jumpCount = 0;

            // Check if we're on a moving platform
            _currentPlatform = hit.collider.GetComponent<MovingPlatform>();
        }
        else
        {
            _currentPlatform = null;
        }

        if (_jumpTriggered)
        {
            _rb.AddForce(Vector3.up * _jumpSpeed, ForceMode.Impulse);
            _jumpTriggered = false;
        }
    }

    void GatherInput()
    {
        _input = new Vector3(_playerInputController.MovementInputVector.x, 0, _playerInputController.MovementInputVector.y);
    }

    private void HandleGrab(bool isGrabbing)
    {
        if (isGrabbing)
        {
            if (grabJoint != null) return;

            Collider[] hits = Physics.OverlapSphere(grabPoint.position, grabRange);

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Grabbable") && hit.attachedRigidbody != null)
                {
                    grabJoint = gameObject.AddComponent<FixedJoint>();
                    grabJoint.connectedBody = hit.attachedRigidbody;
                    grabbedRb = hit.attachedRigidbody;
                    break;
                }
            }
        }
        else
        {
            if (grabJoint != null)
            {
                grabJoint.connectedBody = null;
                Destroy(grabJoint);
                grabJoint = null;
                grabbedRb = null;
            }
        }
    }

    void Look()
    {
        if (_input != Vector3.zero)
        {
            var relative = (transform.position + _input.ToIso()) - transform.position;
            var rot = Quaternion.LookRotation(relative, Vector3.up);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, _turnSpeed * Time.deltaTime);
        }
    }

    void Move()
    {
        Vector3 platformDelta = _currentPlatform != null ? _currentPlatform.DeltaMovement : Vector3.zero;
        Vector3 movement = (transform.forward * _input.magnitude) * _speed * Time.deltaTime;
        _rb.MovePosition(transform.position + movement + platformDelta);
    }

    private void JumpButtonPressed()
    {
        if (_jumpCount < _maxJumpCount)
        {
            _jumpTriggered = true;
            _jumpCount++;
        }
    }
}
