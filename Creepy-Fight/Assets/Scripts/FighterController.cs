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
    [SerializeField] private Transform opponent; // Arraste o oponente aqui no Inspector

    [Header("Sprites (Visual Direto)")]
    [SerializeField] private Sprite idleSprite;    // Imagem do Standby
    [SerializeField] private Sprite walkingSprite; // Imagem/Frame de Andar
    Animator anim;

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

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (bodyCollider != null)
        {
            originalColliderSize = bodyCollider.size;
            originalColliderOffset = bodyCollider.offset;
        }

        // Trava a rotação Z para o lutador não capotar
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
    }

    void Update()
    {
        // Se estiver apanhando ou atacando gravemente, trava o controle livre
        if (currentState == FighterState.Hitstun || currentState == FighterState.Attacking)
            return;

        CheckGrounded();
        GetInputs();
        UpdateStateAndCollider();
        UpdateSpriteDirectly();
        FlipFacingDirection();
        UpdateAnimatorParameters(); // Atualiza os parâmetros e triggers do Animator

        // TESTE DE DANO: Aperte a tecla J para tomar dano, tocar a animação e testar a barra
        if (Input.GetKeyDown(KeyCode.J))
        {
            TriggerDamageTest(10f);
        }
    }

    void FixedUpdate()
    {
        if (currentState == FighterState.Hitstun || currentState == FighterState.Attacking)
            return;

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
    }

    private void Move()
    {
        // Se estiver bloqueando ou agachado no chão, não anda
        if (isBlocking || (currentState == FighterState.Crouching && isGrounded))
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // Movimento arcade direto
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // Parâmetro de andar ensinado pelo professor
        if (rb.linearVelocity.x != 0 && isGrounded)
        {
            anim.SetBool("Andando", true);
        }
        else
        {
            anim.SetBool("Andando", false);
        }
    }

    // Lógica para detectar se o jogador está segurando para trás do oponente (Bloqueio)
    private void CheckBlocking()
    {
        isBlocking = false;

        if (isGrounded && opponent != null)
        {
            // Se o oponente está à direita (opponent.position.x > transform.position.x)
            // e o jogador aperta para a esquerda (horizontalInput < 0) -> Estará bloqueando!
            if (opponent.position.x > transform.position.x && horizontalInput < -0.1f)
            {
                isBlocking = true;
            }
            // Se o oponente está à esquerda e o jogador aperta para a direita -> Estará bloqueando!
            else if (opponent.position.x < transform.position.x && horizontalInput > 0.1f)
            {
                isBlocking = true;
            }
        }
    }

    private void UpdateAnimatorParameters()
    {
        if (anim == null) return;

        CheckBlocking();

        // Se não estiver no chão, ativa o parâmetro "Pulando"
        anim.SetBool("Pulando", !isGrounded);

        // Se estiver agachado, ativa o parâmetro "Agachado"
        anim.SetBool("Agachado", currentState == FighterState.Crouching && isGrounded);

        // Se estiver bloqueando, ativa o parâmetro "Bloqueando"
        anim.SetBool("Bloqueando", isBlocking);
    }

    // Método público que o HealthController ou o sistema de combate pode chamar ao levar dano
    public void TriggerDamageResponse()
    {
        if (anim != null)
        {
            // Dispara o Trigger de dano no Animator configurado pelo professor
            anim.SetTrigger("Dano");
        }
    }

    private void TriggerDamageTest(float damageAmount)
    {
        // Se estiver bloqueando, o dano é evitado ou reduzido
        if (isBlocking)
        {
            Debug.Log(gameObject.name + " bloqueou! Dano evitado.");
            return;
        }

        // Toca a animação de Trigger de Dano
        TriggerDamageResponse();

        // Subtrai a vida pelo HealthController
        HealthController health = GetComponent<HealthController>();
        if (health != null)
        {
            health.TakeDamage(damageAmount);
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

        // Agachamento (segurando para baixo ou 'S')
        if (Input.GetAxisRaw("Vertical") < -0.5f)
        {
            currentState = FighterState.Crouching;

            // Reduz o colisor pela metade
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
        if (spriteRenderer == null) return;

        if (currentState == FighterState.Walking && isGrounded)
        {
            if (walkingSprite != null)
            {
                spriteRenderer.sprite = walkingSprite;
            }
        }
        else if (currentState == FighterState.Idle && isGrounded)
        {
            if (idleSprite != null)
            {
                spriteRenderer.sprite = idleSprite;
            }
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

        if (opponent.position.x < transform.position.x && transform.localScale.x > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (opponent.position.x > transform.position.x && transform.localScale.x < 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}