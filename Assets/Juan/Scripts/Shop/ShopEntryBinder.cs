using UnityEngine;

public class ShopEntryBinder : MonoBehaviour
{
    [SerializeField] private ShopItemData itemData;
    [SerializeField] private ShopItemUI itemUI;

    private void Start()
    {
        itemUI = GetComponent<ShopItemUI> ();
        if (itemData == null)
        {
            Debug.LogWarning($"[{nameof(ShopEntryBinder)}] No se ha asignado un ShopItemData en {name}.");
            return;
        }

        if (itemUI == null)
        {
            Debug.LogWarning($"[{nameof(ShopEntryBinder)}] No se ha asignado un ShopItemUI en {name}.");
            return;
        }

        if (ShopManager.Instance == null)
        {
            Debug.LogWarning($"[{nameof(ShopEntryBinder)}] No existe un ShopManager en la escena. ");
            return;
        }

        ShopManager.Instance.RegisterItem(itemUI, itemData);
    }
}
