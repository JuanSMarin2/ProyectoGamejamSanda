using System;
using System.Collections.Generic;
using UnityEngine;


public class InventoryData : MonoBehaviour
{
    public static InventoryData Instance { get; private set; }

    [SerializeField] private int maxSlots = 6;
    [SerializeField] private List<Item> items = new();


    public event Action OnInventoryChanged;
    public event Action OnSlotsChanged;


    public int MaxSlots => maxSlots;
    public IReadOnlyList<Item> Items => items;


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


    public bool AddItem(Item item)
    {
        if (item == null || items.Count >= maxSlots)
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


    public Item GetItem(int index)
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
}