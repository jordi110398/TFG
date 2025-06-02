using System.Collections;
using System.Collections.Specialized;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player1Controller : MonoBehaviour
{
    // BASIC CHARACTER MOVEMENT VARIABLES
    bool isRunning = false;
    public float speed = 5;
    private bool isFacingRight = true;

    // DASH VARIABLES
    private bool canDash;
    private bool isDashing;
    private float dashingPower = 19f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 3f;
    public TrailRenderer tr;
    public Animator animator;
    public Rigidbody2D rb;
    float horizontalMovement;
    private Vector2 movementInput;

    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;
    public int maxJumps = 2;
    int jumpsRemaining;

    public float jumpPower = 10f;

    // COMBAT
    // ARCHERY (BOW) VARIABLES
    private Animator bowAnimator;
    private bool isCharging = false;
    //private float chargeStartTime = 0f;
    private Vector2 aimInput = Vector2.zero;
    public GameObject arrowPrefab; // Prefab de la flecha
    public GameObject bow; // Prefab del arco
    public Transform arrowSpawnPoint; // Punt de sortida de la flecha
    public float arrowSpeed = 30f;
    private LineRenderer lineRenderer; // Linia d'apuntar
    private bool isAttacking = false;

    // INTERACCIÓ
    public float interactionRange = 2f;
    private Lever closestLever;


    private void Start()
    {

        if (bow != null)
        {
            arrowSpawnPoint = bow.transform.Find("ArrowSpawnPoint");
            if (arrowSpawnPoint == null)
            {
                Debug.LogError("ArrowSpawnPoint no trobat dins del prefab del arco.");
            }
        }
        else
        {
            Debug.LogError("No s'ha assignat el prefab del arco!!!!");
        }
        // Després d'equipar l'arc
        //lineRenderer = bow.GetComponent<LineRenderer>();
    }



    private void Update()
    {
        if (isDashing)
        {
            return;
        }
        rb.linearVelocity = new Vector2(horizontalMovement * speed, rb.linearVelocityY);
        GroundCheck();
        Flip();
        HandleBowRotation();

    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }

        rb.linearVelocity = new Vector2(horizontalMovement * speed, rb.linearVelocityY);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        horizontalMovement = ctx.ReadValue<Vector2>().x;
        animator.SetFloat("Speed", Mathf.Abs(horizontalMovement));
        animator.SetBool("Running", true);
        isRunning = true;
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (jumpsRemaining > 0)
        {
            if (ctx.performed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpPower);
                jumpsRemaining--;
                animator.SetBool("Jumping", true);
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
        if (!isDashing && ctx.performed && horizontalMovement != 0)
        {
            StartCoroutine(Dash());
        }

    }


    private void GroundCheck()
    {
        if (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
        {
            jumpsRemaining = maxJumps;
            animator.SetBool("Jumping", false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        // Visualitzar el rang d’interacció
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

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

        yield return new WaitForSeconds(dashingTime);

        tr.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        animator.SetBool("Dashing", false);

        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    private void Flip()
    {
        if (isFacingRight && horizontalMovement < 0f || !isFacingRight && horizontalMovement > 0f)
        {
            Vector3 localScale = transform.localScale;
            isFacingRight = !isFacingRight;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
    public void EquipBow(GameObject bowObject)
    {
        bow = bowObject;
        arrowSpawnPoint = bow.transform.Find("ArrowSpawnPoint");
        bowAnimator = bow.GetComponent<Animator>();
        lineRenderer = bow.GetComponent<LineRenderer>(); // Assigna el LineRenderer

        if (arrowSpawnPoint == null)
        {
            Debug.LogError("ArrowSpawnPoint no encontrado en el arco recogido.");
        }
        else
        {
            Debug.Log("Bow equipado correctamente, spawn point actualizado.");
        }
    }

    // Getter per saber si el jugador està atacant
    public bool IsAttacking()
    {
        return isAttacking;
    }
    // === APUNTAR I DISPARAR ===
    public void OnAim(InputAction.CallbackContext ctx)
    {
        aimInput = ctx.ReadValue<Vector2>();
    }

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (bow == null || arrowSpawnPoint == null) return;

        if (ctx.started)
        {
            isCharging = true;
            Animator bowAnimator = bow.GetComponent<Animator>();
            if (bowAnimator != null)
            {
                bowAnimator.SetTrigger("Attack");
            }
        }

        if (ctx.canceled && isCharging)
        {
            isCharging = false;
            ShootArrow();
        }
    }

    private void HandleBowRotation()
    {
        if (bow != null)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector2 worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

            Vector2 direction = (worldMousePosition - (Vector2)arrowSpawnPoint.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            bow.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    private void ShootArrow()
    {
        if (arrowSpawnPoint == null || bow == null) return;

        // Direcció cap al mouse
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector2 direction = (worldPosition - (Vector2)arrowSpawnPoint.position).normalized;

        if (direction.magnitude < 0.1f) return;

        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();

        arrowRb.AddForce(direction * arrowSpeed, ForceMode2D.Impulse);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        arrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Destroy(arrow, 5f);
        if (lineRenderer != null) lineRenderer.enabled = false;
    }

    // Interacció
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Debug.Log("El Player1 està interactuant...");
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


}
