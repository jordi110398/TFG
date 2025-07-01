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
    public float projectileSpeed = 5f;
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
            ShootInAllDirections();
            yield return new WaitForSeconds(fireInterval);
            elapsed += fireInterval;
        }
    }

    private void ShootInAllDirections()
    {
        float angleStep = 360f / numberOfProjectiles;
        for (int i = 0; i < numberOfProjectiles; i++)
        {
            float angle = i * angleStep;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            projectile.GetComponent<Rigidbody2D>().linearVelocity = direction.normalized * projectileSpeed;
        }
    }

    public override void TakeDamage(float amount, Transform attacker)
    {
        if (!isVulnerable || isDead) return; // Només rep mal quan està vulnerable
        base.TakeDamage(amount, attacker);
    }
}
