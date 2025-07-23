using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour
{
    public int damage = 10;
    public float destroyDelay = 0.1f;
    public Transform damageZone;

    private bool hasHit = false;
    private Rigidbody2D rb;
    private Collider2D col;
    Vector3 originalScale;
    public SpriteRenderer sr;
    public GameObject auraPrefab; // Prefab per l'aura amb el buff

    // SO D'IMPACTE
    public AudioClip impactSound;
    private AudioSource audioSource;

    public bool isChargedArrow = false; // Indica si la fletxa és carregada
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!hasHit)
            trackMovement();
    }

    void trackMovement()
    {
        Vector2 direction = rb.linearVelocity;
        // Si la velocitat és molt petita, no cal girar
        if (direction.sqrMagnitude < 0.01f) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        // 1. Comprova si col·lisiona amb l'escut del Boss
        BossShield bossShield = collision.GetComponent<BossShield>();
        if (bossShield != null)
        {
            // Només trenca l'escut si la fletxa és carregada i el buff està actiu
            if (isChargedArrow)
            {
                Debug.Log("Fletxa carregada col·lisiona amb BossShield!");
                var p1 = FindAnyObjectByType<Player1Controller>();
                if (p1 != null && p1.TryGetComponent(out BattleCry battleCry) && battleCry.IsBuffActive())
                {
                    bossShield.TryBreakShield(p1.gameObject);
                    Destroy(gameObject);
                    return;
                }
            }
        }

        if (collision.CompareTag("Enemy"))
        {
            EnemyController enemy = collision.GetComponent<EnemyController>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage, damageZone);
                Debug.Log("Fletxa impacta enemic!");

                // Atura moviment
                hasHit = true;
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
                col.enabled = false;

                // Efectes visuals
                FlashWhite();
                PulseEffect();

                // Comença vibració i després es destrueix
                StartCoroutine(ImpactAndDestroy());
            }
        }
        else if (collision.CompareTag("Projectile"))
        {
            Destroy(collision.gameObject); // Destrueix el projectil amb el qual col·lisiona
        }
        else if (!collision.CompareTag("Player1") && !collision.CompareTag("Player2"))
        {
            // Si col·lisiona amb qualsevol altre objecte, atura el moviment
            hasHit = true;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            col.enabled = false;

            // Efectes visuals
            FlashWhite();
            PulseEffect();

            audioSource.PlayOneShot(impactSound);

            // Comença vibració i després es destrueix
            StartCoroutine(ImpactAndDestroy());
        }
    }

    System.Collections.IEnumerator VibrateEffect(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-magnitude, magnitude);
            float offsetY = Random.Range(-magnitude, magnitude);
            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos; // torna a la posició original
    }

    // FLASH BLANC FLETXA + PULSACIÓ
    public void FlashWhite()
    {
        StartCoroutine(FlashCoroutine());
    }

    IEnumerator FlashCoroutine()
    {
        Color originalColor = sr.color;
        sr.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        sr.color = originalColor;
    }
    public void PulseEffect()
    {
        StartCoroutine(PulseCoroutine());
    }

    IEnumerator PulseCoroutine()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.2f; // augmenta un 20%

        float duration = 0.1f;
        float time = 0f;

        // Escala cap amunt
        while (time < duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;

        // Escala cap avall
        time = 0f;
        while (time < duration)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
    }

    IEnumerator ImpactAndDestroy()
    {
        yield return VibrateEffect(0.1f, 0.1f);
        float waitTime = Mathf.Max(impactSound.length, 0.1f);
        yield return new WaitForSeconds(waitTime);
        Destroy(gameObject);
    }


}
