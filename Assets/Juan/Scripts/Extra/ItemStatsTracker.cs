using System;
using UnityEngine;

public class ItemStatsTracker : MonoBehaviour
{
    private void Start()
    {
        LogAllItems();
    }

    [ContextMenu("Log All Items")]
    public void LogAllItems()
    {
        ObjectData[] items = Resources.LoadAll<ObjectData>("Items");

        if (items == null || items.Length == 0)
        {
            Debug.LogWarning("[ITEMS] No se encontro ningun ObjectData en Resources/Items.");
            return;
        }

        Array.Sort(items, (a, b) => a.id.CompareTo(b.id));

        Debug.Log($"[ITEMS] Total: {items.Length}");

        foreach (ObjectData item in items)
        {
            if (item == null)
                continue;

            Debug.Log(
                $"[ITEMS] ID {item.id} | {item.itemName} | {item.Category} | " +
                $"Elegancia: {(int)item.Elegance} | " +
                $"Robustez: {(int)item.Robustness} | " +
                $"Brillo: {(int)item.Brightness}"
            );
        }
    }
}
