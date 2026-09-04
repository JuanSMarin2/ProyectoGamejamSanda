using UnityEngine;

public class InventoryIncreaseSeller : MonoBehaviour
{
    [SerializeField] private string sellerId;

    public bool HasBoughtToday()
    {
        return ItemSellerData.Instance != null &&
               ItemSellerData.Instance.HasBoughtToday(sellerId);
    }

    public bool CanSellIncrease()
    {
        if (HasBoughtToday() || InventoryData.Instance == null)
            return false;

        return InventoryData.Instance.CanIncreaseInventory();
    }

    public bool BuyIncrease()
    {
        if (!CanSellIncrease())
        {
            Debug.Log($"[SELLER] '{sellerId}' no puede vender la ampliacion (ya vendio hoy o inventario al maximo).");
            return false;
        }

        InventoryData.Instance.IncreaseInventory();

        ItemSellerData.GetOrCreate().RegisterPurchase(sellerId);

        Debug.Log($"[SELLER] '{sellerId}' vendio una ampliacion de inventario.");

        return true;
    }
}