using System.Collections.Generic;
using UnityEngine;

public class SelectedItemsData : MonoBehaviour
{
    public const int RequiredBase = 1;
    public const int RequiredLarge = 2;
    public const int RequiredSmall = 3;

    public static SelectedItemsData Instance { get; private set; }

    private readonly List<ObjectData> selectedItems = new();

    public IReadOnlyList<ObjectData> SelectedItems => selectedItems;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static SelectedItemsData GetOrCreate()
    {
        if (Instance == null)
        {
            GameObject dataObject = new GameObject("SelectedItemsData");
            dataObject.AddComponent<SelectedItemsData>();
        }

        return Instance;
    }

    public void SetSelected(IEnumerable<ObjectData> items)
    {
        selectedItems.Clear();

        foreach (ObjectData item in items)
        {
            if (item != null)
                selectedItems.Add(item);
        }
    }

    public void Clear()
    {
        selectedItems.Clear();
    }

    public void ConsumeSelectedFromInventory()
    {
        if (InventoryData.Instance != null)
        {
            foreach (ObjectData item in selectedItems)
                InventoryData.Instance.RemoveItem(item);
        }

        selectedItems.Clear();
    }
}
