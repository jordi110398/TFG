using System.Runtime.CompilerServices;
using UnityEngine;
using System.Collections;



public class FlyingEnemy : EnemyController
{
    public float speed = 8f;
    private GameObject player1;
    private GameObject player2;
    public bool chase = false;
    public Transform startingPoint;
    public Transform chaseZone;
    public float chaseRadius = 5f;
    public bool player1InZone = false;
    public bool player2InZone = false;
    public GameObject target = null;
    private Coroutine flySoundCoroutine;


    protected override void Start()
    {
        base.Start();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (startingPoint == null)
        {
            startingPoint = transform; // Si no es defineix, utilitza la posició actual
        }
        rb.bodyType = RigidbodyType2D.Kinematic; // Perquè pugui ser controlat manualment
        rb.gravityScale = 0f; // Perquè no caigui
                              // SO
                              //flySoundCoroutine = StartCoroutine(PlayFlySound());
        //audioSource.PlayOneShot(AudioManager.Instance.enemyFly);
    }
    // Update is called once per frame
    void Update()
    {
        if (isDead) return;

        if (isBlocked || isKnockbacked)
        {
            rb.linearVelocity = Vector2.zero;

        }

        player1 = GameObject.FindGameObjectWithTag("Player1");
        player2 = GameObject.FindGameObjectWithTag("Player2");
        if (player1 == null && player2 == null)
        {
            Debug.LogWarning("Els jugadors no existeixen.");
            target = null;
            return;
        }

        player1InZone = player1 != null && Vector2.Distance(player1.transform.position, chaseZone.position) < chaseRadius;
        player2InZone = player2 != null && Vector2.Distance(player2.transform.position, chaseZone.position) < chaseRadius;

        chase = player1InZone || player2InZone;

        if (chase)
        {
            Chase();
        }
        else
        {
            target = null; // Neteja el target si no persegueix ningú
            ReturnStartingPoint();
        }

        if (target != null)
            Flip();
        // Inicia la coroutine si no està en marxa
        if (flySoundCoroutine == null)
        {
            flySoundCoroutine = StartCoroutine(PlayFlySound());
        }
    }
    private IEnumerator PlayFlySound()
    {
        while (true)
        {
            if (audioSource == null) Debug.LogWarning("AudioSource del FlyingEnemy és null!");
            audioSource.PlayOneShot(AudioManager.Instance.enemyFly);
            yield return new WaitForSeconds(0.25f); // Ajusta l'interval al teu gust
        }
    }

    private void Chase()
    {
        Debug.Log("Perseguint els jugadors...");
        float minDist = float.MaxValue;
        target = null; // Inicialitza target a null

        if (player1InZone)
        {
            float dist = Vector2.Distance(transform.position, player1.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                target = player1;
            }
        }
        if (player2InZone)
        {
            float dist = Vector2.Distance(transform.position, player2.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                target = player2;
            }
        }

        if (target != null)
        {
            Vector2 direction = (target.transform.position - transform.position).normalized;
            float distance = speed * Time.deltaTime;
            LayerMask wallMask = LayerMask.GetMask("Ground"); // Assegura't que les parets tenen la Layer "Wall"

            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, wallMask);
            if (hit.collider == null)
            {
                rb.MovePosition(rb.position + direction * distance);
            }
            // Si hi ha paret, no es mou o pots fer que busqui una altra ruta
        }
    }

    private void ReturnStartingPoint()
    {
        // Moure l'enemic al punt d'inici
        rb.MovePosition(Vector2.MoveTowards(transform.position, startingPoint.position, speed * Time.deltaTime));
    }

    private void Flip()
    {
        if (transform.position.x > target.transform.position.x)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0); // Orientació normal
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 180, 0); // Orientació invertida
        }
    }
    public override void TakeDamage(float amount, Transform attacker)
    {
        base.TakeDamage(amount, attacker);

        if (audioSource != null && AudioManager.Instance.enemyFlyHurt != null)
            audioSource.PlayOneShot(AudioManager.Instance.enemyFlyHurt);
    }
    protected override void Die()
    {
        base.Die(); // <-- Això fa el drop i la resta de lògica de mort

        // Si vols, afegeix aquí comportament extra pel ratpenat (caure, gravetat, etc.)
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
    }

    private void OnDrawGizmos()
    {
        // Chase Zone
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(chaseZone.position, chaseRadius);
    }
}
