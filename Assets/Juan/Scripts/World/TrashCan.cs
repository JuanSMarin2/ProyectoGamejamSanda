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


    private bool alreadyUsed = false;


    public void Interact()
    {
        if (alreadyUsed)
            return;


        alreadyUsed = true;


        List<Item> obtainedItems = TryGiveItems();

        int coinsGained = 0;


        while (Random.Range(0f, 100f) < coinProbability)
        {
            TryGiveCoin();
            coinsGained++;
        }


        if (TrashFeedback.Instance != null && (obtainedItems.Count > 0 || coinsGained > 0))
        {
            TrashFeedback.Instance.ShowFeedback(obtainedItems, coinsGained);
        }
    }


    private List<Item> TryGiveItems()
    {
        List<Item> obtainedItems = new();


        foreach (TrashItemChance itemChance in items)
        {
            if (obtainedItems.Count >= 3)
                break;


            if (Random.Range(0f, 100f) < itemChance.probability)
            {
                Item item = GetItemByID(itemChance.itemId);


                if (item != null && InventoryData.Instance.AddItem(item))
                {
                    obtainedItems.Add(item);
                }
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
            {
                return item;
            }
        }


        return null;
    }


    private void TryGiveCoin()
    {
        MoneyData.Instance.AddMoney(1);
    }
}