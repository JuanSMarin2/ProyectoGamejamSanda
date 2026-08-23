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
    [SerializeField] private List<ObjectData> items = new();


    public event Action OnInventoryChanged;
    public event Action OnSlotsChanged;


    public int MaxSlots => maxSlots;
    public IReadOnlyList<ObjectData> Items => items;


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
        if (item == null || items.Count >= maxSlots || !CanAddCategory(item.Category))
            return false;

        items.Add(item);

        OnInventoryChanged?.Invoke();

        return true;
    }


    public bool RemoveItem(int index)
    {
        if (index < 0 || index >= items.Count)
            return false;

        items.RemoveAt(index);

        OnInventoryChanged?.Invoke();

        return true;
    }


    public ObjectData GetItem(int index)
    {
        if (index < 0 || index >= items.Count)
            return null;

        return items[index];
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
        return items.Count >= maxSlots;
    }


    public int CountByCategory(PieceCategory category)
    {
        int count = 0;

        foreach (ObjectData item in items)
        {
            if (item != null && item.Category == category)
                count++;
        }

        return count;
    }


    private bool CanAddCategory(PieceCategory category)
    {
        return CountByCategory(category) < GetCategoryLimit(category);
    }
}