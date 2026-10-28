using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Componentes de UI")]
    public TextMeshProUGUI timerText;       
    public TextMeshProUGUI instructionText; 

    [Header("Configuración de Tiempo")]
    public float timeRemaining = 180f; 
    private bool timerIsRunning = false;

    [Header("Panel de Cierre (Opcional)")]
    public GameObject endScreenPanel;

    void Start()
    {
        if (endScreenPanel != null)
        {
            endScreenPanel.SetActive(false);
        }

        timerIsRunning = true;
    }

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);

                if (timeRemaining <= 175f && instructionText != null)
                {
                    instructionText.gameObject.SetActive(false);
                }
            }
            else
            {
                timeRemaining = 0;
                timerIsRunning = false;
                TriggerEndSession();
            }
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        if (timeToDisplay < 0) timeToDisplay = 0;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void TriggerEndSession()
    {
        if (endScreenPanel != null)
        {
            endScreenPanel.SetActive(true);
        }
    }
}