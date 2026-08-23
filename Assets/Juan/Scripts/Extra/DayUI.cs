using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class DayUI : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text rentText;


    [Header("Results")]
    [SerializeField] private GameObject dayResultsPanel;
    [SerializeField] private TMP_Text resultsText;

    [SerializeField] private Button continueButton;
    [SerializeField] private Button returnToMenuButton;
    [SerializeField] private Button finishButton;


    private void Start()
    {
        Subscribe();

        UpdateVisual();

        Debug.Log("DayUI iniciado.");
    }


    private void OnDestroy()
    {
        Unsubscribe();
    }


    private void Subscribe()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnDayResults += ShowResults;
            DayManager.Instance.OnGameFinished += ShowGameFinished;

            Debug.Log("DayUI conectado a DayManager.");
        }
        else
        {
            Debug.LogError("DayUI: DayManager.Instance es NULL.");
        }


        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueDay);

        if (returnToMenuButton != null)
            returnToMenuButton.onClick.AddListener(ReturnToMenu);

        if (finishButton != null)
            finishButton.onClick.AddListener(FinishGame);
    }


    private void Unsubscribe()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnDayResults -= ShowResults;
            DayManager.Instance.OnGameFinished -= ShowGameFinished;
        }


        if (continueButton != null)
            continueButton.onClick.RemoveListener(ContinueDay);

        if (returnToMenuButton != null)
            returnToMenuButton.onClick.RemoveListener(ReturnToMenu);

        if (finishButton != null)
            finishButton.onClick.RemoveListener(FinishGame);
    }


    private void Update()
    {
        UpdateTime();
        UpdateDay();
        UpdateRent();
    }


    private void UpdateVisual()
    {
        UpdateTime();
        UpdateDay();
        UpdateRent();
    }


    private void UpdateTime()
    {
        if (TimeManager.Instance == null || timeText == null)
            return;


        int hour = TimeManager.Instance.CurrentHour;
        int minute = TimeManager.Instance.CurrentMinute;


        string suffix = hour >= 12 ? "PM" : "AM";

        int displayHour = hour;


        if (displayHour > 12)
            displayHour -= 12;

        if (displayHour == 0)
            displayHour = 12;


        timeText.text =
            displayHour.ToString("00") +
            ":" +
            minute.ToString("00") +
            " " +
            suffix;
    }


    private void UpdateDay()
    {
        if (DayManager.Instance == null || dayText == null)
            return;


        dayText.text =
            "Día " +
            DayManager.Instance.CurrentDay;
    }


    private void UpdateRent()
    {
        if (DayManager.Instance == null ||
            MoneyData.Instance == null ||
            rentText == null)
            return;


        int money = MoneyData.Instance.Money;
        int rent = DayManager.Instance.GetCurrentRent();


        rentText.text =
            "Renta: $" +
            money +
            " / $" +
            rent;


        if (money >= rent)
            rentText.color = Color.green;
        else
            rentText.color = Color.red;
    }


    private void ShowResults(bool paid, int money, int rent)
    {
        Debug.Log("DayUI: Recibí resultado del día.");


        if (dayResultsPanel == null)
        {
            Debug.LogError("DayUI: dayResultsPanel es NULL.");
            return;
        }


        dayResultsPanel.SetActive(true);


        if (resultsText != null)
        {
            if (paid)
            {
                resultsText.text =
    "Fin del día! 6:00PM\n" +
    "Pagaste $" + rent + " de arriendo.\n" +
    "Conservas $" + (money - rent) + ".";
            }
            else
            {
                resultsText.text =
                    "GAME OVER\n" +
                    "No tienes suficiente dinero para pagar el arriendo.";
            }
        }


        if (continueButton != null)
            continueButton.gameObject.SetActive(paid);

        if (returnToMenuButton != null)
            returnToMenuButton.gameObject.SetActive(!paid);

        if (finishButton != null)
            finishButton.gameObject.SetActive(false);
    }


    private void ShowGameFinished()
    {
        Debug.Log("DayUI: Juego terminado.");


        if (dayResultsPanel == null)
            return;


        dayResultsPanel.SetActive(true);


        if (resultsText != null)
        {
            resultsText.text =
                "¡Felicitaciones!\n" +
                "Has completado todos los días y has ganado.";
        }


        if (continueButton != null)
            continueButton.gameObject.SetActive(false);

        if (returnToMenuButton != null)
            returnToMenuButton.gameObject.SetActive(false);

        if (finishButton != null)
            finishButton.gameObject.SetActive(true);
    }


    private void ContinueDay()
    {
        if (dayResultsPanel != null)
            dayResultsPanel.SetActive(false);


        DayManager.Instance.ContinueToNextDay();
    }


    private void ReturnToMenu()
    {
        DayManager.Instance.ReturnToMenu();
    }


    private void FinishGame()
    {
        DayManager.Instance.FinishGame();
    }
}