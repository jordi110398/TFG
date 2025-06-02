using UnityEngine;

public class Arrow : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int damage = 10; // Dany que fa la fletxa
    public float destroyDelay = 0.1f; // Temps que triga en destruir-se després del xoc
    public Transform damageZone;

    bool hasHit = false;

    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (hasHit == false)
        {
            trackMovement();
        }
    }

    void trackMovement()
    {
        Vector2 direction = rb.linearVelocity;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Comprovem si l'objecte que toquem és un enemic
        if (collision.CompareTag("Enemy"))
        {
            // Agafem el component "EnemyHealth" de l'enemic
            SlimeController enemy = collision.GetComponent<SlimeController>();

            if (enemy != null)
            {
                // Li restem vida a l'enemic
                enemy.TakeDamage(damage, damageZone);
                Debug.Log("Li ha tocat la fletxa!!");
            }

            // Destrueix la fletxa després d'un petit retard per evitar errors visuals
            //Destroy(gameObject, destroyDelay);
        }
        else if (!collision.CompareTag("Player"))
        {
            // Si toca qualsevol altra cosa que no sigui el jugador, també es destrueix
            //Destroy(gameObject, destroyDelay);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        hasHit = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

    }
}
