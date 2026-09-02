using System.Collections.Generic;
using UnityEngine;

public class ItemSeller : MonoBehaviour
{
    [SerializeField] private string sellerId;
    [SerializeField] private List<ObjectData> itemPool = new();

    public bool HasBoughtToday()
    {
        return ItemSellerData.Instance != null &&
               ItemSellerData.Instance.HasBoughtToday(sellerId);
    }

    public bool HasSpaceForAnyItem()
    {
        if (HasBoughtToday() || itemPool == null || InventoryData.Instance == null)
            return false;

        foreach (ObjectData item in itemPool)
        {
            if (item == null)
                continue;

            if (InventoryData.Instance.CountByCategory(item.Category) <
                InventoryData.GetCategoryLimit(item.Category))
            {
                return true;
            }
        }

        return false;
    }

    public bool BuyItem()
    {
        if (HasBoughtToday())
        {
            Debug.Log($"[SELLER] '{sellerId}' ya vendio su objeto de hoy.");
            return false;
        }

        if (itemPool == null || itemPool.Count == 0)
        {
            Debug.LogWarning($"[SELLER] '{sellerId}' no tiene objetos en su pool.");
            return false;
        }

        if (InventoryData.Instance == null)
        {
            Debug.LogWarning("[SELLER] No hay InventoryData en escena.");
            return false;
        }

        List<ObjectData> candidates = new List<ObjectData>(itemPool);

        while (candidates.Count > 0)
        {
            int index = Random.Range(0, candidates.Count);
            ObjectData item = candidates[index];
            candidates.RemoveAt(index);

            if (item == null)
                continue;

            if (InventoryData.Instance.AddItem(item))
            {
                ItemSellerData.GetOrCreate().RegisterPurchase(sellerId);

                if (TrashFeedback.Instance != null)
                {
                    TrashFeedback.Instance.ShowFeedback(
                        new List<ObjectData> { item },
                        0,
                        false
                    );
                }

                Debug.Log($"[SELLER] '{sellerId}' vendio: {item.itemName} (id {item.id}).");

                return true;
            }
        }

        Debug.LogWarning($"[SELLER] No se pudo dar el objeto de '{sellerId}' (inventario lleno?).");

        return false;
    }
}