using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PlayerTetherManager : MonoBehaviour
{
    public Transform player1;
    public Transform player2;
    public float maxDistance = 5f;

    public int ropeSegments = 20;
    public float ropeSag = 0.5f;
    public GameObject hitEffectPrefab;

    private LineRenderer line;

    private void Awake()
    {
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

        // Calculate rope color based on tension
        float tension = Mathf.Clamp01(currentDistance / maxDistance); // 0 = close, 1 = max
        Color ropeColor = Color.Lerp(Color.yellow, Color.red, tension);
        line.startColor = ropeColor;
        line.endColor = ropeColor;

        DrawRope(start, end);

        // Enforce max distance
        if (currentDistance > maxDistance)
        {
            Vector3 correction = (start - end).normalized * (currentDistance - maxDistance) / 2;
            player1.position -= correction;
            player2.position += correction;
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

        // Starting from segment 1, cast ray between previous and current segment
        if (i > 0)
        {
            Vector3 prevPoint = line.GetPosition(i - 1);
            Vector3 dir = point - prevPoint;
            float dist = dir.magnitude;
            Ray ray = new Ray(prevPoint, dir.normalized);

            if (Physics.Raycast(ray, out RaycastHit hit, dist))
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
