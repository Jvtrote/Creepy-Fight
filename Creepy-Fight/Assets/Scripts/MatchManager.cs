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
    private bool fightersReady = false;
    private float elapsedAfterSpawn = 0f;

    public void RegisterFighters(GameObject p1, GameObject p2)
    {
        p1Health = p1.GetComponent<HealthController>();
        p2Health = p2.GetComponent<HealthController>();
        p1Controller = p1.GetComponent<FighterController>();
        fightersReady = true;
        elapsedAfterSpawn = 0f;
    }

    void Update()
    {
        if (matchEnded || !fightersReady || p1Health == null || p2Health == null) return;

        // Pequena janela de 0.2s após o spawn para todos os Start() e Sliders terminarem de rodar
        elapsedAfterSpawn += Time.deltaTime;
        if (elapsedAfterSpawn < 0.2f) return;

        // Só considera K.O. se a vida máxima já estiver válida e a atual tiver chegado a zero
        if (p1Health.maxHealth > 0 && p1Health.currentHealth <= 0)
        {
            EndMatch("DERROTA - K.O.");
        }
        else if (p2Health.maxHealth > 0 && p2Health.currentHealth <= 0)
        {
            EndMatch("VITÓRIA - K.O.");
        }
    }

    public void EndMatch(string message)
    {
        if (matchEnded) return;
        matchEnded = true;

        if (matchTimer != null) matchTimer.StopTimer();
        if (p1Controller != null) p1Controller.enabled = false;

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

    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}