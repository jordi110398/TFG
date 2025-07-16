using UnityEngine;

public class TrapArrow : MonoBehaviour
{
    public float damage = 1f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player1") || collision.collider.CompareTag("Player2"))
        {
            // Dany al jugador (com ja tens)
            HealthSystem health = FindAnyObjectByType<HealthSystem>();
            if (health != null)
            {
                if (collision.collider.CompareTag("Player1"))
                    health.TakeDamage("Player1", damage);
                else if (collision.collider.CompareTag("Player2"))
                    health.TakeDamage("Player2", damage);
            }
            Destroy(gameObject); // Destrueix la fletxa si toca un jugador
        }
        else if (collision.collider.CompareTag("Shield"))
        {
            StickArrow(collision.contacts[0].point, collision.collider.transform, collision.contacts[0].normal);
        }
        else
        {
            StickArrow(collision.contacts[0].point, collision.collider.transform, collision.contacts[0].normal);
        }
    }

    private void StickArrow(Vector2 hitPoint, Transform parent, Vector2 normal)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic; // Canvia el tipus de cos a cinemàtic
            rb.freezeRotation = true; // Bloqueja la rotació
        }
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        transform.parent = parent;
        transform.position = hitPoint;

        // Ajusta la rotació perquè la fletxa quedi perpendicular a la superfície
        float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Destroy(gameObject, 10f);
    }
}
