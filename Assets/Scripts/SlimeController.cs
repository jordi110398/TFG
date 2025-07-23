using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SlimeController : EnemyController
{
    public GameObject pointA;
    public GameObject pointB;
    private Transform currentPoint;
    public float speed;
    private Coroutine walkSoundCoroutine;

    protected override void Start()
    {
        base.Start();
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
        if (isDead) return;
        if (isBlocked || isKnockbacked)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("isRunning", false);
            if (walkSoundCoroutine != null)
            {
                StopCoroutine(walkSoundCoroutine);
                walkSoundCoroutine = null;
            }
            return;

        }

        anim.SetBool("isRunning", true);

        // Inicia la coroutine si no està en marxa
        if (walkSoundCoroutine == null)
            walkSoundCoroutine = StartCoroutine(PlayWalkSound());

        // Mou correctament el slime
        transform.position = Vector2.MoveTowards(transform.position, currentPoint.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, currentPoint.position) < 0.5f)
        {
            currentPoint = currentPoint == pointB.transform ? pointA.transform : pointB.transform;
        }

        // Ajustar la direcció del sprite
        if (spriteRenderer != null)
            spriteRenderer.flipX = currentPoint == pointA.transform;
    }
    private IEnumerator PlayWalkSound()
    {
        while (true)
        {
            audioSource.PlayOneShot(AudioManager.Instance.enemyJump);
            yield return new WaitForSeconds(0.6f); // Ajusta l'interval al teu gust
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(pointA.transform.position, 0.5f);
        Gizmos.DrawWireSphere(pointB.transform.position, 0.5f);
        Gizmos.DrawLine(pointA.transform.position, pointB.transform.position);
    }

    public override void TakeDamage(float amount, Transform attacker)
    {
        base.TakeDamage(amount, attacker);

        if (audioSource != null && AudioManager.Instance.enemyHurt != null)
            audioSource.PlayOneShot(AudioManager.Instance.enemyHurt);
    }

    protected override void Die()
    {
        base.Die(); // <-- Aquesta línia garanteix que s'instanciïn les partícules i es faci la lògica comuna
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.None; // Permet rotació i moviment lliure
    }
}
