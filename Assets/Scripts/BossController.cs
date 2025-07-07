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
    public float projectileSpeed = 2f;
    private bool isVulnerable = false; // l'escut es pot destruir amb les fletxes carregades o un atac carregat del P2

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        shieldParticles.SetActive(false); // per defecte, desactivat
        StartCoroutine(BossRoutine());
    }

    private IEnumerator BossRoutine()
    {
        while (!isDead)
        {
            // Fase d'atac
            anim.SetBool("isAngry", true); // canvia a estat Boss_Attack
            shieldParticles.SetActive(true); // s'activa l'escut
            yield return StartCoroutine(FireProjectiles());
            anim.SetBool("isAngry", false);

            // Fase vulnerable
            isVulnerable = true;
            shieldParticles.SetActive(false); // Desactiva les partícules de l'escut
            yield return new WaitForSeconds(vulnerableDuration);
            isVulnerable = false;
        }
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
    }

    private void ShootInAllDirections()
    {
        Debug.Log("Boss està disparant projectils en totes direccions");
        float angleStep = 360f / numberOfProjectiles;
        for (int i = 0; i < numberOfProjectiles; i++)
        {
            float angle = i * angleStep;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            projectile.GetComponent<Rigidbody2D>().linearVelocity = direction.normalized * projectileSpeed;
        }
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

        int fanCount = 5; // Nombre de projectils per jugador
        float fanAngle = 40f; // Amplada total del ventall (en graus)

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
        if (!isVulnerable || isDead) return; // Només rep mal quan està vulnerable
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
