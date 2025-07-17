using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
    public GameObject arrowPrefab;
    public Transform[] shootPoints;
    public float shootInterval = 2f;
    public float arrowSpeed = 25f;
    private float timer = 0f;
    public bool isActive = true;

    void Update()
    {
        if (!isActive) return;

        timer += Time.deltaTime;
        if (timer >= shootInterval)
        {
            ShootArrow();
            timer = 0f;
        }
    }

    void ShootArrow()
    {
        foreach (var shootPoint in shootPoints)
        {
            // Ajusta la rotació: gira -45 graus respecte el shootPoint
            Quaternion rotation = shootPoint.rotation * Quaternion.Euler(0, 0, -45f);
            GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, rotation);
            Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = shootPoint.right * arrowSpeed;
        }
    }
    void OnDrawGizmos()
    {
        if (shootPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (var shootPoint in shootPoints)
            {
                if (shootPoint != null)
                {
                    Gizmos.DrawLine(shootPoint.position, shootPoint.position + shootPoint.right * 2f);
                    Gizmos.DrawWireSphere(shootPoint.position, 0.1f);
                }
            }
        }
    }

    public void ActivateTrap()
    {
        isActive = true;
    }

    public void DeactivateTrap()
    {
        isActive = false;
    }

    public void ToggleTrap()
    {
        isActive = !isActive;
    }
}

