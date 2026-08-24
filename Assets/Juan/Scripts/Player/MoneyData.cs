using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;   
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
      
      // Play coin sound effect when money is gained
      EventInstance coinInstance = AudioManager.instance.CreateInstance(FMODEvents.instance.recolectarDinero);
        coinInstance.setParameterByName("CoinCount", (float)amount);
        coinInstance.start();
        coinInstance.release();
        OnMoneyChanged?.Invoke();
    }


    public bool RemoveMoney(int amount)
    {
        if (amount <= 0 || !CanAfford(amount))
            return false;

        money -= amount;
        // Play coin sound effect when money is removed
        EventInstance coinInstance = AudioManager.instance.CreateInstance(FMODEvents.instance.recolectarDinero);
         coinInstance.setParameterByName("CoinCount", (float)amount);
        coinInstance.start();
        coinInstance.release();
        OnMoneyChanged?.Invoke();
        return true;
    }


    public bool CanAfford(int amount)
    {
        return money >= amount;
    }
}