using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[System.Serializable]
public class DayQuota
{
    public int day;
    public int rent;
}


public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }


    [SerializeField] private List<DayQuota> days = new();


    public event Action<int, int> OnDayStarted;
    public event Action<bool, int, int> OnDayResults;
    public event Action OnGameFinished;


    public int CurrentDay { get; private set; } = 1;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;

        DontDestroyOnLoad(gameObject);
    }


    private void Start()
    {
        StartCurrentDay();
    }


    public int GetCurrentRent()
    {
        if (CurrentDay - 1 < 0 || CurrentDay - 1 >= days.Count)
            return 0;

        return days[CurrentDay - 1].rent;
    }


    public void FinishDay()
    {
        int rent = GetCurrentRent();

        int money = 0;


        if (MoneyData.Instance != null)
            money = MoneyData.Instance.Money;


        Debug.Log("DayManager: Fin del día. Dinero: $" + money + " Renta: $" + rent);


        if (money < rent)
        {
            OnDayResults?.Invoke(false, money, rent);
            return;
        }


        if (MoneyData.Instance != null)
            MoneyData.Instance.RemoveMoney(rent);


        if (CurrentDay >= days.Count)
        {
            OnGameFinished?.Invoke();
            return;
        }


        OnDayResults?.Invoke(true, money, rent);
    }


    public void ContinueToNextDay()
    {
        if (CurrentDay >= days.Count)
            return;


        CurrentDay++;

        StartCurrentDay();
    }


    private void StartCurrentDay()
    {
        if (TimeManager.Instance == null)
            return;


        TimeManager.Instance.StartDay();


        OnDayStarted?.Invoke(
            CurrentDay,
            GetCurrentRent()
        );
    }


    public void ReturnToMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("MainMenu");
    }


    public void FinishGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("MainMenu");
    }
}