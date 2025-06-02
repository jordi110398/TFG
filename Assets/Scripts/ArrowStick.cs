using UnityEngine;
using System.Collections;

public class ArrowStick : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool stuck = false;
    public float timeBeforeDestroy = 3f; // segons abans de desaparèixer

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (stuck) return;

        // Evitar enganxar-se al jugador
        if (collision.gameObject.CompareTag("Player1"))
            return;

        stuck = true;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        transform.SetParent(collision.transform);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        ArrowFollowDirection dirScript = GetComponent<ArrowFollowDirection>();
        if (dirScript != null) dirScript.enabled = false;

        StartCoroutine(DestroyAfterSeconds());
    }

    private IEnumerator DestroyAfterSeconds()
    {
        yield return new WaitForSeconds(timeBeforeDestroy);
        Destroy(gameObject);
    }
}

