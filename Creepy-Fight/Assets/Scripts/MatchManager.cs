using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MatchManager : MonoBehaviour
{
    [Header("Referências da UI")]
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private GameObject menuButton;
    [SerializeField] private MatchTimer matchTimer;

    private HealthController p1Health;
    private HealthController p2Health;
    private FighterController p1Controller;
    private bool matchEnded = false;

    // Chamado pelo ArenaSpawner assim que os lutadores nascem
    public void RegisterFighters(GameObject p1, GameObject p2)
    {
        p1Health = p1.GetComponent<HealthController>();
        p2Health = p2.GetComponent<HealthController>();
        p1Controller = p1.GetComponent<FighterController>();
    }

    void Update()
    {
        if (matchEnded || p1Health == null || p2Health == null) return;

        // Verifica se alguém zerou a vida
        if (p1Health.currentHealth <= 0)
        {
            EndMatch("DERROTA - K.O.");
        }
        else if (p2Health.currentHealth <= 0)
        {
            EndMatch("VITÓRIA - K.O.");
        }
    }

    public void EndMatch(string message)
    {
        if (matchEnded) return;
        matchEnded = true;

        // Para o cronômetro
        if (matchTimer != null) matchTimer.StopTimer();

        // Trava o controle do jogador para não continuar batendo depois do K.O.
        if (p1Controller != null) p1Controller.enabled = false;

        // Mostra o texto de resultado e o botão de voltar
        if (resultText != null)
        {
            resultText.text = message;
            resultText.gameObject.SetActive(true);
        }

        if (menuButton != null)
        {
            menuButton.SetActive(true);
        }
    }

    // Conecte essa função no OnClick do BtnVoltarMenu
    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}