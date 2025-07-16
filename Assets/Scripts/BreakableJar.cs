using UnityEngine;

public class BreakableJar : MonoBehaviour
{
    public GameObject breakParticlesPrefab;
    public AudioClip breakSound;

    public GameObject[] dropPrefabs; // Assigna els objectes a soltar a l'Inspector
    public Transform dropPoint; // Opcional: punt concret per fer el drop

    private bool isBroken = false;
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Break()
    {
        if (isBroken) return;
        isBroken = true;
        animator.SetTrigger("isBroken");

        // Desactiva tots els Collider2D del gerro
        foreach (var col in GetComponents<Collider2D>())
            col.enabled = false;

        if (breakParticlesPrefab != null)
            Instantiate(breakParticlesPrefab, transform.position, Quaternion.identity);

        if (breakSound != null)
            AudioSource.PlayClipAtPoint(breakSound, transform.position);

        // Drop d'objectes
        if (dropPrefabs != null)
        {
            foreach (var prefab in dropPrefabs)
            {
                if (prefab != null)
                {
                    Vector3 pos = dropPoint != null ? dropPoint.position : transform.position;
                    Instantiate(prefab, pos, Quaternion.identity);
                }
            }
        }

        //Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Atac d'espasa, fletxa, boomerang, etc.
        if (other.CompareTag("PlayerAttack") || other.CompareTag("Arrow") || other.CompareTag("Boomerang"))
        {
            Break();
        }
    }

    // Si vols que també es pugui trencar amb col·lisions físiques (empenta forta):
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Boomerang"))
        {
            Break();
        }
    }
}
