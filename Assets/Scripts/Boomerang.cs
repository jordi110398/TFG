using UnityEngine;

public class Boomerang : MonoBehaviour
{
    public float rotationSpeed = 720f;
    public float speed = 10f;
    public float maxDistance = 8f;
    public float damage = 1f; // Dany que fa el boomerang
    private Vector2 launchDirection;
    private Vector2 startPosition;
    private bool isFlying = false;
    private Transform playerTransform;
    private bool isEquipped = false;
    private bool isReturning = false;

    // SO
    public AudioSource audioSource;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isEquipped && isFlying)
        {
            // Rotació visual
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

            if (!isReturning)
            {
                // Anada
                transform.position += (Vector3)(launchDirection * speed * Time.deltaTime);

                // Si ha arribat a la distància màxima, torna cap al jugador
                if (Vector2.Distance(startPosition, transform.position) >= maxDistance)
                {
                    isReturning = true;
                }
            }
            else
            {
                // Tornada: sempre cap al jugador, amb velocitat constant
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    playerTransform.position,
                    speed * Time.deltaTime
                );

                // Si arriba prou a prop del jugador, s'oculta
                if (Vector2.Distance(playerTransform.position, transform.position) < 0.5f)
                {
                    isFlying = false;
                    isReturning = false;
                    
                    // Atura el so de vol
                    if (audioSource != null && audioSource.isPlaying)
                        audioSource.Stop();
                    
                    gameObject.SetActive(false);

                    
                }
            }
        }
    }

    public void Launch(Vector2 direction, Transform player)
    {
        launchDirection = direction.normalized;
        startPosition = transform.position;
        playerTransform = player;
        isFlying = true;
        Debug.Log("Boomerang llançat en direcció: " + launchDirection);

        // So de vol en loop
        if (audioSource != null && AudioManager.Instance.player1Boomerang != null)
        {
            audioSource.clip = AudioManager.Instance.player1Boomerang;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void SetEquipped(bool equipped) { isEquipped = equipped; }
    public void Initialize(Player1Controller owner) { /* opcional */ }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isFlying) return; // Només fa mal mentre vola

        if (collision.CompareTag("Enemy"))
        {
            // Suposem que l'enemic té un script Enemy amb un mètode TakeDamage(int)
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, transform);
            }
        }
        else if (collision.CompareTag("Lever"))
        {
            Debug.Log("Boomerang ha activat un palanca");
            Lever lever = collision.GetComponent<Lever>();
            if (lever != null)
            {
                lever.Activate();
            }
        }
        else if (collision.CompareTag("Projectile"))
        {
            Projectile projectile = collision.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Detonate();
            }
        }
        else if (collision.CompareTag("BossShield"))
        {
            BossShield shield = collision.GetComponent<BossShield>();
            if (shield != null)
            {
                Debug.Log("Boomerang ha colpejat el escut del cap");
                isReturning = true;
                // Opcional: ajusta la posició d'inici de tornada
                startPosition = transform.position;
            }
        }
    }
}
