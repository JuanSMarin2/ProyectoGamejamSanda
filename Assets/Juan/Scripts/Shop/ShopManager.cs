using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [SerializeField] private List<ShopItemData> shopItems = new();

    private readonly Dictionary<string, int> levelsByItemId = new();
    private readonly Dictionary<string, Action<int>> itemSpecificCallbacks = new();
    private readonly Dictionary<string, ShopItemUI> uiByItemId = new();

    public event Action<ShopItemData, int> OnItemPurchased;
    public event Action<ShopItemData, int> OnItemLevelChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ResetLocalState();
    }

    private void ResetLocalState()
    {
        levelsByItemId.Clear();

        foreach (ShopItemData item in shopItems)
        {
            if (item == null)
                continue;

            levelsByItemId[item.ItemId] = 0;
        }

        RefreshAllUI();
    }

    public void RegisterItem(ShopItemUI ui, ShopItemData itemData)
    {
        if (ui == null || itemData == null)
            return;

        uiByItemId[itemData.ItemId] = ui;

        ui.OnPurchaseRequested -= HandlePurchaseRequest;
        ui.OnPurchaseRequested += HandlePurchaseRequest;

        ui.Setup(itemData, GetCurrentLevel(itemData));
        RefreshItemUI(itemData);
    }

    public void UnregisterItem(ShopItemData itemData)
    {
        if (itemData == null)
            return;

        if (uiByItemId.ContainsKey(itemData.ItemId))
        {
            uiByItemId.Remove(itemData.ItemId);
        }
    }

    public void SubscribeToItemPurchase(string itemId, Action<int> callback)
    {
        if (string.IsNullOrWhiteSpace(itemId) || callback == null)
            return;

        if (!itemSpecificCallbacks.ContainsKey(itemId))
        {
            itemSpecificCallbacks[itemId] = null;
        }

        itemSpecificCallbacks[itemId] += callback;
    }

    public void UnsubscribeFromItemPurchase(string itemId, Action<int> callback)
    {
        if (string.IsNullOrWhiteSpace(itemId) || callback == null)
            return;

        if (itemSpecificCallbacks.TryGetValue(itemId, out Action<int> action))
        {
            action -= callback;
            itemSpecificCallbacks[itemId] = action;
        }
    }

    public int GetCurrentLevel(ShopItemData itemData)
    {
        if (itemData == null)
            return 0;

        if (!levelsByItemId.TryGetValue(itemData.ItemId, out int level))
        {
            levelsByItemId[itemData.ItemId] = 0;
            return 0;
        }

        return Mathf.Clamp(level, 0, itemData.MaxLevel);
    }

    public bool IsPurchased(ShopItemData itemData)
    {
        if (itemData == null)
            return false;

        return GetCurrentLevel(itemData) > 0;
    }

    public bool IsPurchased(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return false;

        return levelsByItemId.TryGetValue(itemId, out int level) && level > 0;
    }

    public bool IsMaxLevel(ShopItemData itemData)
    {
        if (itemData == null)
            return true;

        return GetCurrentLevel(itemData) >= itemData.MaxLevel;
    }

    public bool TryBuyItem(ShopItemData itemData)
    {
        if (itemData == null)
            return false;

        int currentLevel = GetCurrentLevel(itemData);

        if (currentLevel >= itemData.MaxLevel)
            return false;

        int cost = itemData.GetUpgradeCost(currentLevel);

        if (MoneyData.Instance == null)
        {
            Debug.LogWarning("ShopManager: MoneyData.Instance is null.");
            return false;
        }

        if (!MoneyData.Instance.CanAfford(cost))
            return false;

        bool paymentSucceeded = MoneyData.Instance.RemoveMoney(cost);

        if (!paymentSucceeded)
            return false;

        currentLevel++;
        levelsByItemId[itemData.ItemId] = currentLevel;
        AudioManager.instance.PlayOneShot(FMODEvents.instance.cachingComprar, transform.position);
        OnItemPurchased?.Invoke(itemData, currentLevel);
        OnItemLevelChanged?.Invoke(itemData, currentLevel);

        if (itemSpecificCallbacks.TryGetValue(itemData.ItemId, out Action<int> callback))
        {
            callback?.Invoke(currentLevel);
        }

        RefreshItemUI(itemData);
        return true;
    }

    private void HandlePurchaseRequest(ShopItemData itemData)
    {
        TryBuyItem(itemData);
    }

    private void RefreshAllUI()
    {
        foreach (ShopItemData item in shopItems)
        {
            if (item == null)
                continue;

            RefreshItemUI(item);
        }
    }

    private void RefreshItemUI(ShopItemData itemData)
    {
        if (itemData == null)
            return;

        if (!uiByItemId.TryGetValue(itemData.ItemId, out ShopItemUI ui))
            return;

        int currentLevel = GetCurrentLevel(itemData);
        int cost = itemData.GetUpgradeCost(currentLevel);

        bool canAfford = !IsMaxLevel(itemData) &&
                        MoneyData.Instance != null &&
                        MoneyData.Instance.CanAfford(cost);

        ui.Refresh(currentLevel, cost, canAfford);
    }
}
