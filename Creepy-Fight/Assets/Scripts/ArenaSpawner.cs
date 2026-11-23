using UnityEngine;
using UnityEngine.UI;

public class ArenaSpawner : MonoBehaviour
{
    [Header("Prefabs dos Lutadores (0: Jeff, 1: Rake, 2: Sonic.exe, 3: Slenderman)")]
    [SerializeField] private GameObject[] fighterPrefabs;

    [Header("Pontos de Posição na Arena")]
    [SerializeField] private Transform player1Spawn;
    [SerializeField] private Transform player2Spawn;

    [Header("Barras de Vida da UI")]
    [SerializeField] private Slider player1HealthSlider;
    [SerializeField] private Slider player2HealthSlider;

    [Header("Gerenciador da Partida")]
    [SerializeField] private MatchManager matchManager;

    void Start()
    {
        // Garante que o índice escolhido exista dentro do tamanho da lista
        int p1Index = Mathf.Clamp(GameData.player1CharacterIndex, 0, fighterPrefabs.Length - 1);
        int p2Index = Mathf.Clamp(GameData.player2CharacterIndex, 0, fighterPrefabs.Length - 1);

        // Cria o Player 1 e o Oponente na cena
        GameObject p1 = Instantiate(fighterPrefabs[p1Index], player1Spawn.position, Quaternion.identity);
        GameObject p2 = Instantiate(fighterPrefabs[p2Index], player2Spawn.position, Quaternion.identity);

        // Liga o oponente ao Player 1 para ele virar automaticamente na direção certa
        FighterController p1Controller = p1.GetComponent<FighterController>();
        if (p1Controller != null)
        {
            p1Controller.opponent = p2.transform;
        }

        // Conecta as barras de vida aos respectivos lutadores
        SetupHealth(p1, player1HealthSlider);
        SetupHealth(p2, player2HealthSlider);

        // Desativa o controle manual do oponente para ele não andar junto com o Player 1
        FighterController p2Controller = p2.GetComponent<FighterController>();
        if (p2Controller != null)
        {
            p2Controller.enabled = false;
        }

        // Registra os dois lutadores no MatchManager para monitorar K.O. e fim de round
        if (matchManager != null)
        {
            matchManager.RegisterFighters(p1, p2);
        }
    }

    private void SetupHealth(GameObject fighter, Slider slider)
    {
        HealthController health = fighter.GetComponent<HealthController>();
        if (health != null && slider != null)
        {
            health.healthSlider = slider;
            slider.maxValue = health.maxHealth;
            slider.value = health.maxHealth;
        }
    }
}