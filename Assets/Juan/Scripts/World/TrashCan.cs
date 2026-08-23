using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class TrashItemChance
{
    public int itemId;

    [Range(0f, 100f)]
    public float probability;
}


public class TrashCan : MonoBehaviour
{
    [SerializeField] private List<TrashItemChance> items = new();

    [SerializeField]
    [Range(0f, 100f)]
    private float coinProbability = 50f;


    private bool itemsCollected = false;
    private bool coinsCollected = false;


    public void Interact()
    {
        if (itemsCollected)
            return;

        if (InventoryData.Instance == null)
            return;


        List<Item> obtainedItems = new();
        int coinsGained = 0;


        if (!coinsCollected)
        {
            TryGiveCoin();
            coinsGained++;

            while (Random.Range(0f, 100f) < coinProbability)
            {
                TryGiveCoin();
                coinsGained++;
            }

            coinsCollected = true;
        }


        if (!InventoryData.Instance.IsFull())
        {
            obtainedItems = TryGiveItems();

            if (obtainedItems.Count > 0)
                itemsCollected = true;
        }


        if (TrashFeedback.Instance != null)
        {
            bool inventoryFull =
                InventoryData.Instance.IsFull() &&
                obtainedItems.Count == 0;

            TrashFeedback.Instance.ShowFeedback(
                obtainedItems,
                coinsGained,
                inventoryFull
            );
        }
    }


    private List<Item> TryGiveItems()
    {
        List<Item> obtainedItems = new();


        foreach (TrashItemChance itemChance in items)
        {
            if (obtainedItems.Count >= 3)
                break;

            if (InventoryData.Instance.IsFull())
                break;


            if (Random.Range(0f, 100f) < itemChance.probability)
            {
                Item item = GetItemByID(itemChance.itemId);


                if (item != null && InventoryData.Instance.AddItem(item))
                {
                    obtainedItems.Add(item);
                }


                if (InventoryData.Instance.IsFull())
                    break;
            }
        }


        return obtainedItems;
    }


    private Item GetItemByID(int id)
    {
        Item[] availableItems = Resources.LoadAll<Item>("Items");


        foreach (Item item in availableItems)
        {
            if (item.id == id)
                return item;
        }


        return null;
    }


    private void TryGiveCoin()
    {
        if (MoneyData.Instance != null)
            MoneyData.Instance.AddMoney(1);
    }
}