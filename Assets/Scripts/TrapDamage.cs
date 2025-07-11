using UnityEngine;

public class TrapDamage : MonoBehaviour
{
    public float damage = 1f;
    public float damageCooldown = 0.5f; // Segons entre danys

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            // Busca el HealthSystem i aplica dany
            HealthSystem health = FindAnyObjectByType<HealthSystem>();
            if (health != null)
            {
                if (other.TryGetComponent(out Player1Controller p1))
                {
                    if (!p1.isInvincible)
                        health.TakeDamage("Player1", damage);
                }
                else if (other.TryGetComponent(out Player2Controller p2))
                {
                    if (!p2.isInvincible)
                        health.TakeDamage("Player2", damage);
                }
            }
        }
    }
}
