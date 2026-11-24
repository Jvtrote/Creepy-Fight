using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] private float damage = 15f;
    private GameObject owner;
    private bool hasHitThisSwing = false;

    void Awake()
    {
        owner = transform.root.gameObject;
    }

    void OnEnable()
    {
        // Reseta toda vez que a hitbox é ativada para um novo golpe
        hasHitThisSwing = false;
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHitThisSwing) return;

        // Nunca acerta o próprio lutador nem qualquer objeto filho dele
        if (collision.transform.root.gameObject == owner) return;

        HealthController enemyHealth = collision.GetComponentInParent<HealthController>();
        FighterController enemyFighter = collision.GetComponentInParent<FighterController>();

        if (enemyHealth != null)
        {
            hasHitThisSwing = true;

            // Se o oponente estiver bloqueando, não tira vida
            if (enemyFighter != null && enemyFighter.IsBlocking())
            {
                Debug.Log(enemyHealth.gameObject.name + " bloqueou o golpe!");
                gameObject.SetActive(false);
                return;
            }

            enemyHealth.TakeDamage(damage);

            if (enemyFighter != null)
            {
                enemyFighter.TriggerDamageResponse();
            }

            gameObject.SetActive(false);
        }
    }
}