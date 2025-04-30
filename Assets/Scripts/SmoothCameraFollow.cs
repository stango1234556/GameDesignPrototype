using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    #region Variables

    private Vector3 _offset;
    [SerializeField] private Transform target;
    [SerializeField] private float speedFactor = 5f; // speedFactor replaces smoothTime

    #endregion

    #region Unity callbacks

    private void Awake() => _offset = transform.position - target.position;

    private void LateUpdate()
    {
        Vector3 targetPosition = target.position + _offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, speedFactor * Time.deltaTime);
    }

    #endregion
}
