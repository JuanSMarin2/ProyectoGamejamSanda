using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button buyButton;
    [SerializeField] private TextMeshProUGUI buyButtonText;

    private ShopItemData itemData;
    private int currentLevel;
    private int currentPrice;

    public event System.Action<ShopItemData> OnPurchaseRequested;

    public void Setup(ShopItemData data, int level)
    {
        itemData = data;
        currentLevel = Mathf.Clamp(level, 0, data.MaxLevel);

        if (itemData == null)
        {
            Debug.LogWarning("ShopItemUI.Setup called with null data.");
            return;
        }

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() =>
        {
            if (itemData == null)
                return;

            OnPurchaseRequested?.Invoke(itemData);
        });

        RefreshVisuals();
    }

    public void Refresh(int level, int price, bool canAfford)
    {
        if (itemData == null)
            return;

        currentLevel = Mathf.Clamp(level, 0, itemData.MaxLevel);
        currentPrice = price;

        bool isMaxLevel = currentLevel >= itemData.MaxLevel;

        itemIcon.sprite = itemData.Icon;
        itemNameText.text = itemData.ItemName;
        descriptionText.text = itemData.Description;

        if (isMaxLevel)
        {
            levelText.text = "MAX";
            priceText.text = "MAX";
            buyButtonText.text = "MAX";
            buyButton.interactable = false;
            return;
        }

        levelText.text = $"Lv {currentLevel}/{itemData.MaxLevel}";
        priceText.text = $"{currentPrice}";
        buyButtonText.text = "BUY";
        buyButton.interactable = canAfford;
        buyButtonText.color = canAfford ? Color.black : new Color(1f, 1f, 1f, 0.7f);
    }

    private void RefreshVisuals()
    {
        if (itemData == null)
            return;

        itemIcon.sprite = itemData.Icon;
        itemNameText.text = itemData.ItemName;
        descriptionText.text = itemData.Description;

        bool isMaxLevel = currentLevel >= itemData.MaxLevel;

        if (isMaxLevel)
        {
            levelText.text = "MAX";
            priceText.text = "MAX";
            buyButtonText.text = "MAX";
            buyButton.interactable = false;
            return;
        }

        levelText.text = $"Lv {currentLevel}/{itemData.MaxLevel}";
        priceText.text = $"{itemData.GetUpgradeCost(currentLevel)}";
        buyButtonText.text = "BUY";
        buyButton.interactable = true;
    }

    public void SetDisabled()
    {
        buyButton.interactable = false;
        buyButtonText.text = "MAX";
        levelText.text = "MAX";
        priceText.text = "MAX";
    }
}
