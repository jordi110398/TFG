using System.Collections;
using System.Collections.Generic;
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
    public GameObject slashEffectPrefab; // Efecte visual de tall
    public GameObject stabEffectPrefab; // Efecte visual d'estocada
    public Transform hitPoint; // Punt on es fa el tall
    public Transform stabPoint; // Punt on es fa l'estocada

    // SHIELD VARIABLES
    public GameObject shield;         // Prefab de l'escut
    public Transform shieldHolder;    // Referència al lloc on es col·loca l'escut
    private bool isBlocking = false;
    private bool isRightMouseHeld = false;

    // KNOCKBACK
    // Variables pel Flash
    private SpriteRenderer spriteRenderer;
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;
    private Color originalColor;
    public float force;

    // COMBAT
    [Header("Combat")]
    public float chargedDashSpeed = 20f;
    public float chargedDashDuration = 0.5f;
    public int chargedDashDamage = 25;
    public float chargedDashKnockbackForce = 15f;
    public Color normalSwordColor = Color.white;
    private SpriteRenderer swordSprite;
    public Color chargedSwordColor = Color.cyan;
    private bool isFrozenAfterDash = false;
    public float freezeAfterDashDuration = 0.5f;
    private bool isChargingDash = false;
    private bool isCharging = false;

    private bool isChargedAttack = false;

    private Vector2 dashDirection;
    private bool isAttacking = false;
    public Transform attackOrigin;
    public float attackRadius = 1f;
    public LayerMask enemyMask;
    public float swordDamage = 1f;

    // INTERACCIÓ
    public float interactionRange = 2f;
    private Lever closestLever;
    public PlayerInput playerInput;

    // ATAC SECUNDARI (BATTLECRY)
    public float radius = 3f;
    public float buffDuration = 5f;
    public float cooldown = 10f;
    public LayerMask allyLayer;
    private bool canUseCry = true;
    public GameObject battleCryEffectPrefab;

    // INVINCIBILITAT
    public bool isInvincible = false;

    private bool isStunned = false;

    [Header("Item")]
    public Transform itemHolder;
    private GameObject heldItem;

    [Header("Player Manager")]
    // Referència al PlayerManager
    public GameObject playerManager;
    Vector3 originalScale;

    // CHECKPOINT
    //public Vector3 lastCheckpointPosition;

    // PARTICULES MORT
    public GameObject deathParticlesPrefab;
    public bool isDead = false;
    // SO
    public AudioSource audioSource;
    private Coroutine walkSoundCoroutine;

    private void Awake()
    {
        // SO
        audioSource = GetComponent<AudioSource>();
        // ESCALA, FISIQUES, SPRITE
        originalScale = transform.localScale; // Guarda l'escala original del jugador
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        playerInput = GetComponent<PlayerInput>();
        // MENU PAUSA
        playerInput.actions.FindActionMap("UI").Enable();
        // ESPASA I ESCUT
        Transform sword = transform.Find("WeaponHand/SwordPivot/Sword"); // desactivar el collider de l'espasa ja equipada
        hitPoint = transform.Find("HitPoint");
        stabPoint = transform.Find("StabPoint");
        if (sword != null)
        {
            swordSprite = sword.GetComponent<SpriteRenderer>();
            normalSwordColor = swordSprite.color; // Guarda el color original de l'espasa
            chargedSwordColor = Color.cyan;
            Collider2D col = sword.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;
        }

        // Assigna dinàmicament el PlayerManager
        if (playerManager == null)
        {
            playerManager = GameObject.FindWithTag("PlayerManager");
            if (playerManager == null)
                Debug.LogWarning("No s'ha trobat cap PlayerManager a l'escena!");
        }
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
        if (isDashing || isChargedAttack || isChargingDash)
        {
            return;
        }

        //if (isDead) return;

        GroundCheck();
        Flip();

        // SO DE CAMINAR
        bool isMoving = Mathf.Abs(horizontalMovement) > 0.1f && isGrounded;
        if (isMoving && walkSoundCoroutine == null)
        {
            walkSoundCoroutine = StartCoroutine(PlayWalkSound());
        }
        else if (!isMoving && walkSoundCoroutine != null)
        {
            StopCoroutine(walkSoundCoroutine);
            walkSoundCoroutine = null;
            //audioSource.Stop();
        }

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
        //if (isDead) return;

        if (isDashing || isChargedAttack || isChargingDash)
        {
            return;
        }

        if (!isBlocking && !isDashing)
        {
            rb.linearVelocity = new Vector2(horizontalMovement * speed, rb.linearVelocityY);
        }
    }


    public void OnMove(InputAction.CallbackContext ctx)
    {
        // Evita el moviment si està bloquejant, atacant, fent dash o frozen
        if (isBlocking || isAttacking || isDashing || isFrozenAfterDash || isChargedAttack || isChargingDash)
        {
            horizontalMovement = 0;
            animator.SetFloat("Speed", 0);
            return;
        }
        horizontalMovement = ctx.ReadValue<Vector2>().x;
        animator.SetFloat("Speed", Mathf.Abs(horizontalMovement));
        //animator.SetBool("Running", true);
        isRunning = true;
    }

    private IEnumerator PlayWalkSound()
    {
        while (true)
        {
            float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(horizontalMovement) / speed);
            float interval = Mathf.Lerp(0.6f, 0.15f, normalizedSpeed); // 0.4s lent, 0.15s ràpid

            audioSource.PlayOneShot(AudioManager.Instance.player2Walk);

            yield return new WaitForSeconds(interval);
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

                // So de salt normal o doble salt
                if (jumpsRemaining == maxJumps - 1)
                {
                    audioSource.PlayOneShot(AudioManager.Instance.player2Jump);
                }
                else if (jumpsRemaining == maxJumps - 2)
                {
                    audioSource.PlayOneShot(AudioManager.Instance.player2DoubleJump);
                }
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
        //isOverBox = Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, boxLayer);

        if (isGrounded)
        {
            jumpsRemaining = maxJumps;
            animator.SetBool("Jumping", false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Groundcheck
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        // Damage Zone
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackOrigin.position, attackRadius);
        // Battle cry
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);
        // Visualitzar el rang d’interacció
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        animator.SetBool("Dashing", true);

        // SO DE DASH
        audioSource.PlayOneShot(AudioManager.Instance.player2Dash);

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
                Debug.Log("Player2 ha recollit un objecte: " + pickup.name);
                break;
            }
        }
    }
    public bool HasHeldItem()
    {
        return heldItem != null;
    }

    // DROP
    public void OnDrop(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        if (heldItem != null)
        {
            // Desparenta l'objecte
            heldItem.transform.SetParent(null);
            //heldItem.transform.position = transform.position + Vector3.right * (isFacingRight ? 1f : -1f);

            // Activa el Rigidbody2D per la física
            Rigidbody2D rb = heldItem.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.simulated = true;
                rb.linearVelocity = Vector2.zero; // Per evitar que surti disparat
            }

            // Activa el Collider2D per detectar col·lisions
            Collider2D col = heldItem.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = true;

            heldItem = null; // Reseteja la referència
            Debug.Log("Objecte deixat.");
        }
        else
        {
            Debug.LogWarning("No tens cap objecte equipat per deixar!");
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Empenta caixes només durant dash normal (no atac carregat)
        if (isDashing && !isChargedAttack && collision.gameObject.CompareTag("Pushable"))
        {
            Rigidbody2D boxRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (boxRb != null)
            {
                boxRb.bodyType = RigidbodyType2D.Dynamic;

                Vector2 dashDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
                boxRb.AddForce(dashDirection * dashPushForce, ForceMode2D.Impulse);

                StartCoroutine(ResetBoxPhysics(boxRb));
            }
        }

        // Dany i knockback a enemics només en atac carregat
        if (isChargedAttack && collision.gameObject.CompareTag("Enemy"))
        {
            Rigidbody2D enemyRb = collision.gameObject.GetComponent<Rigidbody2D>();
            EnemyController enemyHealth = collision.gameObject.GetComponent<EnemyController>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(chargedDashDamage, transform);
            }

            if (enemyRb != null)
            {
                Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                enemyRb.AddForce(knockbackDir * chargedDashKnockbackForce, ForceMode2D.Impulse);
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

    public void ResetScale()
    {
        float xSign = isFacingRight ? 1f : -1f;
        transform.localScale = new Vector3(originalScale.x * xSign, originalScale.y, originalScale.z);
    }


    private void Flip()
    {
        float flipThreshold = 0.1f;

        if (isAttacking || isChargedAttack || isChargingDash) return;

        // Decideix la direcció
        bool flip = false;
        if (!isBlocking)
        {
            if ((isFacingRight && horizontalMovement < -flipThreshold) ||
                (!isFacingRight && horizontalMovement > flipThreshold))
            {
                isFacingRight = !isFacingRight;
                flip = true;
            }
        }
        else
        {
            if ((isFacingRight && aimInput.x < -flipThreshold) ||
                (!isFacingRight && aimInput.x > flipThreshold))
            {
                isFacingRight = !isFacingRight;
                flip = true;
            }
        }

        if (flip)
        {
            // Sempre parteix de l'escala original
            float xSign = isFacingRight ? 1f : -1f;
            transform.localScale = new Vector3(originalScale.x * xSign, originalScale.y, originalScale.z);
        }
    }
    /* JA ESTÀ EQUIPADA PER DEFECTE
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
    */

    // Getter per saber si el jugador està atacant
    public bool IsAttacking()
    {
        return isAttacking;
    }
    public void OnChargedAttack(InputAction.CallbackContext ctx)
    {
        float moveInput = playerInput.actions["Move"].ReadValue<Vector2>().x;
        if (ctx.started && isGrounded && Mathf.Abs(moveInput) > 0.5f)
        {
            isCharging = true;
            animator.SetBool("isCharging", true);
        }
        else if (ctx.canceled && isCharging)
        {
            isCharging = false;
            animator.SetBool("isCharging", false);

            // Inicia estocada carregada
            isChargedAttack = true;
            animator.SetTrigger("chargedAttack");

            // Direcció de l'estocada segons el moviment
            Vector2 stabDirection = new Vector2(Mathf.Sign(moveInput), 0f);
            StartCoroutine(PerformChargedStab(stabDirection));
        }
    }

    private IEnumerator PerformChargedStab(Vector2 stabDirection)
    {
        isChargingDash = true; // Bloqueja controls

        // SO DEL CHARGED ATTACK
        audioSource.PlayOneShot(AudioManager.Instance.player2ChargedAttack);

        // Dash automàtic
        float dashForce = 30f; // Ajusta segons el teu joc
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(stabDirection.normalized * dashForce, ForceMode2D.Impulse);

        swordSprite.color = chargedSwordColor;
        Debug.Log("Canviant color de l'espasa a carregada");
        yield return new WaitForSeconds(0.1f);
        PlayStabEffect();

        // Detecta enemics davant del jugador (ajusta la posició i mida segons el teu joc)
        Vector2 origin = (Vector2)attackOrigin.position + stabDirection * 0.7f;
        float stabRadius = 0.8f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, stabRadius, enemyMask);

        foreach (var hit in hits)
        {
            BreakableJar jar = hit.GetComponent<BreakableJar>();
            if (jar != null)
            {
                jar.Break();
            }
            EnemyController enemy = hit.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(chargedDashDamage, transform);
                // knockback, efectes, etc.
            }
        }

        // Espera la durada de l'animació d'estocada
        yield return new WaitForSeconds(0.3f);

        isChargedAttack = false;
        isChargingDash = false; // Desbloqueja controls
        normalSwordColor = Color.white;
        swordSprite.color = normalSwordColor; // Restaura el color original del jugador
    }

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // No pots atacar si estàs bloquejant, a l'aire, movent-te molt lent o ja estàs atacant
        if (isBlocking || !isGrounded || animator.GetFloat("Speed") < 0.1f || isAttacking)
            return;

        StartCoroutine(PerformAttack());
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        animator.SetFloat("Speed", 0);  // Atura animació de córrer

        sword.SetActive(true);
        equippedSwordTrail.enabled = true;
        animator.SetTrigger("Attack");

        // SO D'ATAC
        audioSource.PlayOneShot(AudioManager.Instance.player2Attack);

        // Esperar un petit moment per sincronitzar amb l'animació (opcional)
        yield return new WaitForSeconds(0.1f);
        PlaySlashEffect();

        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(attackOrigin.position, attackRadius, enemyMask);
        foreach (var target in enemiesInRange)
        {
            BreakableJar jar = target.GetComponent<BreakableJar>();
            if (jar != null)
            {
                jar.Break();
            }
            EnemyController enemy = target.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(swordDamage, transform);
            }
        }

        // Espera la durada real de l'animació d'atac
        yield return new WaitForSeconds(0.3f); // 0.1 + 0.4 = 0.5s total

        isAttacking = false;
        sword.SetActive(false);
        equippedSwordTrail.enabled = false;

    }

    void PlaySlashEffect()
    {
        // Determina la direcció
        float angle = isFacingRight ? 0f : 180f;
        Quaternion rotation = Quaternion.Euler(0, angle, 0);

        GameObject effect = Instantiate(slashEffectPrefab, hitPoint.position, rotation);

        Destroy(effect, 1f);
    }

    void PlayStabEffect()
    {
        // Determina la direcció
        float angle = isFacingRight ? 0f : 180f;
        Quaternion rotation = Quaternion.Euler(0, angle, 0);

        GameObject effect = Instantiate(stabEffectPrefab, stabPoint.position, rotation);

        Destroy(effect, 1f);
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
        if (shield == null)
        {
            Debug.LogError("Escut no trobat.");
        }
        else
        {
            Debug.Log("Escut equipat correctament.");
            // Només desactiva l'escut equipat, no el prefab original!
            if (shield.transform.parent == shieldHolder)
                shield.SetActive(false);
        }
    }

    public void OnAim(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();

        // Si jugues amb gamepad, l'input és direcció i el pots usar directament
        if (Gamepad.current != null && Gamepad.current.rightStick.IsActuated())
        {
            Vector2 direction = input.normalized;
            AimInDirection(direction);
        }
        else // si és des de ratolí, el valor és la posició del cursor
        {
            Vector3 mousePos = input; // En realitat és posició de pantalla
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector2 direction = (worldPos - transform.position).normalized;

            AimInDirection(direction);
        }
    }
    void AimInDirection(Vector2 direction)
    {
        // Aquí apuntaries o dispararies en la direcció correcta
        Debug.DrawRay(transform.position, direction * 2f, Color.red);
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

    // BLOQUEJAR AMB EL RATOLÍ
    public void OnBlock(InputAction.CallbackContext ctx)
    {
        if (shield == null) return;
        Debug.Log("BLOQUEIG ACTIVAT");
        if (ctx.performed)
        {
            isRightMouseHeld = true;
        }
        else if (ctx.canceled)
        {
            isRightMouseHeld = false;
        }

        UpdateBlocking();
    }
    private void UpdateBlocking()
    {
        float aimThreshold = 0.1f;

        // Només activem el bloqueig si el botó dret està premut i s’està apuntant
        Vector2 aimDir = GetAimDirection();

        if (isRightMouseHeld && aimDir.magnitude > aimThreshold)
        {
            isBlocking = true;
            shield.SetActive(true);
            animator.SetBool("isBlocking", true);

            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
            shieldHolder.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            isBlocking = false;
            animator.SetBool("isBlocking", false);
            shield.SetActive(false);
        }
    }

    // BLOQUEJAR AMB MANDO
    public void OnBlockStick(InputAction.CallbackContext ctx)
    {
        if (shield == null) return;

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

    // ATAC SECUNDARI (BATTLE CRY)
    public void OnBattleCry(InputAction.CallbackContext ctx)
    {
        if (!canUseCry) return;
        StartCoroutine(ActivateBattleCry());
        //StartCoroutine(ShakeCamera(0.2f, 0.5f)); // intensitat i durada

    }

    private IEnumerator<WaitForSeconds> ActivateBattleCry()
    {
        canUseCry = false;

        // SO DE BATTLECRY
        audioSource.PlayOneShot(AudioManager.Instance.player2BattleCry);

        // Reproduir VFXs
        Quaternion rotation = Quaternion.Euler(-90f, 0f, 0f);
        GameObject battleCryPrefab = Instantiate(battleCryEffectPrefab, transform.position, rotation);
        battleCryPrefab.transform.SetParent(transform); // es mou amb el jugador
        battleCryPrefab.transform.localPosition = Vector3.zero;
        //battleCryPrefab.transform.localScale = Vector3.one; 
        Destroy(battleCryPrefab, 5f);

        Collider2D[] allies = Physics2D.OverlapCircleAll(transform.position, radius, allyLayer);
        foreach (var ally in allies)
        {
            if (ally.TryGetComponent(out BattleCry buff))
            {
                buff.ApplyBuff(buffDuration);
            }
        }

        yield return new WaitForSeconds(cooldown);
        canUseCry = true;
    }

    // Funció per saber si està bloquejant (necessària per al Knockback)
    public bool IsBlocking()
    {
        return isBlocking;
    }

    // Funció per aplicar Knockback
    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (!isBlocking)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
            StartCoroutine(DisableMovement(0.3f));
        }
    }

    private IEnumerator DisableMovement(float duration)
    {
        isStunned = true;
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }
    public IEnumerator PlayDamageFlash()
    {
        Debug.Log("Flaix de mal activat!"); // Comprovació de si entra a la funció
        spriteRenderer.color = Color.red; // Color per al mal
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }
    public IEnumerator PlayDamagePulse()
    {
        float pulseScale = 1.2f;
        float pulseDuration = 0.1f;
        float xSign = isFacingRight ? 1f : -1f;

        // Escala cap amunt mantenint la direcció
        transform.localScale = new Vector3(originalScale.x * pulseScale * xSign, originalScale.y * pulseScale, originalScale.z);
        yield return new WaitForSeconds(pulseDuration);

        // Torna a la mida original i direcció correcta
        transform.localScale = new Vector3(originalScale.x * xSign, originalScale.y, originalScale.z);
    }

    public IEnumerator PlayBlockingFlash()
    {
        Debug.Log("Flaix de bloquejar activat!"); // Comprovació de si entra a la funció
        spriteRenderer.color = Color.cyan; // Color per al bloqueig
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }
    public void ApplyInvincibility(float duration)
    {
        StartCoroutine(InvincibilityCoroutine(duration));
    }

    private IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        // Aquí pots afegir efectes visuals
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }

    public void EquipItem(GameObject item)
    {
        if (heldItem != null)
        {
            Debug.LogWarning("Ja tens un objecte consumible equipat!");
            return;
        }

        heldItem = item;
        heldItem.transform.SetParent(itemHolder.transform);
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.SetActive(true);
        Debug.Log("Consumible equipat: " + item.name);
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
            playerManager.GetComponent<HealthSystem>().Heal("Player2", 20);
            audioSource.PlayOneShot(AudioManager.Instance.player2Heal);

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

    public void OnUseItem(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            UseHeldItem();
    }

    // PAUSAR LA PARTIDA
    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Debug.Log("Partida pausada per Player2");
            PauseManager.Instance?.TogglePause();
        }
    }

    // MORIR
    public void Die()
    {
        // Atura moviment i accions
        enabled = false;
        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger("isDead");

        // Instancia les partícules de mort
        if (deathParticlesPrefab != null)
            Instantiate(deathParticlesPrefab, transform.position, Quaternion.identity);

        // Drop d'objectes equipats
        if (heldItem != null)
        {
            heldItem.transform.SetParent(null);
            Rigidbody2D rbItem = heldItem.GetComponent<Rigidbody2D>();
            if (rbItem != null) rbItem.simulated = true;
            Collider2D col = heldItem.GetComponent<Collider2D>();
            if (col != null) col.enabled = true;
            heldItem = null;
        }

        // Atura tots els sons
        audioSource.Stop();
    }
    public void ReviveAtCheckpoint()
    {
        animator.ResetTrigger("isDead");
        animator.SetTrigger("Revive");
        enabled = true;
        // Mou els jugadors a la posició del checkpoint
        CheckpointManager.Instance.MovePlayersToCheckpoint();
    }
    public void GoToLastCheckpoint()
    {
        CheckpointManager.Instance.MovePlayersToCheckpoint();
    }

}
