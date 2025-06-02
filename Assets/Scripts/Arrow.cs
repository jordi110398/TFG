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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (!hasHit)
            trackMovement();
    }

    void trackMovement()
    {
        Vector2 direction = rb.linearVelocity;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        if (collision.CompareTag("Enemy"))
        {
            SlimeController enemy = collision.GetComponent<SlimeController>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage, damageZone);
                Debug.Log("Fletxa impacta enemic!");

                // Clavar la fletxa a l’enemic
                FlashWhite(); // Efecte flash a la fletxa
                PulseEffect(); // Pulsació
                hasHit = true;
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
                // Evita deformacions per escala del parent
                transform.SetParent(null); // desparenta
                Vector3 hitPos = transform.position; // guarda posició
                transform.SetParent(collision.transform, true); // reparenta mantenint posició mundial
                transform.position = hitPos; // reforça posició
                transform.localScale = originalScale; // restaura escala

                col.enabled = false;

                // Comença vibració
                StartCoroutine(VibrateEffect(0.1f, 0.1f));
            }
        }
        else if (!collision.CompareTag("Player"))
        {
            hasHit = true;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            col.enabled = false;
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

}
