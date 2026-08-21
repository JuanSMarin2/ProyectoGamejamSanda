using UnityEngine;

public class MoneyData : MonoBehaviour
{
    public static MoneyData Instance { get; private set; }

    [SerializeField] private int money = 0;


    public int Money => money;




    private void Awake() {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    Instance = this;
        DontDestroyOnLoad(gameObject);
    }


 public void AddMoney(int amount)
    {  money += amount; }


    public void RemoveMoney(int amount)
    { money -= amount; }

public bool CanAfford(int amount)
    {
        return money >= amount;
    }
}