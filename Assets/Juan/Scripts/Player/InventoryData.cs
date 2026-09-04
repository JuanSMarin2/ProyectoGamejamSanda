using System;
using System.Collections.Generic;
using UnityEngine;


public class InventoryData : MonoBehaviour
{


    private const int InitialBaseLimit = 2;
    private const int InitialLargeLimit = 3;
    private const int InitialSmallLimit = 4;

    private const int MaxBaseLimit = 5;
    private const int MaxLargeLimit = 5;
    private const int MaxSmallLimit = 5;

    public static InventoryData Instance { get; private set; }

    [SerializeField] private int maxBaseItems = InitialBaseLimit;
    [SerializeField] private int maxLargeItems = InitialLargeLimit;
    [SerializeField] private int maxSmallItems = InitialSmallLimit;

    public event Action OnInventoryChanged;
    public event Action OnSlotsChanged;


    public int MaxSlots => maxBaseItems + maxLargeItems + maxSmallItems;


    public ObjectData BaseItem => baseItems.Count > 0 ? baseItems[0] : null;
    public IReadOnlyList<ObjectData> BaseItems => baseItems;
    public IReadOnlyList<ObjectData> SmallItems => smallItems;
    public IReadOnlyList<ObjectData> LargeItems => largeItems;
    public IReadOnlyList<ObjectData> Items => combinedItems;
    public int ItemCount => combinedItems.Count;


    private readonly List<ObjectData> baseItems = new();
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

        maxBaseItems = Mathf.Clamp(maxBaseItems, InitialBaseLimit, MaxBaseLimit);
        maxLargeItems = Mathf.Clamp(maxLargeItems, InitialLargeLimit, MaxLargeLimit);
        maxSmallItems = Mathf.Clamp(maxSmallItems, InitialSmallLimit, MaxSmallLimit);
    }


    public static int GetCategoryLimit(PieceCategory category)
    {
        if (Instance == null)
        {
            switch (category)
            {
                case PieceCategory.Base:
                    return InitialBaseLimit;
                case PieceCategory.LargeAccessory:
                    return InitialLargeLimit;
                case PieceCategory.SmallAccessory:
                    return InitialSmallLimit;
                default:
                    return 0;
            }
        }

        switch (category)
        {
            case PieceCategory.Base:
                return Instance.maxBaseItems;
            case PieceCategory.LargeAccessory:
                return Instance.maxLargeItems;
            case PieceCategory.SmallAccessory:
                return Instance.maxSmallItems;
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
                baseItems.Add(item);
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

        if (baseItems.Remove(item))
        {
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

        PlayRemoveSound();

        RebuildCombinedItems();
        OnInventoryChanged?.Invoke();

        return true;
    }


    private void PlayRemoveSound()
    {
        if (AudioManager.instance == null ||
            FMODEvents.instance == null ||
            FMODEvents.instance.agarrarObjeto.IsNull)
        {
            return;
        }

        AudioManager.instance.PlayOneShot(
            FMODEvents.instance.agarrarObjeto,
            transform.position
        );
    }


    public void ClearInventory()
    {
        if (ItemCount == 0)
            return;
        baseItems.Clear();
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


    public bool CanIncreaseInventory()
    {
        return maxBaseItems < MaxBaseLimit ||
               maxLargeItems < MaxLargeLimit ||
               maxSmallItems < MaxSmallLimit;
    }


    public void IncreaseInventory()
    {
        if (maxBaseItems >= MaxBaseLimit &&
            maxLargeItems >= MaxLargeLimit &&
            maxSmallItems >= MaxSmallLimit)
            return;

        maxBaseItems = Mathf.Min(maxBaseItems + 1, MaxBaseLimit);
        maxLargeItems = Mathf.Min(maxLargeItems + 1, MaxLargeLimit);
        maxSmallItems = Mathf.Min(maxSmallItems + 1, MaxSmallLimit);

        OnSlotsChanged?.Invoke();
        OnInventoryChanged?.Invoke();
    }


    public bool IsFull()
    {
        return ItemCount >= MaxSlots;
    }


    public int CountByCategory(PieceCategory category)
    {
        switch (category)
        {
            case PieceCategory.Base:
                return baseItems.Count;
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

        combinedItems.AddRange(baseItems);
        combinedItems.AddRange(smallItems);
        combinedItems.AddRange(largeItems);
    }
}