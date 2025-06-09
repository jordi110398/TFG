using UnityEngine;

public class BattleCry : MonoBehaviour
{
    private bool isBuffed = false;
    private float buffTimer = 0f;
    public float damageMultiplier = 1.25f; // Ex: +25% damage
    public GameObject shieldVisualPrefab; // assigna des de l'inspector
    private GameObject activeShield;

    public void ApplyBuff(float duration)
    {
        Debug.Log("Buff aplicat");
        isBuffed = true;
        buffTimer = duration;
        if (TryGetComponent(out Player1Controller player))
        {
            player.ApplyInvincibility(duration);
        }

        if (shieldVisualPrefab != null && activeShield == null)
        {
            activeShield = Instantiate(shieldVisualPrefab, transform);
        }
    }

    void Update()
    {
        if (isBuffed)
        {
            buffTimer -= Time.deltaTime;
            if (buffTimer <= 0)
            {
                isBuffed = false;

                if (activeShield != null)
                    Destroy(activeShield);
            }
        }
    }

    public float GetDamageMultiplier()
    {
        return isBuffed ? damageMultiplier : 1f;
    }
}
