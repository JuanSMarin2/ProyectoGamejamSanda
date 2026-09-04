using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopItemData", menuName = "Game/Shop/Shop Item Data")]
public class ShopItemData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string itemId;
    [SerializeField] private string itemName = "New item";
    [TextArea(2, 4)]
    [SerializeField] private string description = "Description of the item.";
    [SerializeField] private Sprite icon;

    [Header("Economy and progression")]
    [SerializeField] private int basePrice = 10;
    [SerializeField] private int maxLevel = 1;
    [SerializeField] private int[] priceByLevel;

    public string ItemId => itemId;
    public string ItemName => itemName;
    public string Description => description;
    public Sprite Icon => icon;
    public int BasePrice => basePrice;
    public int MaxLevel => maxLevel;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            itemId = Guid.NewGuid().ToString();
        }

        maxLevel = Mathf.Max(1, maxLevel);

        if (priceByLevel == null || priceByLevel.Length == 0)
        {
            priceByLevel = new int[maxLevel];
            for (int i = 0; i < maxLevel; i++)
            {
                priceByLevel[i] = basePrice;
            }
        }

        if (priceByLevel.Length != maxLevel)
        {
            int[] adjusted = new int[maxLevel];

            for (int i = 0; i < maxLevel; i++)
            {
                if (i < priceByLevel.Length)
                {
                    adjusted[i] = Mathf.Max(1, priceByLevel[i]);
                }
                else
                {
                    adjusted[i] = basePrice;
                }
            }

            priceByLevel = adjusted;
        }
    }

    public int GetUpgradeCost(int currentLevel)
    {
        if (currentLevel < 0)
            currentLevel = 0;

        if (currentLevel >= maxLevel)
            return 0;

        int index = Mathf.Clamp(currentLevel, 0, priceByLevel.Length - 1);
        return Mathf.Max(1, priceByLevel[index]);
    }

    public bool HasMoreLevels(int currentLevel)
    {
        return currentLevel < maxLevel;
    }
}
