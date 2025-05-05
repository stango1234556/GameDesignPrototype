using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PlayerTetherManager : MonoBehaviour
{
    public Transform player1;
    public Transform player2;

    public float baseMaxDistance = 6f;
    public float minMaxDistance = 0.25f;
    public float maxAdjustSpeed = 2f;

    public int ropeSegments = 20;
    public float ropeSag = 0.5f;
    public GameObject hitEffectPrefab; 

    [HideInInspector] public float currentMaxDistance;

    private LineRenderer line;

    private void Awake()
    {
        currentMaxDistance = baseMaxDistance;
        line = GetComponent<LineRenderer>();
        line.positionCount = ropeSegments;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
    }

    private void FixedUpdate()
    {
        if (player1 == null || player2 == null) return;

        Vector3 start = player1.position;
        Vector3 end = player2.position;
        float currentDistance = Vector3.Distance(start, end);

        // Tension color (bright red to darker red)
        float tension = Mathf.Clamp01(currentDistance / currentMaxDistance);
        Color brightRed = new Color(1f, 0f, 0f);
        Color darkRed = new Color(0.6f, 0.1f, 0.1f);
        Color ropeColor = Color.Lerp(brightRed, darkRed, tension);
        line.startColor = ropeColor;
        line.endColor = ropeColor;

        DrawRope(start, end);

        // Soft enforcement
        if (currentDistance > currentMaxDistance)
        {
            Vector3 direction = (start - end).normalized;
            float excess = currentDistance - currentMaxDistance;

            Rigidbody rb1 = player1.GetComponent<Rigidbody>();
            Rigidbody rb2 = player2.GetComponent<Rigidbody>();

            if (rb1 != null) rb1.AddForce(-direction * excess * 2f);
            if (rb2 != null) rb2.AddForce(direction * excess * 2f);
        }
    }

    private void DrawRope(Vector3 start, Vector3 end)
    {
        for (int i = 0; i < ropeSegments; i++)
        {
            float t = i / (float)(ropeSegments - 1);
            Vector3 point = Vector3.Lerp(start, end, t);
            float sagOffset = Mathf.Sin(Mathf.PI * t) * ropeSag;
            point.y -= sagOffset;
            line.SetPosition(i, point);

            if (i > 0)
            {
                Vector3 prevPoint = line.GetPosition(i - 1);
                Vector3 dir = point - prevPoint;
                float dist = dir.magnitude;

                if (Physics.Raycast(prevPoint, dir.normalized, out RaycastHit hit, dist))
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        Destroy(hit.collider.gameObject);
                        Instantiate(hitEffectPrefab, hit.point, Quaternion.identity);
                    }
                }
            }
        }
    }
}
