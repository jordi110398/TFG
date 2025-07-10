using UnityEngine;

public class BossShield : MonoBehaviour
{
    public BossController boss;
    public float shieldDamage = 10f; // Dany que fa l'escut als jugadors
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Només actiu si l'escut està actiu i el boss NO és vulnerable
        if (!boss.shieldParticles.activeSelf || boss.isVulnerable) return;

        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            // Fes mal a través del HealthSystem
            var healthSystem = FindAnyObjectByType<HealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.TakeDamage(other.tag, shieldDamage);
            }

            // Knockback opcional
            var rb = other.attachedRigidbody;
            if (rb != null)
            {
                Vector2 knockDir = (other.transform.position - boss.transform.position).normalized;
                rb.AddForce(knockDir * 1f, ForceMode2D.Impulse);
            }
        }
    }
    public void TryBreakShield(GameObject attacker)
    {
        // Només trenca si l'escut està actiu
        if (!boss.shieldParticles.activeSelf || boss.isVulnerable) return;

        // Només el Player1 amb BattleCry pot trencar l'escut
        var p1 = attacker.GetComponent<Player1Controller>();
        if (p1 != null && p1.TryGetComponent(out BattleCry battleCry) && battleCry.IsBuffActive())
        {
            Debug.Log("Player1 ha trencat l'escut del Boss amb BattleCry!");
            boss.shieldParticles.SetActive(false);
            boss.isVulnerable = true;
            // Efectes visuals/so aquí
        }
    }
}
