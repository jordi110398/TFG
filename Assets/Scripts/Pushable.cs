using UnityEngine;

public class Pushable : MonoBehaviour
{
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Push(Vector2 direction, float force)
    {
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
        StartCoroutine(StopBox());
    }

    private System.Collections.IEnumerator StopBox()
    {
        yield return new WaitForSeconds(0.2f); // Temps per deixar-la moure una mica
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
    void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.gameObject.CompareTag("Player1"))
    {
        // Cancel·la la velocitat si el toc és de Player1
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
}

}
