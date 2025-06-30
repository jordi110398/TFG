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
                Debug.Log("Contact normal: " + contact.normal);
                if (contact.normal.y < -0.5f)
                {
                    playerOnTop = collision.gameObject;
                    Debug.Log("Player1 assignat com a playerOnTop!");
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
            {
                Player1Controller controller = playerOnTop.GetComponent<Player1Controller>();
                if (controller != null)
                    controller.platformVelocity = Vector2.zero;
                playerOnTop = null;
            }
        }
    }

    void FixedUpdate()
    {
        if (playerOnTop != null)
        {
            Player1Controller controller = playerOnTop.GetComponent<Player1Controller>();
            if (controller != null)
            {
                controller.platformVelocity = rb.linearVelocity;
                Debug.Log("Platform velocity: " + rb.linearVelocity);
            }
        }
    }
}
