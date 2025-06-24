using System.Collections;
using System.Collections.Specialized;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player1Controller : MonoBehaviour
{
    [Header("Moviment")]
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
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;
    public int maxJumps = 2;
    int jumpsRemaining;
    public float jumpPower = 10f;
    [Header("Combat")]
    // COMBAT
    // ARCHERY (BOW) VARIABLES
    private Animator bowAnimator;
    private bool isCharging = false;
    //private float chargeStartTime = 0f;
    public float bowRotationOffset = 0f;
    private Vector2 aimInput = Vector2.zero;
    public GameObject arrowPrefab; // Prefab de la flecha
    private GameObject bow; // Prefab del arco
    public Transform arrowSpawnPoint; // Punt de sortida de la flecha
    public float arrowSpeed = 30f;
    private LineRenderer lineRenderer; // Linia d'apuntar
    private bool isAttacking = false;
    // BOOMERANG
    private GameObject boomerangPrefab;
    private Boomerang boomerang;
    public Transform weaponHand;
    //public float boomerangSpeed = 15f;
    private bool hasBoomerangActive = false;
    // INTERACCIÓ
    public float interactionRange = 2f;
    private Lever closestLever;
    private PlayerInput playerInput;
    // INVINCIBILITAT
    public bool isInvincible = false;

    [Header("Flash i knockback")]
    // Variables pel Flash
    private SpriteRenderer spriteRenderer;
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;
    private Color originalColor;
    // KNOCKBACK
    public float force;
    private bool isStunned = false;

    [Header("Player Manager")]
    // Referència al PlayerManager
    public GameObject playerManager;

    [Header("Item")]
    // ITEMS
    public Transform itemHolder;
    private GameObject heldItem;


    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        // Assigna dinàmicament el PlayerManager
        if (playerManager == null)
        {
            playerManager = GameObject.FindWithTag("PlayerManager");
            if (playerManager == null)
                Debug.LogWarning("No s'ha trobat cap PlayerManager a l'escena!");
        }
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
            // Compensa el flip a l'arc
            if (bow != null)
            {
                Vector3 bowScale = bow.transform.localScale;
                bowScale.x *= -1f;
                bow.transform.localScale = bowScale;
            }
        }
    }
    public void EquipBow(GameObject bowObject)
    {
        bow = bowObject;
        arrowSpawnPoint = bow.transform.Find("ArrowSpawnPoint");
        bow.SetActive(false);
        bowAnimator = bow.GetComponent<Animator>();
        lineRenderer = bow.GetComponent<LineRenderer>(); // Assigna el LineRenderer

        if (arrowSpawnPoint == null)
        {
            Debug.LogError("ArrowSpawnPoint no trobat");
        }
        else
        {
            Debug.Log("Bow equipat.");
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

    public Vector2 GetAimDirection()
    {
        Vector2 aimValue = playerInput.actions["Aim"].ReadValue<Vector2>();

        // Si és un joystick (valors normalitzats entre -1 i 1)
        if (aimValue.magnitude <= 1.1f && aimValue != Vector2.zero)
        {
            return aimValue.normalized;
        }

        // Si és una posició de pantalla (mouse position)
        Vector2 screenPlayerPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 direction = (aimValue - screenPlayerPos).normalized;

        return direction;
    }


    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (bow == null || arrowSpawnPoint == null) return;

        if (ctx.started)
        {
            bow.SetActive(true);
            isCharging = true;
            Animator bowAnimator = bow.GetComponent<Animator>();
            if (bowAnimator != null)
            {
                bowAnimator.SetBool("isAttacking", true);
            }
        }

        if (ctx.canceled && isCharging)
        {
            isCharging = false;
            ShootArrow();
            Animator bowAnimator = bow.GetComponent<Animator>();
            if (bowAnimator != null)
            {
                bowAnimator.SetBool("IsAttacking", false); // Torna a idle quan acabes l'atac
            }
        }
    }

    private void HandleBowRotation()
    {
        if (bow != null)
        {
            Vector2 direction = GetAimDirection();
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            angle += bowRotationOffset;
            bow.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    private void ShootArrow()
    {
        if (arrowSpawnPoint == null || bow == null) return;


        // Direcció cap al mouse
        Vector2 direction = GetAimDirection();

        if (direction.magnitude < 0.1f) return;

        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowPrefab.transform.rotation);
        Arrow arrowScript = arrow.GetComponent<Arrow>();
        int baseDamage = arrowScript.damage;
        // Si el jugador té un buff de dany, augmenta el dany de la fletxa
        if (TryGetComponent(out BattleCry battleCry))
        {
            arrowScript.damage = Mathf.RoundToInt(baseDamage * battleCry.GetDamageMultiplier());
            if (battleCry.GetDamageMultiplier() > 1f && arrowScript.auraPrefab != null)
            {
                // L'aura és filla de la fletxa i hereta la seva transformació
                // Busca el fill "ArrowTale" dins la fletxa
                Transform arrowTail = arrow.transform.Find("ArrowTail");
                GameObject aura = Instantiate(arrowScript.auraPrefab, arrowTail);
                aura.transform.localPosition = Vector3.zero; // Centra l'aura a la cua de la fletxa
                //aura.transform.localScale = Vector3.one;     // Manté l'escala original del prefab
            }
        }
        else
        {
            arrowScript.damage = baseDamage;
        }

        Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();

        arrowRb.AddForce(direction * arrowSpeed, ForceMode2D.Impulse);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        arrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Destroy(arrow, 5f);
        if (lineRenderer != null) lineRenderer.enabled = false;
        // Amaga l'arc després d'un temps curt
        StartCoroutine(HideBowAfterDelay(0.3f));
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
    // PICK-UP
    public void OnPickUp(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // Busca si hi ha un ItemPickup a prop
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRange);
        foreach (var hit in hits)
        {
            ItemPickup pickup = hit.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                pickup.TryPickUp(gameObject);
                break;
            }
        }
    }

    private IEnumerator HideBowAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (bow != null)
        {
            bow.SetActive(false);
        }
    }

    public void EquipBoomerang(GameObject boomerangObject)
    {
        boomerangPrefab = boomerangObject;
        boomerangPrefab.SetActive(false); // El boomerang està equipat però ocult fins que es llenci

        Boomerang boomerang = boomerangObject.GetComponent<Boomerang>();
        if (boomerang != null)
        {
            boomerang.SetEquipped(true);      // Activa la rotació només quan està equipat (si ho vols)
            boomerang.Initialize(this);       // Passa referència al propietari si cal
        }
        Debug.Log("Boomerang equipat.");
    }

    public void OnBoomerang(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        if (boomerangPrefab != null && !boomerangPrefab.activeInHierarchy)
        {
            boomerangPrefab.SetActive(true);
            boomerangPrefab.transform.SetParent(null); // <-- DESPARENTA!
            boomerangPrefab.transform.position = weaponHand.position + (Vector3)(GetAimDirection() * 0.8f);

            Boomerang boomerang = boomerangPrefab.GetComponent<Boomerang>();
            if (boomerang != null)
            {
                Vector2 direction = GetAimDirection();
                boomerang.Launch(direction, transform);
                Debug.Log("Boomerang volant");
            }
        }
    }

    public bool IsInvincible()
    {
        Debug.Log("Invencible: " + isInvincible);
        return isInvincible;
    }
    public void ApplyInvincibility(float duration)
    {
        StartCoroutine(InvincibilityCoroutine(duration));
    }

    private IEnumerator InvincibilityCoroutine(float duration)
    {
        Debug.Log("ARA ES INVENCIBLE!!");
        isInvincible = true;
        spriteRenderer.color = Color.cyan; // Canviar el color del sprite
        yield return new WaitForSeconds(duration);
        isInvincible = false;
        Debug.Log("INVENCIBILITAT FINALITZADA");
        spriteRenderer.color = originalColor; // Restaurar el color original
    }
    // Funció per aplicar Knockback
    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (!isInvincible)
        {
            Debug.Log("Aplicant Knockback vertical al jugador!");
            rb.linearVelocity = Vector2.zero;
            Vector2 verticalKnockback = Vector2.up * force;
            rb.AddForce(verticalKnockback, ForceMode2D.Impulse);

            StartCoroutine(DisableMovement(0.3f));
        }
    }

    private IEnumerator DisableMovement(float duration)
    {
        isStunned = true;
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }

    public void DisableInvincibility()
    {
        isInvincible = false;
        Debug.Log("INVENCIBILITAT DESACTIVADA");
    }
    public IEnumerator PlayDamageFlash()
    {
        Debug.Log("Flaix de mal activat!"); // Comprovació de si entra a la funció
        spriteRenderer.color = Color.red; // Color vermell
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    public IEnumerator PlayBlockingFlash()
    {
        Debug.Log("Flaix de bloquejar P1 activat!"); // Comprovació de si entra a la funció
        spriteRenderer.color = Color.cyan; // Color blau
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    // ITEMS
    public void OnUseItem(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            UseHeldItem();
    }
    public bool HasHeldItem()
    {
        return itemHolder != null;
    }
    public void EquipItem(GameObject item)
    {
        if (heldItem != null)
        {
            Debug.LogWarning("Ja tens un objecte equipat!");
            return;
        }

        heldItem = item;
        heldItem.transform.SetParent(itemHolder.transform);
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.SetActive(true);
        Debug.Log("Objecte equipat: " + item.name);
    }
    public void UseHeldItem()
    {
        if (heldItem == null)
        {
            Debug.LogWarning("No tens cap objecte equipat!");
            return;
        }

        if (heldItem.CompareTag("Potion"))
        {
            Debug.Log("Utilitzant poció: " + heldItem.name);
            // Aplica curació immediatament
            playerManager.GetComponent<HealthSystem>().Heal("Player1", 20);

            // Activa l'FX si existeix
            Transform fx = heldItem.transform.Find("HealingFX");
            if (fx != null)
            {
                fx.gameObject.SetActive(true);
                fx.SetParent(null); // Allibera l'FX per no destruir-lo amb la poció
                Destroy(fx.gameObject, 2f); // Elimina’l després d’un temps
            }

            // Destrueix la poció (visualment ja s'ha vist l'FX)
            Destroy(heldItem);
            heldItem = null;
        }
        else if (heldItem.CompareTag("Key"))
        {
            // Busca un cofre a prop
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRange);
            Chest chestToOpen = null;
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Chest"))
                {
                    chestToOpen = hit.GetComponent<Chest>();
                    break;
                }
            }

            if (chestToOpen != null)
            {
                Debug.Log("Obrint cofre amb la clau!");
                chestToOpen.Open(); // Assumeix que tens un mètode Open() al script Chest
                Destroy(heldItem);
                heldItem = null;
            }
            else
            {
                Debug.LogWarning("No hi ha cap cofre a prop per obrir!");
            }
        }
        else
        {
            Debug.LogWarning("L'objecte equipat no es pot utilitzar així!");
        }
    }

}