using UnityEngine;

public class ItemPickable : MonoBehaviour
{
    [SerializeField] private int itemId;
    [SerializeField] private Item fallbackItem;
    [SerializeField] private SpriteRenderer spriteRenderer;




    private Item item;

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
 Item[] items = Resources.LoadAll<Item>("Items");

        item = fallbackItem;

        foreach (Item currentItem in items)
        {
            if (currentItem.id == itemId)
            {
                item = currentItem;
                break;
            }
        }
    }






    private void UpdateSprite()
    {
        if (spriteRenderer == null || item == null)
            return;

        spriteRenderer.sprite = item.sprite;
    }
}