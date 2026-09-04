using UnityEngine;

public class BootsUpgradeReceiver : MonoBehaviour
{
    [SerializeField] private ShopItemData bootsItem;
    [SerializeField] private PlayerMovement playerMovement;

    private void Start()
    {
        if (bootsItem == null)
        {
            Debug.LogWarning("BootsUpgradeReceiver: bootsItem no está asignado.");
            return;
        }

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        if (ShopManager.Instance == null)
        {
            Debug.LogWarning("BootsUpgradeReceiver: no hay ShopManager en escena.");
            return;
        }

        if (ShopManager.Instance.IsPurchased(bootsItem))
        {
            int currentLevel = ShopManager.Instance.GetCurrentLevel(bootsItem);
            ApplySpeedMultiplier(currentLevel);
        }

        ShopManager.Instance.SubscribeToItemPurchase(bootsItem.ItemId, ApplySpeedMultiplier);
    }

    private void OnDestroy()
    {
        if (ShopManager.Instance != null && bootsItem != null)
        {
            ShopManager.Instance.UnsubscribeFromItemPurchase(bootsItem.ItemId, ApplySpeedMultiplier);
        }
    }

    private void ApplySpeedMultiplier(int purchasedLevel)
    {
        if (playerMovement == null)
            return;

        float multiplier = 1f + (Mathf.Min(purchasedLevel, 3) * 0.10f);
        playerMovement.SetMovementMultiplier(multiplier);
    }
}
