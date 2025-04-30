using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour
{
    public float moveDistance = 3f;
    public float moveSpeed = 2f;
    public float pauseDuration = 1f;

    private Vector3 _startPos;
    private Vector3 _endPos;
    private Vector3 _targetPos;
    private Vector3 _lastPosition;
    public Vector3 DeltaMovement { get; private set; }

    private Rigidbody _rb;
    private bool _isPaused = false;

    private void Start()
    {
        _startPos = transform.position;
        _endPos = _startPos + Vector3.up * moveDistance;
        _targetPos = _endPos;

        _lastPosition = transform.position;

        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;

        StartCoroutine(SwitchDirection());
    }

    private void FixedUpdate()
    {
        if (_isPaused) return;

        Vector3 newPos = Vector3.MoveTowards(transform.position, _targetPos, moveSpeed * Time.fixedDeltaTime);
        _rb.MovePosition(newPos);
    }

    private void LateUpdate()
    {
        DeltaMovement = transform.position - _lastPosition;
        _lastPosition = transform.position;
    }

    private IEnumerator SwitchDirection()
    {
        while (true)
        {
            // Wait until platform reaches target
            while (Vector3.Distance(transform.position, _targetPos) > 0.01f)
            {
                yield return null;
            }

            // Snap to position and pause
            _rb.MovePosition(_targetPos);
            _isPaused = true;
            yield return new WaitForSeconds(pauseDuration);

            // Flip direction and resume
            _targetPos = (_targetPos == _startPos) ? _endPos : _startPos;
            _isPaused = false;
        }
    }
}
