using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SlimeController : MonoBehaviour
{
    public GameObject pointA;
    public GameObject pointB;
    private Rigidbody2D rb;
    private Animator anim;
    private Transform currentPoint;
    public float speed;
    // VIDA DEL SLIME
    public float slimeHealth = 5f; // Vida inicial del Slime


    // DAMAGE VARIABLES
    public float damageAmount = 1f; // Quantitat de mal que fa el Slime
    private float damageCooldown = 1f; // Temps d'espera entre danys
    private float nextDamageTime;

    // Variables pel Flash
    private SpriteRenderer spriteRenderer;
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;
    private Color originalColor;

    // Referència al PlayerManager
    public GameObject playerManager;
    private bool isBlocked = false;
    private bool isKnockbacked = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentPoint = pointB.transform;
        anim.SetBool("isRunning", true);

        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        if (playerManager == null)
        {
            Debug.LogError("PlayerManager no trobat!");
        }
    }

    private void Update()
    {
        if (isBlocked || isKnockbacked)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("isRunning", false);
            return;
        }

        anim.SetBool("isRunning", true);

        Vector2 direction = (currentPoint.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * speed, 0);

        if (Vector2.Distance(transform.position, currentPoint.position) < 0.5f)
        {
            currentPoint = currentPoint == pointB.transform ? pointA.transform : pointB.transform;
        }

        // Ajustar la direcció del sprite
        spriteRenderer.flipX = currentPoint == pointA.transform;
    }

    public void TakeDamage(float amount, Transform attacker)
    {
        if (isKnockbacked) return; // Evitar múltiples knockbacks simultanis

        slimeHealth -= amount;
        StartCoroutine(PlayDamageFlash());

        if (slimeHealth <= 0)
        {
            Die();
        }
        else
        {
            // Knockback
            Vector2 knockbackDirection = (transform.position - attacker.position).normalized;
            float knockbackForce = 20f;

            rb.linearVelocity = Vector2.zero;          // Atura el moviment anterior
            isKnockbacked = true;                // Marca com a knockback actiu
            rb.bodyType = RigidbodyType2D.Dynamic; // Permet forces físiques
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

            StartCoroutine(HandleKnockback(0.5f));
        }
    }

    private IEnumerator HandleKnockback(float duration)
    {
        yield return new WaitForSeconds(duration);

        rb.linearVelocity = Vector2.zero;                // Atura el moviment residual
        rb.bodyType = RigidbodyType2D.Kinematic;   // Torna al mode de moviment patrullant
        isKnockbacked = false;
    }

    public void Die()
    {
        anim.SetTrigger("isDead");
        rb.linearVelocity = Vector2.zero;
        isBlocked = true;
        anim.SetBool("isRunning", false);

        GetComponent<Collider2D>().enabled = false;
        StartCoroutine(HandleDeath());
    }

    private IEnumerator HandleDeath()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

    public IEnumerator PlayDamageFlash()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    public void ReceiveKnockback(Transform attacker)
    {
        Vector2 knockDir = (transform.position - attacker.position).normalized;
        rb.bodyType = RigidbodyType2D.Dynamic; // Temporàriament si cal
        rb.AddForce(knockDir * 10f, ForceMode2D.Impulse); // Ajusta la força

        // Opcional: tornar a Kinematic després
        StartCoroutine(ResetToKinematic());
    }

    private IEnumerator ResetToKinematic()
    {
        yield return new WaitForSeconds(0.3f);
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if ((collision.CompareTag("Player1") || collision.CompareTag("Player2")) && Time.time >= nextDamageTime)
        {
            nextDamageTime = Time.time + damageCooldown;

            Player1Controller playerController1 = collision.GetComponent<Player1Controller>();
            Player2Controller playerController2 = collision.GetComponent<Player2Controller>();

            if (playerManager != null)
            {
                if (playerController2 != null && playerController2.IsBlocking())
                {
                    isBlocked = true;
                    anim.SetBool("isRunning", false);
                    anim.SetBool("isAttacking", false);
                    StartCoroutine(playerController2.PlayBlockingFlash());
                    return;
                }
                else
                {
                    isBlocked = false;
                }

                if (collision.CompareTag("Player1"))
                {
                    playerManager.GetComponent<HealthSystem>().TakeDamage("Player1", damageAmount);
                    anim.SetBool("isAttacking", true);
                }
                else if (collision.CompareTag("Player2"))
                {
                    playerManager.GetComponent<HealthSystem>().TakeDamage("Player2", damageAmount);
                    anim.SetBool("isAttacking", true);
                }

                if (playerController2 != null && !playerController2.IsBlocking())
                {
                    playerController2.ApplyKnockback(Vector2.up, 3f);
                    StartCoroutine(playerController2.PlayDamageFlash());
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player1") || collision.CompareTag("Player2"))
        {
            isBlocked = false;
            anim.SetBool("isAttacking", false);
            anim.SetBool("isRunning", true);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(pointA.transform.position, 0.5f);
        Gizmos.DrawWireSphere(pointB.transform.position, 0.5f);
        Gizmos.DrawLine(pointA.transform.position, pointB.transform.position);
    }
}
