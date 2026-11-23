using UnityEngine;
using TMPro;

public class MatchTimer : MonoBehaviour
{
    [Header("Configuração do Timer")]
    [SerializeField] private float startingTime = 99f;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private MatchManager matchManager;

    private float currentTime;
    private bool isRunning = true;

    void Start()
    {
        currentTime = startingTime;
        UpdateTimerUI();
    }

    void Update()
    {
        if (!isRunning) return;

        if (currentTime > 0f)
        {
            currentTime -= Time.deltaTime;

            if (currentTime < 0f)
            {
                currentTime = 0f;
            }

            UpdateTimerUI();
        }
        else
        {
            isRunning = false;
            OnTimeUp();
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;

        int secondsLeft = Mathf.CeilToInt(currentTime);
        timerText.text = secondsLeft.ToString("00");
    }

    private void OnTimeUp()
    {
        Debug.Log("Tempo esgotado!");
        if (matchManager != null)
        {
            matchManager.EndMatch("TEMPO ESGOTADO!");
        }
    }

    public void StopTimer()
    {
        isRunning = false;
    }
}