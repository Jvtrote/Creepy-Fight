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
        Hitstun
    }

    [Header("Componentes")]
    private Rigidbody2D rb;
    private BoxCollider2D bodyCollider;
    [SerializeField] private Transform opponent; // Arraste o oponente aqui no Inspector

    [Header("Atributos de Movimento")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Estado Atual")]
    public FighterState currentState = FighterState.Idle;

    // Configuração do Colisor (para o Agachamento)
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    private bool isGrounded;
    private float horizontalInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<BoxCollider2D>();

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
        // Se estiver apanhando ou atacando, bloqueia os comandos de movimento
        if (currentState == FighterState.Hitstun || currentState == FighterState.Attacking)
            return;

        CheckGrounded();
        GetInputs();
        UpdateStateAndCollider();
        FlipFacingDirection();
    }

    void FixedUpdate()
    {
        // Se estiver apanhando ou atacando, não aplica movimento do teclado
        if (currentState == FighterState.Hitstun || currentState == FighterState.Attacking)
            return;

        Move();
    }

    private void GetInputs()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Pulo
        if (Input.GetButtonDown("Jump") && isGrounded && currentState != FighterState.Crouching)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            currentState = FighterState.Jumping;
        }
    }

    private void Move()
    {
        // Se estiver agachado no chão, o lutador não deve andar
        if (currentState == FighterState.Crouching && isGrounded)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // Movimento arcade direto e responsivo
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    private void UpdateStateAndCollider()
    {
        if (!isGrounded)
        {
            currentState = FighterState.Jumping;
            RestoreCollider();
            return;
        }

        // Agachamento (segurando para baixo ou 'S')
        if (Input.GetAxisRaw("Vertical") < -0.5f)
        {
            currentState = FighterState.Crouching;

            // Reduz o colisor pela metade para esquivar de golpes altos
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

        // Se o oponente estiver à esquerda e estarmos olhando para a direita (ou vice-versa)
        if (opponent.position.x < transform.position.x && transform.localScale.x > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (opponent.position.x > transform.position.x && transform.localScale.x < 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    // Desenha a esfera do GroundCheck na aba Scene para ajudar no ajuste visual
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}