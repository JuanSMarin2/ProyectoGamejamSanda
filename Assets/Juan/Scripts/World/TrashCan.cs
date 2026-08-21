using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class TrashItemChance
{
    public int itemId;
    [Range(0f, 100f)] public float probability;
}


public class TrashCan : MonoBehaviour
{
    [SerializeField] private List<TrashItemChance> items = new();

    [SerializeField] private float coinProbability = 50f;

    private bool alreadyUsed = false;


    public void Interact()
    {
        if (alreadyUsed)
            return;

        alreadyUsed = true;

        TryGiveItems();


        while (Random.Range(0f, 100f) < coinProbability)
        {
            TryGiveCoin();
        }
    }


    private void TryGiveItems()
    {
        foreach (TrashItemChance itemChance in items)
        {
            if (Random.Range(0f, 100f) < itemChance.probability)
            {
                Item[] availableItems = Resources.LoadAll<Item>("Items");

                foreach (Item item in availableItems)
                {
                    if (item.id == itemChance.itemId)
                    {
                        InventoryData.Instance.AddItem(item);
                        break;
                    }
                }
            }
        }
    }


    private void TryGiveCoin()
    {
        MoneyData.Instance.AddMoney(1);
    }
}