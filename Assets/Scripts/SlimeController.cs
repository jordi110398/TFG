using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SlimeController : EnemyController
{
    public GameObject pointA;
    public GameObject pointB;
    private Transform currentPoint;
    public float speed;

    protected override void Start()
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
        if(isDead) return;
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
        if (spriteRenderer != null)
            spriteRenderer.flipX = currentPoint == pointA.transform;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(pointA.transform.position, 0.5f);
        Gizmos.DrawWireSphere(pointB.transform.position, 0.5f);
        Gizmos.DrawLine(pointA.transform.position, pointB.transform.position);
    }

    protected override void Die()
    {
        base.Die(); // <-- Aquesta línia garanteix que s'instanciïn les partícules i es faci la lògica comuna
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.None; // Permet rotació i moviment lliure
    }
}
