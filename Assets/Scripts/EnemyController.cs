using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Vida")]
    public float maxHealth = 5f;
    protected float currentHealth;

    [Header("Dany")]
    public float damageAmount = 1f;
    public float damageCooldown = 1f;
    protected float nextDamageTime;
    protected bool isDead = false;

    [Header("Knockback")]
    public float knockbackForce = 20f;
    protected bool isKnockbacked = false;

    [Header("Damage Flash")]
    public SpriteRenderer spriteRenderer;
    public Color flashColor = Color.red;
    public float flashDuration = 0.1f;
    protected Color originalColor;
    private Coroutine pulseCoroutine;

    // Dead FX
    public GameObject deathParticlesPrefab;

    [Header("Player Manager")]
    // Referència al PlayerManager
    public GameObject playerManager;
    protected bool isBlocked = false;
    protected bool isInvincible = false;
    protected Animator anim;
    protected Rigidbody2D rb;

    [Header("Drops")]
    public GameObject[] dropPrefabs; // Assigna els objectes a dropejar a l'Inspector
    public Transform dropPoint; // Opcional: punt concret per fer el drop

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
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

                if (collision.CompareTag("Player1") && playerController1 != null)
                {
                    if (!playerController1.IsInvincible())
                    {
                        playerManager.GetComponent<HealthSystem>().TakeDamage("Player1", damageAmount);
                        anim.SetBool("isAttacking", true);

                        // Knockback direcció correcta
                        Vector2 knockDir = (playerController1.transform.position - transform.position).normalized;
                        playerController1.ApplyKnockback(knockDir, 3f);
                        StartCoroutine(playerController1.PlayDamageFlash());
                    }
                }
                else if (collision.CompareTag("Player2") && playerController2 != null)
                {
                    if (!playerController2.IsInvincible())
                    {
                        playerManager.GetComponent<HealthSystem>().TakeDamage("Player2", damageAmount);
                        anim.SetBool("isAttacking", true);

                        Vector2 knockDir = (playerController2.transform.position - transform.position).normalized;
                        playerController2.ApplyKnockback(knockDir, 3f);
                        StartCoroutine(playerController2.PlayDamageFlash());
                    }
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

    public virtual void TakeDamage(float amount, Transform attacker)
    {
        if (isKnockbacked) return;
        currentHealth -= amount;
        StartCoroutine(PlayDamageFlash());
        StartCoroutine(PlayDamagePulse());
        Debug.Log($"{gameObject.name} ha rebut mal.");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            ApplyKnockback(attacker);
        }
    }

    protected virtual void ApplyKnockback(Transform attacker)
    {
        Vector2 knockbackDir = (transform.position - attacker.position).normalized;
        rb.linearVelocity = Vector2.zero;
        isKnockbacked = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        StartCoroutine(HandleKnockback(0.2f));
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
        yield return new WaitForSeconds(0.1f);
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    protected virtual IEnumerator HandleKnockback(float duration)
    {
        yield return new WaitForSeconds(duration);
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        isKnockbacked = false;
    }

    protected virtual IEnumerator PlayDamageFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    protected virtual IEnumerator PlayDamagePulse()
    {
        Vector3 originalScale = transform.localScale;
        float pulseScale = 1.2f;
        float pulseDuration = 0.1f;

        transform.localScale = originalScale * pulseScale;
        yield return new WaitForSeconds(pulseDuration);
        transform.localScale = originalScale;
    }

    public void TriggerDamagePulse()
    {
        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PlayDamagePulse());
    }
    protected virtual void Die()
    {
        if (deathParticlesPrefab != null)
        {
            Debug.Log("Instanciant partícules de mort!", this);
            Instantiate(deathParticlesPrefab, transform.position, Quaternion.identity);
        }

        // DROP D'OBJECTES
        if (dropPrefabs != null)
        {
            foreach (var prefab in dropPrefabs)
            {
                if (prefab != null)
                {
                    Vector3 pos = dropPoint != null ? dropPoint.position : transform.position;
                    Instantiate(prefab, pos, Quaternion.identity);
                }
            }
        }

        isDead = true;
        if (anim != null)
            anim.SetTrigger("isDead");
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic; // No es mou més
        // Desactiva només el collider de trigger
        foreach (var col in GetComponents<Collider2D>())
        {
            if (col.isTrigger)
                col.enabled = false;
        }
        StartCoroutine(HandleDeath());
        Debug.Log($"{gameObject.name} ha mort.");
    }

    protected virtual IEnumerator HandleDeath()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}

