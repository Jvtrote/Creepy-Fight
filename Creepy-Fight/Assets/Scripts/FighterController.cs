using System.Collections;
using UnityEngine;

public class FighterController : MonoBehaviour
{
    public enum FighterState
    {
        Idle,
        Walking,
        Crouching,
        Jumping,
        Attacking,
        Blocking,
        Hitstun
    }

    [Header("Componentes")]
    private Rigidbody2D rb;
    private BoxCollider2D bodyCollider;
    private SpriteRenderer spriteRenderer;
    public Transform opponent; // Arraste o oponente aqui no Inspector

    [Header("Combate e Hitbox")]
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private string atk1Trigger = "Atk1";
    [SerializeField] private string atk2Trigger = "Atk2";
    [SerializeField] private string atk3Trigger = "Atk3";
    [SerializeField] private float comboResetWindow = 1.2f;
    [SerializeField] private float hitboxStartDelay = 0.1f;
    [SerializeField] private float hitboxActiveDuration = 0.25f;
    [SerializeField] private float attackRecoveryTime = 0.3f;
    [SerializeField] private float atk1Damage = 10f;
    [SerializeField] private float atk2Damage = 15f;
    [SerializeField] private float atk3Damage = 25f;

    [Header("Sprites (Visual Direto)")]
    [SerializeField] private Sprite idleSprite;    // Imagem do Standby
    [SerializeField] private Sprite walkingSprite; // Imagem/Frame de Andar
    private Animator anim;

    [Header("Atributos de Movimento")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Estado Atual")]
    public FighterState currentState = FighterState.Idle;

    // Configuração do Colisor
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    private bool isGrounded;
    private float horizontalInput;
    private bool isBlocking;

    // Controle de combo (1 -> 2 -> 3)
    private int comboStep = 0;
    private float lastAttackTime = -999f;
    private Coroutine attackRoutine;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (bodyCollider != null)
        {
            originalColliderSize = bodyCollider.size;
            originalColliderOffset = bodyCollider.offset;
        }

        if (rb != null)
        {
            rb.freezeRotation = true;
        }

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }

    void Start()
    {
        // Já inicia virado para o oponente no primeiro frame
        FlipFacingDirection();
    }

    void Update()
    {
        // Sempre mantém o lutador virado para o oponente (mesmo se ainda estiver se posicionando)
        if (currentState != FighterState.Attacking && currentState != FighterState.Hitstun)
        {
            FlipFacingDirection();
        }

        // Se estiver apanhando ou atacando, bloqueia movimentação livre
        if (currentState == FighterState.Hitstun || currentState == FighterState.Attacking)
            return;

        CheckGrounded();
        GetInputs();
        UpdateStateAndCollider();
        UpdateSpriteDirectly();
        UpdateAnimatorParameters();
    }

    void FixedUpdate()
    {
        if (currentState == FighterState.Hitstun)
            return;

        if (currentState == FighterState.Attacking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        Move();
    }

    private void GetInputs()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Pulo com a tecla W
        if (Input.GetKeyDown(KeyCode.W) && isGrounded && currentState != FighterState.Crouching)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            currentState = FighterState.Jumping;
        }

