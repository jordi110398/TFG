using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float curveStrength = 3f; // Força de curvatura aplicada a la trajectòria del projectil
    public GameObject explosionPrefab; // Prefab d'explosió a instanciar al col·lisionar
    public float projectileDamage = 1f; // Dany que infligeix el projectil

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // Calcula la direcció perpendicular a la velocitat actual
        Vector2 perp = Vector2.Perpendicular(rb.linearVelocity).normalized;
        rb.AddForce(perp * curveStrength);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Player2"))
        {
            // Comprova si Player2 està bloquejant
            if (collision.gameObject.CompareTag("Player2"))
            {
                var player2 = collision.gameObject.GetComponent<Player2Controller>();
                if (player2 != null && player2.IsBlocking())
                {
                    // Opcional: efecte de bloqueig
                    player2.PlayBlockingFlash();
                    Detonate();
                    return;
                }
            }

            // Dany
            GameObject playerManager = GameObject.FindWithTag("PlayerManager");
            if (playerManager != null)
            {
                string playerTag = collision.gameObject.CompareTag("Player1") ? "Player1" : "Player2";
                playerManager.GetComponent<HealthSystem>().TakeDamage(playerTag, projectileDamage);
            }

            // Knockback
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                float knockbackForce = 30f; // Ajusta la força segons el teu joc
                playerRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
            }
        }
        Detonate();
    }

    public void Detonate()
    {
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            // So d'explosió
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.explosionSound, 0.2f);
        Destroy(gameObject);
    }
}
