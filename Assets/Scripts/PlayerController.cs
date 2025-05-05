using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _jumpForce = 7f;
    [SerializeField] private float _turnSpeed = 720f;
    [SerializeField] private int _maxJumpCount = 2;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Grabbing")]
    [SerializeField] private Transform grabPoint;
    [SerializeField] private float grabRange = 1.5f;
    [SerializeField] private LayerMask grabbableLayer;

    [Header("Visual")]
    [SerializeField] private Renderer bodyRenderer;

    [Header("Tether Pull Settings")]
    [SerializeField] private float basePullForce = 15f;
    [SerializeField] private float excessMultiplier = 20f;
    [SerializeField] private float maxPullCap = 100f;

    [Header("Custom Gravity")]
    [SerializeField] private float customGravity = -9.81f;
    [SerializeField] private Vector3 gravityDirection = Vector3.down;

    [Header("Jump Feel Tweaks")]
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    private Color _baseColor;
    private Vector3 _input;
    private int _jumpCount;
    private bool _jumpTriggered;
    private bool _jumpHeld;
    private PlayerInputController _playerInputController;
    private FixedJoint grabJoint;
    private Rigidbody grabbedRb;
    private MovingPlatform _currentPlatform;

    private PlayerTetherManager _tetherManager;
    private float _desiredMaxDistance;
    private bool _tetherControlActive = false;
    private bool _isAnchored = false;

    private float _previousStickAngle = 0f;
    private float _cwRotated = 0f;
    private float _ccwRotated = 0f;

    private void Awake()
    {
        _playerInputController = GetComponent<PlayerInputController>();
        _playerInputController.OnJumpButtonPressed += JumpButtonPressed;
        _playerInputController.OnJumpReleased += () => _jumpHeld = false;
        _playerInputController.OnGrabInputChanged += HandleGrab;
        _playerInputController.OnTetherControlChanged += OnTetherControlChanged;
        _playerInputController.OnLaunchPressed += TryLaunchTowardPartner;

        _tetherManager = FindObjectOfType<PlayerTetherManager>();
        _desiredMaxDistance = _tetherManager.baseMaxDistance;

        if (bodyRenderer != null)
            _baseColor = bodyRenderer.material.color;

        _rb.useGravity = false;
    }

    void FixedUpdate()
    {
        if (_isAnchored)
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        Move();
        GatherInput();
        Look();

        if (IsGrounded())
        {
            _jumpCount = 0;
            _currentPlatform = null;
        }

        if (_jumpTriggered && !_isAnchored)
        {
            _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
            _jumpTriggered = false;
        }

        // Better jump fall behavior
        if (_rb.velocity.y < 0)
        {
            _rb.AddForce(gravityDirection * customGravity * (fallMultiplier - 1f), ForceMode.Acceleration);
        }
        else if (_rb.velocity.y > 0 && !_jumpHeld)
        {
            _rb.AddForce(gravityDirection * customGravity * (lowJumpMultiplier - 1f), ForceMode.Acceleration);
        }

        // Tether logic...
        if (_tetherControlActive && _tetherManager != null)
        {
            Vector2 stick = _playerInputController.RightStickInputVector;

            if (stick.magnitude > 0.3f)
            {
                float angle = Mathf.Atan2(stick.y, stick.x) * Mathf.Rad2Deg;
                float deltaAngle = Mathf.DeltaAngle(_previousStickAngle, angle);

                if (deltaAngle < 0)
                {
                    _cwRotated += -deltaAngle;
                    _ccwRotated = 0f;
                }
                else if (deltaAngle > 0)
                {
                    _ccwRotated += deltaAngle;
                    _cwRotated = 0f;
                }

                _previousStickAngle = angle;

                if (_cwRotated >= 90f)
                {
                    _desiredMaxDistance = Mathf.Clamp(
                        _tetherManager.currentMaxDistance - 0.5f,
                        _tetherManager.minMaxDistance,
                        _tetherManager.baseMaxDistance
                    );
                    _cwRotated = 0f;
                }

                if (_ccwRotated >= 90f)
                {
                    _desiredMaxDistance = Mathf.Clamp(
                        _tetherManager.currentMaxDistance + 0.5f,
                        _tetherManager.minMaxDistance,
                        _tetherManager.baseMaxDistance
                    );
                    _ccwRotated = 0f;
                }
            }
            else
            {
                _previousStickAngle = 0f;
                _cwRotated = 0f;
                _ccwRotated = 0f;
            }

            Transform otherPlayer = (_tetherManager.player1 == transform) ? _tetherManager.player2 : _tetherManager.player1;
            Rigidbody otherRb = otherPlayer.GetComponent<Rigidbody>();

            if (otherRb != null)
            {
                Vector3 dir = transform.position - otherPlayer.position;
                float dist = dir.magnitude;

                if (dist > _tetherManager.currentMaxDistance + 0.1f)
                {
                    Vector3 pullDir = dir.normalized;
                    float excess = dist - _tetherManager.currentMaxDistance;
                    float pullStrength = basePullForce + Mathf.Clamp(excess * excessMultiplier, 0, maxPullCap);
                    otherRb.AddForce(pullDir * pullStrength * Time.fixedDeltaTime, ForceMode.Force);
                }
            }
        }
        else
        {
            _desiredMaxDistance = Mathf.MoveTowards(
                _tetherManager.currentMaxDistance,
                _tetherManager.baseMaxDistance,
                _tetherManager.maxAdjustSpeed * Time.fixedDeltaTime
            );
        }

        _tetherManager.currentMaxDistance = _desiredMaxDistance;

        // Custom gravity always applied
        _rb.AddForce(gravityDirection * customGravity, ForceMode.Acceleration);
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
        if (_isAnchored) return;

        Vector3 platformDelta = _currentPlatform != null ? _currentPlatform.DeltaMovement : Vector3.zero;
        Vector3 movement = (transform.forward * _input.magnitude) * _speed * Time.deltaTime;
        _rb.MovePosition(transform.position + movement + platformDelta);
    }

    private void JumpButtonPressed()
    {
        if (_jumpCount < _maxJumpCount && !_isAnchored)
        {
            _jumpTriggered = true;
            _jumpCount++;
            _jumpHeld = true;
        }
    }

    private void OnTetherControlChanged(bool isHeld)
    {
        _tetherControlActive = isHeld;

        if (isHeld && IsGrounded())
        {
            _isAnchored = true;
        }
        else
        {
            _isAnchored = false;
        }

        if (bodyRenderer != null && bodyRenderer.material.HasProperty("_Color"))
        {
            bodyRenderer.material.color = _isAnchored ? Color.green : _baseColor;
        }
    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, _groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }

    public bool IsAnchored()
    {
        return _isAnchored;
    }

    private void TryLaunchTowardPartner()
    {
        if (_tetherManager == null) return;

        Transform otherPlayer = (_tetherManager.player1 == transform) ? _tetherManager.player2 : _tetherManager.player1;
        PlayerController otherController = otherPlayer.GetComponent<PlayerController>();

        if (otherController != null && otherController.IsAnchored())
        {
            Vector3 dir = (otherPlayer.position - transform.position).normalized;
            Vector3 launchDir = (dir + Vector3.up * 0.5f).normalized;
            float launchForce = 18f;

            _rb.velocity = Vector3.zero;
            _rb.AddForce(launchDir * launchForce, ForceMode.VelocityChange);
        }
    }
}
