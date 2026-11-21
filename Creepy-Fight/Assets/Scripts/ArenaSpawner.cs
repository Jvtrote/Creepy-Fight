using UnityEngine;
using UnityEngine.UI;

public class ArenaSpawner : MonoBehaviour
{
    [Header("Prefabs dos Lutadores (na mesma ordem dos botões: 0, 1, 2, 3)")]
    [SerializeField] private GameObject[] fighterPrefabs;

    [Header("Pontos de Posição na Arena")]
    [SerializeField] private Transform player1Spawn;
    [SerializeField] private Transform player2Spawn;

    [Header("Barras de Vida da UI")]
    [SerializeField] private Slider player1HealthSlider;
    [SerializeField] private Slider player2HealthSlider;

    void Start()
    {
        // Garante que o índice não estoure o tamanho da lista caso você ainda não tenha todos os 4 prefabs
        int p1Index = Mathf.Clamp(GameData.player1CharacterIndex, 0, fighterPrefabs.Length - 1);
        int p2Index = Mathf.Clamp(GameData.player2CharacterIndex, 0, fighterPrefabs.Length - 1);

        // Cria o Player 1 e o Oponente na cena
        GameObject p1 = Instantiate(fighterPrefabs[p1Index], player1Spawn.position, Quaternion.identity);
        GameObject p2 = Instantiate(fighterPrefabs[p2Index], player2Spawn.position, Quaternion.identity);

        // Conecta as barras de vida
        SetupHealth(p1, player1HealthSlider);
        SetupHealth(p2, player2HealthSlider);

        // Desativa o controle manual do oponente para ele não andar junto com o Player 1
        FighterController p2Controller = p2.GetComponent<FighterController>();
        if (p2Controller != null)
        {
            p2Controller.enabled = false;
        }
    }

    private void SetupHealth(GameObject fighter, Slider slider)
    {
        HealthController health = fighter.GetComponent<HealthController>();
        if (health != null && slider != null)
        {
            // Se healthSlider for private no seu HealthController, mude para public no HealthController.cs
            health.healthSlider = slider;
            slider.maxValue = health.maxHealth;
            slider.value = health.maxHealth;
        }
    }
}