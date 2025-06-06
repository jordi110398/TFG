using UnityEngine;

public class Boomerang : MonoBehaviour
{
    public float speed = 15f;
    public float returnDelay = 3f;

    private Rigidbody2D rb;
    private Transform player;
    private float rotationSpeed = 360f;
    private bool returning = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction, Transform playerTransform)
    {
        player = playerTransform;
        Vector2 velocity = direction.normalized * speed;
        Debug.Log("Llançant boomerang amb velocitat: " + velocity);
        rb.linearVelocity = velocity;
        Invoke(nameof(StartReturn), returnDelay);
    }

    void StartReturn()
    {
        returning = true;
    }

    void Update()
    {
        // Rotació visual (estil boomerang)
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        if (returning && player != null)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            rb.linearVelocity = directionToPlayer * speed;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (returning && other.CompareTag("Player1"))
        {
            Destroy(gameObject);
        }
    }
}
