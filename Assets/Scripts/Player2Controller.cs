using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player2Controller : MonoBehaviour
{
    // BASIC CHARACTER MOVEMENT VARIABLES
    bool isRunning = false;
    public float speed = 5;
    private bool isFacingRight = true;

    // DASH VARIABLES
    private bool canDash;
    private bool isDashing;
    private float dashingPower = 8f;
    private float dashingTime = 0.4f;
    private float dashingCooldown = 3f;
    public float dashPushForce = 30f;
    public TrailRenderer tr;
    public Animator animator;
    public Rigidbody2D rb;
    float horizontalMovement;
    private Vector2 movementInput;

    // PUSH
    public LayerMask boxLayer;


    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;
    private bool isGrounded;
    private bool isOverBox;
    public int maxJumps = 2;
    int jumpsRemaining;

    public float jumpPower = 5f;

    // SWORD VARIABLES
    private Animator swordAnimator;
    private Vector2 aimInput = Vector2.zero;
    public GameObject sword; // Prefab de l'espasa
    public ParticleSystem attackParticles;
    private TrailRenderer swordTrail;
    public TrailRenderer equippedSwordTrail;
    public Transform swordHolder;

    // SHIELD VARIABLES
    public GameObject shield;         // Prefab de l'escut
    public Transform shieldHolder;    // Referència al lloc on es col·loca l'escut
    private bool isBlocking = false;

    // KNOCKBACK
    // Variables pel Flash
    private SpriteRenderer spriteRenderer;
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;
    private Color originalColor;
    public float force;

    // COMBAT
    private bool isAttacking = false;
    public Transform attackOrigin;
    public float attackRadius = 1f;
    public LayerMask enemyMask;
    public float swordDamage = 1f;

    // INTERACCIÓ
    public float interactionRange = 2f;
    private Lever closestLever;



    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    private void Start()
    {

        if (sword != null)
        {


        }
        else
        {
            Debug.LogError("No s'ha assignat el prefab de l'espasa!!!!");
        }
    }

    private void Update()
    {
        if (isDashing)
        {
            return;
        }

        GroundCheck();
        Flip();

        // Si estàs bloquejant, el jugador no es pot moure
        if (!isBlocking)
        {
            rb.linearVelocity = new Vector2(horizontalMovement * speed, rb.linearVelocityY);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocityY); // Bloqueja el moviment horitzontal
        }
    }


    private void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }

        if (!isBlocking)
        {
            rb.linearVelocity = new Vector2(horizontalMovement * speed, rb.linearVelocityY);
        }
    }


    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (!isBlocking)
        {
            horizontalMovement = ctx.ReadValue<Vector2>().x;
            animator.SetFloat("Speed", Mathf.Abs(horizontalMovement));
            //animator.SetBool("Running", true);
            isRunning = true;

        }

    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        // Bloqueja el salt si està bloquejant
        if (isBlocking) return;

        if (jumpsRemaining > 0)
        {
            if (ctx.performed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpPower);
                jumpsRemaining--;
                animator.SetBool("Jumping", true);
                Debug.Log("player2 esta saltant!!!!");
            }
            else if (ctx.canceled)
            {
                // Salt suau
                rb.linearVelocity = new Vector2(rb.linearVelocityX, rb.linearVelocityY * 0.5f);
                jumpsRemaining--;
                animator.SetBool("Jumping", true);
            }

        }

    }
    public void OnDash(InputAction.CallbackContext ctx)
    {
        if (!isDashing && ctx.performed && horizontalMovement != 0 && isGrounded)
        {
            StartCoroutine(Dash());
        }

    }


    private void GroundCheck()
    {
        isGrounded = Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer);
        isOverBox = Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, boxLayer);

        if (isGrounded || isOverBox)
        {
            jumpsRemaining = maxJumps;
            animator.SetBool("Jumping", false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        Gizmos.DrawWireSphere(attackOrigin.position, attackRadius);

    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        animator.SetBool("Dashing", true);
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        tr.emitting = true;
        // Empènyer caixes durant el dash
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.5f);

        foreach (var col in colliders)
        {
            if (col.attachedRigidbody != null && col.CompareTag("Pushable"))
            {
                Vector2 pushDir = new Vector2(transform.localScale.x, 0).normalized;
                Pushable box = col.GetComponent<Pushable>();
                if (box != null)
                {
                    box.Push(pushDir, dashPushForce); // Pot ajustar la força
                }

            }
        }


        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        animator.SetBool("Dashing", false);
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    // Interacció
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Debug.Log("El Player2 està interactuant...");
            // Busca totes les palanques dins el rang
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRange);
            float minDist = float.MaxValue;

            closestLever = null;

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Lever"))
                {
                    float dist = Vector2.Distance(transform.position, hit.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestLever = hit.GetComponent<Lever>();
                    }
                }
            }

            if (closestLever != null)
            {
                closestLever.Activate();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDashing && collision.gameObject.CompareTag("Pushable"))
        {
            Rigidbody2D boxRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (boxRb != null)
            {
                // Activa la física només per un moment
                boxRb.bodyType = RigidbodyType2D.Dynamic;

                // Dona un impuls (ex: en direcció del dash)
                Vector2 dashDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
                boxRb.AddForce(dashDirection * dashPushForce, ForceMode2D.Impulse);

                // Desactiva la física després de 0.3 segons
                StartCoroutine(ResetBoxPhysics(boxRb));
            }
        }
    }

    private IEnumerator ResetBoxPhysics(Rigidbody2D boxRb)
    {
        yield return new WaitForSeconds(0.3f);
        if (boxRb != null)
        {
            boxRb.linearVelocity = Vector2.zero;
            boxRb.angularVelocity = 0f;
            boxRb.bodyType = RigidbodyType2D.Kinematic;
        }
    }


    private void Flip()
    {
        // Només es fa flip si no està bloquejant
        if (isBlocking)
        {
            if (isFacingRight && aimInput.x < 0f || !isFacingRight && aimInput.x > 0f)
            {
                Vector3 localScale = transform.localScale;
                isFacingRight = !isFacingRight;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }
        }
        else
        {
            if (isFacingRight && horizontalMovement < 0f || !isFacingRight && horizontalMovement > 0f)
            {
                Vector3 localScale = transform.localScale;
                isFacingRight = !isFacingRight;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }
        }
    }
    public void EquipSword(GameObject swordObject)
    {
        sword = swordObject;
        //arrowSpawnPoint = sword.transform.Find("ArrowSpawnPoint");
        swordAnimator = sword.GetComponent<Animator>();
        swordTrail = sword.GetComponent<TrailRenderer>(); // Aquí assignem el trail

        if (sword == null)
        {
            Debug.LogError("sword no encontrada.");
        }
        else
        {
            Debug.Log("sword equipado correctamente, spawn point actualizado.");
            sword.SetActive(false); // Oculta o mostra l'espasa
        }
    }

    // Getter per saber si el jugador està atacant
    public bool IsAttacking()
    {
        return isAttacking;
    }

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(attackOrigin.position, attackRadius, enemyMask);
        foreach (var enemy in enemiesInRange)
        {
            enemy.GetComponent<SlimeController>().TakeDamage(swordDamage, transform);
        }

        // Comprovar que no està en Idle (Speed < 0.1) ni bloquejant ni saltant
        if (animator.GetFloat("Speed") < 0.01f || isBlocking || !isGrounded) return;

        float attackInput = ctx.ReadValue<float>();

        // Comprovar direcció d'atac
        if (Mathf.Abs(attackInput) > 0.5f)
        {
            animator.SetTrigger("Attack");
            //swordAnimator.SetTrigger("isAttacking");
            sword.SetActive(true); // Mostrar l'espasa
            isAttacking = true;
            equippedSwordTrail.enabled = true;
            PlayAttackEffect(); // Partícules d'atac

            // Desactivar l'espasa després de l'atac
            StartCoroutine(HideSwordAfterAttack());
        }
    }

    private IEnumerator HideSwordAfterAttack()
    {
        // Esperar el temps de l'animació (ajusta aquest valor segons duri l'animació d'atac)
        yield return new WaitForSeconds(0.7f);

        // Reiniciar l'estat d'atac i el trigger
        isAttacking = false;
        sword.SetActive(false);
        animator.ResetTrigger("Attack");
        //swordAnimator.ResetTrigger("isAttacking");

        // Aquí actualitzem l'animador amb els valors correctes
        if (Mathf.Abs(horizontalMovement) > 0.01f)
        {
            animator.SetFloat("Speed", Mathf.Abs(horizontalMovement));
        }
        else
        {
            animator.SetFloat("Speed", 0);
        }
    }

    public void PlayAttackEffect()
    {
        if (attackParticles != null) attackParticles.Play();
        if (swordTrail != null)
        {
            
            swordTrail.emitting = true;
            Invoke(nameof(StopTrail), 0.2f);
        }
    }

    void StopTrail()
    {
        if (swordTrail != null) swordTrail.emitting = false;
        equippedSwordTrail.enabled = false;
    }

    public void EquipShield(GameObject shieldObject)
    {
        shield = shieldObject;
        //shieldParticles = shield.GetComponent<TrailRenderer>(); // Aquí assignem el trail

        if (shield == null)
        {
            Debug.LogError("Escut no trobat.");
        }
        else
        {
            Debug.Log("Escut equipat correctament.");
            shield.SetActive(false); // Oculta o mostra l'escut
        }
    }

    public void OnAim(InputAction.CallbackContext ctx)
    {
        aimInput = ctx.ReadValue<Vector2>();
        // Comprovació de la direcció d'apuntat
        if (Mathf.Abs(aimInput.x) > 0.5f) // Si està apuntant cap a l'esquerra o la dreta
        {
            isBlocking = true;
            shield.SetActive(true);
            animator.SetBool("isBlocking", true);

            // Rotació de l'escut depenent de la direcció
            if (aimInput.x > 0)
            {
                shieldHolder.localScale = new Vector3(1, 1, 1); // Escut a la dreta
            }
            else
            {
                shieldHolder.localScale = new Vector3(-1, 1, 1); // Escut a l'esquerra
            }

            if ((aimInput.x > 0 && !isFacingRight) || (aimInput.x < 0 && isFacingRight))
            {
                Flip();
            }
        }
        else
        {
            isBlocking = false;
            shield.SetActive(false);
            animator.SetBool("isBlocking", false);
        }
    }
    // BLOQUEJAR AMB EL RATOLÍ
    public void OnBlock(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            isBlocking = true;
            shield.SetActive(true);
            animator.SetBool("isBlocking", true);

            Vector2 aimInput = ctx.ReadValue<Vector2>();

            // Rotació de l'escut depenent de la direcció
            if (aimInput.x > 0)
            {
                shieldHolder.localScale = new Vector3(1, 1, 1);
            }
            else if (aimInput.x < 0)
            {
                shieldHolder.localScale = new Vector3(-1, 1, 1);
            }

            // Flip si està mirant a l'altre costat
            if ((aimInput.x > 0 && !isFacingRight) || (aimInput.x < 0 && isFacingRight))
            {
                Flip();
            }
        }
        else if (ctx.canceled)
        {
            isBlocking = false;
            shield.SetActive(false);
            animator.SetBool("isBlocking", false);
        }
    }

    // Funció per saber si està bloquejant (necessària per al Knockback)
    public bool IsBlocking()
    {
        return isBlocking;
    }

    // Funció per aplicar Knockback
    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (!isBlocking) // No aplicar si està bloquejant
        {
            Debug.Log("Aplicant Knockback vertical al jugador!");
            rb.linearVelocity = Vector2.zero; // Reiniciar la velocitat
            Vector2 verticalKnockback = Vector2.up * force; // Knockback només cap amunt
            rb.AddForce(verticalKnockback, ForceMode2D.Impulse);

            // Bloquejar moviments durant el knockback
            StartCoroutine(DisableMovement(0.3f));
        }
    }

    private IEnumerator DisableMovement(float duration)
    {
        float originalSpeed = speed;
        speed = 0;
        yield return new WaitForSeconds(duration);
        speed = originalSpeed;
    }
    public IEnumerator PlayDamageFlash()
    {
        Debug.Log("Flaix de mal activat!"); // Comprovació de si entra a la funció
        spriteRenderer.color = Color.red; // Prova amb un color més visible
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    public IEnumerator PlayBlockingFlash()
    {
        Debug.Log("Flaix de bloquejar activat!"); // Comprovació de si entra a la funció
        spriteRenderer.color = Color.cyan; // Prova amb un color més visible
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }
}
