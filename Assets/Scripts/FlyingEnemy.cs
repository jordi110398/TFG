using System.Runtime.CompilerServices;
using UnityEngine;



public class FlyingEnemy : EnemyController
{
    public float speed;
    private GameObject player1;
    private GameObject player2;
    public bool chase = false;
    public Transform startingPoint;
    public Transform chaseZone;
    public float chaseRadius = 5f;
    public bool player1InZone = false;
    public bool player2InZone = false;
    public GameObject target = null;


    // Update is called once per frame
    void Update()
    {
        if (isDead) return;
        
        if (isBlocked || isKnockbacked)
        {
            rb.linearVelocity = Vector2.zero;
            //anim.SetBool("isRunning", false);
            return;
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
            transform.position = Vector2.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
        }
    }

    private void ReturnStartingPoint()
    {
        // Moure l'enemic al punt d'inici
        transform.position = Vector2.MoveTowards(transform.position, startingPoint.position, speed * Time.deltaTime);
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
    protected override void Die()
    {
        isDead = true;
        if (anim != null)
            anim.SetTrigger("isDead");
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Dynamic; // Permet caure per la gravetat
        rb.gravityScale = 1f; // Gravetat activada
        // Desactiva només el collider de trigger
        foreach (var col in GetComponents<Collider2D>())
        {
            if (col.isTrigger)
                col.enabled = false;
        }
        StartCoroutine(HandleDeath());
    }

    private void OnDrawGizmos()
    {
        // Chase Zone
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(chaseZone.position, chaseRadius);
    }
}
