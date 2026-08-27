using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PrefabItemEntry
{
    public int itemId;
    public GameObject prefab;
}

public class PrefabItemSetup : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] private List<PrefabItemEntry> basePrefabs = new();
    [SerializeField] private GameObject defaultBasePrefab;

    [Header("Large Accessories")]
    [SerializeField] private List<PrefabItemEntry> largePrefabs = new();
    [SerializeField] private GameObject defaultLargePrefab;

    [Header("Small Accessories")]
    [SerializeField] private List<PrefabItemEntry> smallPrefabs = new();
    [SerializeField] private GameObject defaultSmallPrefab;

    public GameObject GetPrefab(PieceCategory category, int id)
    {
        List<PrefabItemEntry> entries = GetEntries(category);

        foreach (PrefabItemEntry entry in entries)
        {
            if (entry != null && entry.itemId == id && entry.prefab != null)
                return entry.prefab;
        }

        GameObject defaultPrefab = GetDefaultPrefab(category);

        if (defaultPrefab != null)
        {
            Debug.LogWarning($"[PREFAB SETUP] No hay prefab para el id {id} ({category}). Usando el prefab por defecto.");
            return defaultPrefab;
        }

        Debug.LogWarning($"[PREFAB SETUP] No hay prefab para el id {id} ({category}) y no hay prefab por defecto asignado.");
        return null;
    }

    private List<PrefabItemEntry> GetEntries(PieceCategory category)
    {
        switch (category)
        {
            case PieceCategory.Base:
                return basePrefabs;
            case PieceCategory.LargeAccessory:
                return largePrefabs;
            case PieceCategory.SmallAccessory:
                return smallPrefabs;
            default:
                return basePrefabs;
        }
    }

    private GameObject GetDefaultPrefab(PieceCategory category)
    {
        switch (category)
        {
            case PieceCategory.Base:
                return defaultBasePrefab;
            case PieceCategory.LargeAccessory:
                return defaultLargePrefab;
            case PieceCategory.SmallAccessory:
                return defaultSmallPrefab;
            default:
                return null;
        }
    }
}