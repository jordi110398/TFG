using UnityEngine;
using System.Collections;

public class BossController : EnemyController
{
    [Header("Boss Settings")]
    public GameObject projectilePrefab;
    public GameObject shieldParticles;
    public Transform firePoint;
    public int numberOfProjectiles = 8;
    public float fireInterval = 0.2f;
    public float attackDuration = 4f;
    public float vulnerableDuration = 3f;
    public float patrolDuration = 3f;
    public float projectileSpeed = 2f;
    public float patrolSpeed = 1.5f;
    public GameObject pointA;
    public GameObject pointB;

    private bool isVulnerable = false;
    private Transform currentPoint;
    private bool isPatrolling = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        shieldParticles.SetActive(false);
        currentPoint = pointB.transform;
        StartCoroutine(BossRoutine());
    }

    private IEnumerator BossRoutine()
    {
        while (!isDead)
        {
            // --- Fase 1: Patrulla invulnerable ---
            isVulnerable = false;
            isPatrolling = true;
            shieldParticles.SetActive(true);
            anim.SetBool("isPatrolling", true);
            anim.SetBool("isAttacking", false);
            anim.SetBool("isAngry", false);

            float patrolTimer = 0f;
            while (patrolTimer < patrolDuration)
            {
                PatrolMove();
                patrolTimer += Time.deltaTime;
                yield return null;
            }
            rb.linearVelocity = Vector2.zero;
            isPatrolling = false;
            anim.SetBool("isPatrolling", false);

            // --- Fase 2: Atac vulnerable ---
            isVulnerable = true;
            shieldParticles.SetActive(false);
            anim.SetBool("isAttacking", true);
            anim.SetBool("isAngry", true);

            yield return StartCoroutine(FireProjectiles());

            anim.SetBool("isAttacking", false);
            anim.SetBool("isAngry", false);

            // --- Fase 3: Idle vulnerable (opcional, pausa quiet) ---
            yield return new WaitForSeconds(vulnerableDuration);
        }
    }

    private void PatrolMove()
    {
        if (isDead) return;
        Vector2 direction = (currentPoint.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * patrolSpeed, 0);

        if (Vector2.Distance(transform.position, currentPoint.position) < 0.5f)
        {
            currentPoint = currentPoint == pointB.transform ? pointA.transform : pointB.transform;
        }

        // Flip del sprite segons la direcció
        if (spriteRenderer != null)
            spriteRenderer.flipX = currentPoint == pointA.transform;
    }

    private IEnumerator FireProjectiles()
    {
        float elapsed = 0f;
        while (elapsed < attackDuration)
        {
            ShootAtPlayers();
            yield return new WaitForSeconds(fireInterval);
            elapsed += fireInterval;
        }
        rb.linearVelocity = Vector2.zero;
    }

    private void ShootAtPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player1");
        GameObject player2 = GameObject.FindGameObjectWithTag("Player2");
        if (player2 != null)
        {
            var tempList = new System.Collections.Generic.List<GameObject>(players);
            tempList.Add(player2);
            players = tempList.ToArray();
        }

        int fanCount = 5;
        float fanAngle = 40f;

        foreach (GameObject player in players)
        {
            if (player == null) continue;
            Vector2 toPlayer = (player.transform.position - firePoint.position).normalized;
            float baseAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            float startAngle = baseAngle - fanAngle / 2f;
            float angleStep = fanAngle / (fanCount - 1);

            for (int i = 0; i < fanCount; i++)
            {
                float angle = startAngle + angleStep * i;
                Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
                projectile.GetComponent<Rigidbody2D>().linearVelocity = direction.normalized * projectileSpeed;
            }
        }
    }

    public override void TakeDamage(float amount, Transform attacker)
    {
        if (!isVulnerable || isDead) return;
        base.TakeDamage(amount, attacker);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            Collider2D bossCol = GetComponent<Collider2D>();
            Collider2D projCol = collision.collider;
            if (bossCol != null && projCol != null)
                Physics2D.IgnoreCollision(projCol, bossCol);
        }
    }
}
