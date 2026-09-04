using UnityEngine;

public class InventorySpaceUpgradeReceiver : MonoBehaviour
{
    [SerializeField] private ShopItemData inventorySpaceItem;

    private void Start()
    {
        if (inventorySpaceItem == null)
        {
            Debug.LogWarning("InventorySpaceUpgradeReceiver: inventorySpaceItem no esta asignado.");
            return;
        }

        if (ShopManager.Instance == null)
        {
            Debug.LogWarning("InventorySpaceUpgradeReceiver: no hay ShopManager en escena.");
            return;
        }

        ShopManager.Instance.SubscribeToItemPurchase(
            inventorySpaceItem.ItemId,
            ApplyInventoryIncrease
        );
    }

    private void OnDestroy()
    {
        if (ShopManager.Instance != null && inventorySpaceItem != null)
        {
            ShopManager.Instance.UnsubscribeFromItemPurchase(
                inventorySpaceItem.ItemId,
                ApplyInventoryIncrease
            );
        }
    }

    private void ApplyInventoryIncrease(int purchasedLevel)
    {
        if (InventoryData.Instance == null)
            return;

        InventoryData.Instance.IncreaseInventory();

        Debug.Log($"[SHOP] Ampliacion de inventario comprada (nivel {purchasedLevel}).");
    }
}