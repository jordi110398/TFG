using UnityEngine;

public class Boomerang : MonoBehaviour
{
    public float speed = 15f;
    public float returnDelay = 3f;
    private Rigidbody2D rb;
    private Transform player;
    private float rotationSpeed = 360f;
    private bool returning = false;
    private Vector3 direction;
    public System.Action onBoomerangReturn;
    public int damage = 1; 


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player1").transform;
    }

    public void Launch(Vector3 dir)
    {
        direction = dir.normalized;

        rb.linearVelocity = direction * speed;
        transform.right = direction;

        Invoke(nameof(StartReturn), returnDelay);
    }

    private void StartReturn()
    {
        returning = true;
    }

    void Update()
    {
        if (returning && player != null)
        {
            Vector2 directionToPlayer = ((Vector2)player.position - rb.position).normalized;
            rb.linearVelocity = directionToPlayer * speed;

            // Gira el bumerang cap a la direcció de retorn
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, angle), rotationSpeed * Time.deltaTime);

            // Si arriba molt a prop del jugador, desapareix
            if (Vector2.Distance(transform.position, player.position) < 0.5f)
            {
                onBoomerangReturn?.Invoke(); // Notifica al jugador
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy")) // Assegura't que el Slime té el tag "Enemy"
        {
            // Intentem trobar el script que gestiona la vida del Slime
            SlimeController enemy = collision.GetComponent<SlimeController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, transform);
            }
        }
    }
}
