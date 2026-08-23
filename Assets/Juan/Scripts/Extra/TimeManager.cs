using System;
using UnityEngine;


public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }


    [SerializeField] private int startHour = 6;
    [SerializeField] private int endHour = 18;
    [SerializeField] private float realSecondsPerGameHour = 60f;


    private float currentTime;
    private bool dayStarted;
    private bool dayEnded;


    public event Action<int, int> OnTimeChanged;


    public int CurrentHour { get; private set; }
    public int CurrentMinute { get; private set; }


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


    private void Update()
    {
        if (!dayStarted || dayEnded)
            return;


        currentTime += Time.deltaTime;

        UpdateTime();


        if (currentTime >= (endHour - startHour) * realSecondsPerGameHour)
        {
            EndDay();
        }
    }


    public void StartDay()
    {
        currentTime = 0f;
        dayStarted = true;
        dayEnded = false;

        CurrentHour = startHour;
        CurrentMinute = 0;

        Time.timeScale = 1f;

        OnTimeChanged?.Invoke(CurrentHour, CurrentMinute);
    }


    private void UpdateTime()
    {
        float gameHours = currentTime / realSecondsPerGameHour;

        int totalMinutes = Mathf.FloorToInt(gameHours * 60f);

        int newHour = startHour + totalMinutes / 60;
        int newMinute = totalMinutes % 60;


        newMinute = Mathf.FloorToInt(newMinute / 30f) * 30;


        if (newHour != CurrentHour || newMinute != CurrentMinute)
        {
            CurrentHour = newHour;
            CurrentMinute = newMinute;

            OnTimeChanged?.Invoke(CurrentHour, CurrentMinute);
        }
    }


    private void EndDay()
    {
        if (dayEnded)
            return;


        dayEnded = true;

        CurrentHour = endHour;
        CurrentMinute = 0;

        OnTimeChanged?.Invoke(CurrentHour, CurrentMinute);


        if (DayManager.Instance != null)
        {
            DayManager.Instance.FinishDay();
        }


        Time.timeScale = 0f;
    }
}