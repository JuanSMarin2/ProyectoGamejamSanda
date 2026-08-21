using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryVisual : MonoBehaviour
{
    [SerializeField] private List<Image> itemImages = new();
    [SerializeField] private List<TMP_Text> itemNames = new();


    private void Update()
    {
        UpdateVisual();
    }


    private void UpdateVisual()
    {
        if (InventoryData.Instance == null)
            return;


        for (int i = 0; i < itemImages.Count; i++)
        {
            bool hasItem = i < InventoryData.Instance.Items.Count;

            itemImages[i].gameObject.SetActive(hasItem);

            if (hasItem)
            {
                itemImages[i].sprite = InventoryData.Instance.Items[i].sprite;
            }
        }


        for (int i = 0; i < itemNames.Count; i++)
        {
            bool hasItem = i < InventoryData.Instance.Items.Count;

            itemNames[i].gameObject.SetActive(hasItem);

            if (hasItem)
            {
                itemNames[i].text = InventoryData.Instance.Items[i].itemName;
            }
        }
    }
}