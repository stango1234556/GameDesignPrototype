using System.Collections;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float shootingInterval = 2f;
    public float range = 20f;

    private Transform targetPlayer;

    public float lifetime = 3f;


    private void Start()
    {
        StartCoroutine(ShootAtPlayer());
    }

    private void Update()
    {
        UpdateTarget();
        AimAtTarget();
    }

    private void UpdateTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < closestDistance && dist <= range)
            {
                closestDistance = dist;
                targetPlayer = player.transform;
            }
        }
    }

    private void AimAtTarget()
    {
        if (targetPlayer != null)
        {
            Vector3 direction = (targetPlayer.position - transform.position).normalized;
            direction.y = 0; // keep it flat
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private IEnumerator ShootAtPlayer()
    {
        while (true)
        {
            if (targetPlayer != null)
            {
                GameObject proj = Instantiate(projectilePrefab, transform.position + transform.forward, Quaternion.identity);
                Rigidbody rb = proj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = transform.forward * projectileSpeed;
                }
            }

            yield return new WaitForSeconds(shootingInterval);
        }
    }
}