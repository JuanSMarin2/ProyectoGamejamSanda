using System.Collections.Generic;
using UnityEngine;

public class ItemPickable : MonoBehaviour
{
    [SerializeField] private int itemId;
    [SerializeField] private ObjectData fallbackItem;
    [SerializeField] private SpriteRenderer spriteRenderer;


    private static Dictionary<int, ObjectData> itemCache;

    private ObjectData item;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();


        LoadItem();
        UpdateSprite();
    }

    private void OnValidate()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        LoadItem();
        UpdateSprite();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (InventoryData.Instance == null || item == null)
            return;


        if (!InventoryData.Instance.AddItem(item))
            return;

        Destroy(gameObject);
    }


    private void LoadItem()
    {
        EnsureCacheLoaded();

        item = fallbackItem;

        if (itemCache.TryGetValue(itemId, out ObjectData cachedItem))
            item = cachedItem;
    }


    private void UpdateSprite()
    {
        if (spriteRenderer == null || item == null)
            return;

        spriteRenderer.sprite = item.sprite;
    }


    private static void EnsureCacheLoaded()
    {
        if (itemCache != null)
            return;

        itemCache = new Dictionary<int, ObjectData>();

        ObjectData[] items = Resources.LoadAll<ObjectData>("Items");

        foreach (ObjectData currentItem in items)
        {
            if (currentItem == null)
                continue;

            if (!itemCache.ContainsKey(currentItem.id))
                itemCache.Add(currentItem.id, currentItem);
        }
    }
}