        // Ataque com a tecla J (sequência 1 -> 2 -> 3)
        if (Input.GetKeyDown(KeyCode.J))
        {
            StartComboAttack();
        }
    }

    private void StartComboAttack()
    {
        if (Time.time - lastAttackTime > comboResetWindow || comboStep >= 3)
        {
            comboStep = 0;
        }

        comboStep++;
        lastAttackTime = Time.time;

        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
        }

        attackRoutine = StartCoroutine(ExecuteAttack(comboStep));
    }

    private IEnumerator ExecuteAttack(int step)
    {
        currentState = FighterState.Attacking;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (anim != null)
        {
            if (HasAnimatorParameter("Andando", AnimatorControllerParameterType.Bool))
                anim.SetBool("Andando", false);

            if (step == 1 && HasAnimatorParameter(atk1Trigger, AnimatorControllerParameterType.Trigger))
                anim.SetTrigger(atk1Trigger);
            else if (step == 2 && HasAnimatorParameter(atk2Trigger, AnimatorControllerParameterType.Trigger))
                anim.SetTrigger(atk2Trigger);
            else if (step >= 3 && HasAnimatorParameter(atk3Trigger, AnimatorControllerParameterType.Trigger))
                anim.SetTrigger(atk3Trigger);
        }

        if (attackHitbox != null)
        {
            AttackHitbox hitboxScript = attackHitbox.GetComponent<AttackHitbox>();
            if (hitboxScript != null)
            {
                float dmg = (step == 1) ? atk1Damage : (step == 2) ? atk2Damage : atk3Damage;
                hitboxScript.SetDamage(dmg);
            }
        }

        if (hitboxStartDelay > 0f)
        {
            yield return new WaitForSeconds(hitboxStartDelay);
        }

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(true);
        }

        if (hitboxActiveDuration > 0f)
        {
            yield return new WaitForSeconds(hitboxActiveDuration);
        }

        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }

        if (attackRecoveryTime > 0f)
        {
            yield return new WaitForSeconds(attackRecoveryTime);
        }

        currentState = isGrounded ? FighterState.Idle : FighterState.Jumping;
        attackRoutine = null;
    }

    private bool HasAnimatorParameter(string paramName, AnimatorControllerParameterType type)
    {
        if (anim == null || string.IsNullOrEmpty(paramName)) return false;

        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName && param.type == type)
            {
                return true;
            }
        }
        return false;
    }

    private void Move()
    {
        if (isBlocking || (currentState == FighterState.Crouching && isGrounded))
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        if (anim != null && HasAnimatorParameter("Andando", AnimatorControllerParameterType.Bool))
        {
            anim.SetBool("Andando", rb.linearVelocity.x != 0 && isGrounded);
        }
    }

    private void CheckBlocking()
    {
        isBlocking = false;

        if (isGrounded && opponent != null)
        {
            if (opponent.position.x > transform.position.x && horizontalInput < -0.1f)
            {
                isBlocking = true;
            }
            else if (opponent.position.x < transform.position.x && horizontalInput > 0.1f)
            {
                isBlocking = true;
            }
        }
    }

    public bool IsBlocking()
    {
        return isBlocking;
    }

    private void UpdateAnimatorParameters()
    {
        if (anim == null) return;

        CheckBlocking();

        if (HasAnimatorParameter("Pulando", AnimatorControllerParameterType.Bool))
            anim.SetBool("Pulando", !isGrounded);

        if (HasAnimatorParameter("Agachado", AnimatorControllerParameterType.Bool))
            anim.SetBool("Agachado", currentState == FighterState.Crouching && isGrounded);

        if (HasAnimatorParameter("Bloqueando", AnimatorControllerParameterType.Bool))
            anim.SetBool("Bloqueando", isBlocking);
    }

    public void TriggerDamageResponse()
    {
        if (anim != null && HasAnimatorParameter("Dano", AnimatorControllerParameterType.Trigger))
        {
            anim.SetTrigger("Dano");
        }
    }

    private void UpdateStateAndCollider()
    {
        if (!isGrounded)
        {
            currentState = FighterState.Jumping;
            RestoreCollider();
            return;
        }

        if (isBlocking)
        {
            currentState = FighterState.Blocking;
            RestoreCollider();
            return;
        }

        if (Input.GetAxisRaw("Vertical") < -0.5f)
        {
            currentState = FighterState.Crouching;

            if (bodyCollider != null)
            {
                bodyCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y * 0.5f);
                bodyCollider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - (originalColliderSize.y * 0.25f));
            }
        }
        else
        {
            RestoreCollider();

            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                currentState = FighterState.Walking;
            }
            else
            {
                currentState = FighterState.Idle;
            }
        }
    }

    private void UpdateSpriteDirectly()
    {
        if (currentState == FighterState.Attacking || anim != null) return;
        if (spriteRenderer == null) return;

        if (currentState == FighterState.Walking && isGrounded && walkingSprite != null)
        {
            spriteRenderer.sprite = walkingSprite;
        }
        else if (currentState == FighterState.Idle && isGrounded && idleSprite != null)
        {
            spriteRenderer.sprite = idleSprite;
        }
    }

    private void RestoreCollider()
    {
        if (bodyCollider != null)
        {
            bodyCollider.size = originalColliderSize;
            bodyCollider.offset = originalColliderOffset;
        }
    }

    private void CheckGrounded()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
    }

    private void FlipFacingDirection()
    {
        if (opponent == null) return;

        bool shouldFaceLeft = opponent.position.x < transform.position.x;
        float direction = shouldFaceLeft ? -1f : 1f;
        float absScaleX = Mathf.Abs(transform.localScale.x);

        // Mantém o mesmo tamanho e espelha o objeto pai (e todos os filhos, incluindo a Hitbox)
        transform.localScale = new Vector3(absScaleX * direction, transform.localScale.y, transform.localScale.z);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
    public void SetOpponent(Transform newOpponent)
    {
        opponent = newOpponent;
        FlipFacingDirection();
    }
}