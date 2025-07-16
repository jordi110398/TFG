using UnityEngine;

public class TrapArrow : MonoBehaviour
{
    public float damage = 1f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player1") || collision.collider.CompareTag("Player2"))
        {
            HealthSystem health = FindAnyObjectByType<HealthSystem>();
            if (health != null)
            {
                if (collision.collider.CompareTag("Player1"))
                    health.TakeDamage("Player1", damage);
                else if (collision.collider.CompareTag("Player2"))
                    health.TakeDamage("Player2", damage);
            }
        }
        Destroy(gameObject); // Destrueix la fletxa després de col·lidir
    }
}
