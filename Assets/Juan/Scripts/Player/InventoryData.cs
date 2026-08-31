using System;
using System.Collections.Generic;
using UnityEngine;


public class InventoryData : MonoBehaviour
{


    private const int MaxBaseItems = 1;
    private const int MaxLargeAccessories = 2;
    private const int MaxSmallAccessories = 3;

    public static InventoryData Instance { get; private set; }

    [SerializeField] private int maxSlots = 6;

    public event Action OnInventoryChanged;
    public event Action OnSlotsChanged;


    public int MaxSlots => maxSlots;

    public ObjectData BaseItem => baseItem;
    public IReadOnlyList<ObjectData> SmallItems => smallItems;
    public IReadOnlyList<ObjectData> LargeItems => largeItems;
    public IReadOnlyList<ObjectData> Items => combinedItems;
    public int ItemCount => combinedItems.Count;


    private ObjectData baseItem;
    private readonly List<ObjectData> smallItems = new();
    private readonly List<ObjectData> largeItems = new();
    private readonly List<ObjectData> combinedItems = new();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        maxSlots = Mathf.Clamp(maxSlots, 6, 9);
    }


    public static int GetCategoryLimit(PieceCategory category)
    {
        switch (category)
        {
            case PieceCategory.Base:
                return MaxBaseItems;
            case PieceCategory.LargeAccessory:
                return MaxLargeAccessories;
            case PieceCategory.SmallAccessory:
                return MaxSmallAccessories;
            default:
                return 0;
        }
    }


    public bool AddItem(ObjectData item)
    {
        if (item == null || IsFull() || !CanAddCategory(item.Category))
            return false;

        switch (item.Category)
        {
            case PieceCategory.Base:
                baseItem = item;
                break;
            case PieceCategory.SmallAccessory:
                smallItems.Add(item);
                break;
            case PieceCategory.LargeAccessory:
                largeItems.Add(item);
                break;
            default:
                return false;
        }

        RebuildCombinedItems();
        OnInventoryChanged?.Invoke();

        return true;
    }


    public bool RemoveItem(int index)
    {
        if (index < 0 || index >= combinedItems.Count)
            return false;

        return RemoveItem(combinedItems[index]);
    }


    public bool RemoveItem(ObjectData item)
    {
        if (item == null)
            return false;

        bool removed = false;

        if (baseItem == item)
        {
            baseItem = null;
            removed = true;
        }
        else if (smallItems.Remove(item))
        {
            removed = true;
        }
        else if (largeItems.Remove(item))
        {
            removed = true;
        }

        if (!removed)
            return false;

        RebuildCombinedItems();
        OnInventoryChanged?.Invoke();

        return true;
    }


    public void ClearInventory()
    {
        if (ItemCount == 0)
            return;

        baseItem = null;
        smallItems.Clear();
        largeItems.Clear();

        RebuildCombinedItems();
        OnInventoryChanged?.Invoke();
    }


    public ObjectData GetItem(int index)
    {
        if (index < 0 || index >= combinedItems.Count)
            return null;

        return combinedItems[index];
    }


    public void BuySlot()
    {
        if (maxSlots >= 9)
            return;

        maxSlots++;

        OnSlotsChanged?.Invoke();
        OnInventoryChanged?.Invoke();
    }


    public bool IsFull()
    {
        return ItemCount >= maxSlots;
    }


    public int CountByCategory(PieceCategory category)
    {
        switch (category)
        {
            case PieceCategory.Base:
                return baseItem != null ? 1 : 0;
            case PieceCategory.LargeAccessory:
                return largeItems.Count;
            case PieceCategory.SmallAccessory:
                return smallItems.Count;
            default:
                return 0;
        }
    }


    private bool CanAddCategory(PieceCategory category)
    {
        return CountByCategory(category) < GetCategoryLimit(category);
    }


    private void RebuildCombinedItems()
    {
        combinedItems.Clear();

        if (baseItem != null)
            combinedItems.Add(baseItem);

        combinedItems.AddRange(smallItems);
        combinedItems.AddRange(largeItems);
    }
}