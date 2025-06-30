using UnityEngine;

public class Pushable : MonoBehaviour
{
    private Rigidbody2D rb;
    private GameObject playerOnTop = null;

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
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    playerOnTop = collision.gameObject;
                    break;
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player1"))
        {
            if (playerOnTop == collision.gameObject)
                playerOnTop = null;
        }
    }

    void FixedUpdate()
    {
        if (playerOnTop != null)
        {
            // Mou el Player1 la mateixa distància que la caixa s'ha mogut aquest frame
            Rigidbody2D playerRb = playerOnTop.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 boxVelocity = rb.linearVelocity * Time.fixedDeltaTime;
                playerRb.position += boxVelocity;
            }
        }
    }
}
