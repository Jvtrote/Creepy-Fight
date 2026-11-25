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

        FighterController p1Controller = p1.GetComponent<FighterController>();
        FighterController p2Controller = p2.GetComponent<FighterController>();

        // Liga os dois lutadores um ao outro e força a virada imediata
        if (p1Controller != null)
        {
            p1Controller.opponent = p2.transform;
            p1Controller.SendMessage("FlipFacingDirection", SendMessageOptions.DontRequireReceiver);
        }

        if (p2Controller != null)
        {
            p2Controller.opponent = p1.transform;
            p2Controller.SendMessage("FlipFacingDirection", SendMessageOptions.DontRequireReceiver);
        }

        // Conecta as barras de vida aos respectivos lutadores
        SetupHealth(p1, player1HealthSlider);
        SetupHealth(p2, player2HealthSlider);

        // Desativa o controle manual do oponente para ele não andar junto com o Player 1
        if (p2Controller != null)
        {
            p2Controller.enabled = false;

            // Mantém o oponente sempre olhando para o Player 1 mesmo com o controle desligado
            AutoFaceTarget autoFace = p2.AddComponent<AutoFaceTarget>();
            autoFace.target = p1.transform;
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

// Componente leve que mantém o oponente virado para o alvo enquanto está no modo treino/IA
public class AutoFaceTarget : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (target == null) return;

        bool shouldFaceLeft = target.position.x < transform.position.x;
        float absScaleX = Mathf.Abs(transform.localScale.x);
        transform.localScale = new Vector3(shouldFaceLeft ? -absScaleX : absScaleX, transform.localScale.y, transform.localScale.z);
    }
}