using System;
using UnityEngine;

public class MoneyData : MonoBehaviour
{
    public static MoneyData Instance { get; private set; }

    [SerializeField] private int money = 0;


    public int Money => money;


    public event Action OnMoneyChanged;


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


    public void AddMoney(int amount)
    {
        if (amount <= 0)
            return;

        money += amount;
        OnMoneyChanged?.Invoke();
    }


    public bool RemoveMoney(int amount)
    {
        if (amount <= 0 || !CanAfford(amount))
            return false;

        money -= amount;
        OnMoneyChanged?.Invoke();
        return true;
    }


    public bool CanAfford(int amount)
    {
        return money >= amount;
    }
}