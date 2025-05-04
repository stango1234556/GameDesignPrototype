using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Tooltip("Time in seconds before the projectile self-destructs.")]
    public float lifetime = 6f;

    [Tooltip("Amount of damage dealt to the player (optional).")]
    public int damage = 1;

    private void Start()
    {
        // Destroy this projectile after 'lifetime' seconds
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Uncomment and customize if you have a PlayerHealth script
            // collision.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(damage);
        }

        // Destroy on any collision
        Destroy(gameObject);
    }
}
