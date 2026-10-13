using UnityEngine;
using UnityEngine.UI;

public class HealthController : MonoBehaviour
{
    [Header("Configuração de Vida")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Interface (UI)")]
    [SerializeField] private Slider healthSlider;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    // Função para tomar dano
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Atualiza a barra visualmente
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        // Verifica se a vida acabou
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " foi derrotado!");
        // Aqui você pode desativar o lutador ou reiniciar a fase depois
    }
}