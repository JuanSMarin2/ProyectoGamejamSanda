using System.Collections.Generic;
using UnityEngine;

public class ItemSellerData : MonoBehaviour
{
    public static ItemSellerData Instance { get; private set; }

    private readonly Dictionary<string, int> lastPurchaseDayBySeller = new();

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

    public static ItemSellerData GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        GameObject holder = new GameObject("ItemSellerData");
        return holder.AddComponent<ItemSellerData>();
    }

    public bool HasBoughtToday(string sellerId)
    {
        if (string.IsNullOrEmpty(sellerId))
            return false;

        return lastPurchaseDayBySeller.TryGetValue(sellerId, out int day) &&
               day == (DayManager.Instance != null ? DayManager.Instance.CurrentDay : 1);
    }

    public void RegisterPurchase(string sellerId)
    {
        if (string.IsNullOrEmpty(sellerId))
            return;

        lastPurchaseDayBySeller[sellerId] =
            DayManager.Instance != null ? DayManager.Instance.CurrentDay : 1;
    }
}