using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[System.Serializable]
public class TrashItemChance
{
    public int itemId;

    [Range(0f, 100f)]
    public float probability;
}


public class TrashCan : MonoBehaviour
{
    [FormerlySerializedAs("items")]
    [SerializeField] private List<TrashItemChance> baseItems = new();
    [SerializeField] private List<TrashItemChance> largeAccessoryItems = new();
    [SerializeField] private List<TrashItemChance> smallAccessoryItems = new();

    [SerializeField]
    [Range(0f, 100f)]
    private float coinProbability = 50f;

    [SerializeField] private InteractableObject interactable;

    [SerializeField] private float restockInterval = 60f;

    [SerializeField] private int maxItemsPerInteract = 3;


    private static Dictionary<int, ObjectData> itemCache;

    private bool itemsCollected = false;
    private bool coinsCollected = false;
    private float restockTimer;


    private readonly HashSet<int> consumedBaseEntries = new();
    private readonly HashSet<int> consumedLargeEntries = new();
    private readonly HashSet<int> consumedSmallEntries = new();


    private void Awake()
    {
        if (interactable == null)
            interactable = GetComponent<InteractableObject>();
    }


    private void Start()
    {
        UpdateNewIndicator();
    }


    private void Update()
    {
        restockTimer += Time.deltaTime;

        if (restockTimer < restockInterval)
            return;

        restockTimer = 0f;

        Restock();
    }


    private void Restock()
    {
        bool hadConsumedStock =
            consumedBaseEntries.Count > 0 ||
            consumedLargeEntries.Count > 0 ||
            consumedSmallEntries.Count > 0 ||
            coinsCollected;

        if (!hadConsumedStock)
            return;

        consumedBaseEntries.Clear();
        consumedLargeEntries.Clear();
        consumedSmallEntries.Clear();

        itemsCollected = false;
        coinsCollected = false;

        UpdateNewIndicator();

        Debug.Log($"[TRASH] '{name}' ha restaurado su stock.");
    }


    public void Interact()
    {
        if (itemsCollected){
            PlayTrashSound(FMODEvents.instance != null ? FMODEvents.instance.cerrarCaneca : default);
            return;
        }

        PlayTrashSound(FMODEvents.instance != null ? FMODEvents.instance.abrirCaneca : default);

        if (InventoryData.Instance == null)
            return;


        int coinsGained = 0;

        if (!coinsCollected)
        {

            coinsCollected = true;
            coinsGained = RollCoins();
        }


        List<ObjectData> obtainedItems = new();

        if (PoliceOfficer.IsPlayerInVision)
        {
            PoliceOfficer.ReportCrime();

            Debug.Log("[TRASH] Un policía te ha visto rebuscando: viene a por ti.");
        }

        obtainedItems = TryGiveItems();

        itemsCollected = !CanGiveAnything();


        UpdateNewIndicator();


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


    private int RollCoins()
    {
        if (MoneyData.Instance == null)
            return 0;

        int coinsGained = 0;


        MoneyData.Instance.AddMoney(1);
        coinsGained++;


        while (Random.Range(0f, 100f) < coinProbability)
        {
            MoneyData.Instance.AddMoney(1);
            coinsGained++;
        }

        return coinsGained;
    }


    private List<ObjectData> TryGiveItems()
    {
        List<ObjectData> obtainedItems = new();

        TryGiveCategory(
            PieceCategory.Base,
            baseItems,
            consumedBaseEntries,
            obtainedItems
        );

        TryGiveCategory(
            PieceCategory.LargeAccessory,
            largeAccessoryItems,
            consumedLargeEntries,
            obtainedItems
        );

        TryGiveCategory(
            PieceCategory.SmallAccessory,
            smallAccessoryItems,
            consumedSmallEntries,
            obtainedItems
        );

        return obtainedItems;
    }


    private void TryGiveCategory(
        PieceCategory category,
        List<TrashItemChance> pool,
        HashSet<int> consumedEntries,
        List<ObjectData> obtainedItems)
    {


        if (obtainedItems.Count >= maxItemsPerInteract)
            return;

        if (InventoryData.Instance.CountByCategory(category) >=
            InventoryData.GetCategoryLimit(category))
        {
            return;
        }

        for (int i = 0; i < pool.Count; i++)
        {

            if (obtainedItems.Count >= maxItemsPerInteract)
                return;

            if (consumedEntries.Contains(i))
                continue;

            if (InventoryData.Instance.IsFull() ||
                InventoryData.Instance.CountByCategory(category) >=
                InventoryData.GetCategoryLimit(category))
            {
                return;
            }

            TrashItemChance itemChance = pool[i];


            if (Random.Range(0f, 100f) >= itemChance.probability)
                continue;

            ObjectData item = GetItemByID(itemChance.itemId);

            if (item == null)
            {


                consumedEntries.Add(i);

                Debug.LogWarning(
                    $"[TRASH] No existe ningún ObjectData con id " +
                    $"{itemChance.itemId} en Resources/Items. " +
                    $"Revisa la loot table de '{name}'."
                );

                continue;
            }

            if (item.Category != category)
            {


                consumedEntries.Add(i);

                Debug.LogWarning(
                    $"[TRASH] El ítem '{item.itemName}' (id {item.id}) " +
                    $"es de categoría {item.Category} pero está en la " +
                    $"lista de {category} de '{name}'. Corrígelo en el " +
                    $"asset o en la loot table."
                );

                continue;
            }

            if (InventoryData.Instance.AddItem(item))
            {
                consumedEntries.Add(i);
                obtainedItems.Add(item);
            }
        }
    }


    private bool CanGiveAnything()
    {
        if (InventoryData.Instance == null ||
            InventoryData.Instance.IsFull())
        {
            return false;
        }

        return CanGiveCategory(PieceCategory.Base, baseItems, consumedBaseEntries) ||
               CanGiveCategory(PieceCategory.LargeAccessory, largeAccessoryItems, consumedLargeEntries) ||
               CanGiveCategory(PieceCategory.SmallAccessory, smallAccessoryItems, consumedSmallEntries);
    }


    private bool CanGiveCategory(
        PieceCategory category,
        List<TrashItemChance> pool,
        HashSet<int> consumedEntries)
    {
        if (InventoryData.Instance.CountByCategory(category) >=
            InventoryData.GetCategoryLimit(category))
        {
            return false;
        }


        for (int i = 0; i < pool.Count; i++)
        {
            if (!consumedEntries.Contains(i))
                return true;
        }

        return false;
    }


    private ObjectData GetItemByID(int id)
    {
        EnsureCacheLoaded();

        return itemCache.TryGetValue(id, out ObjectData item)
            ? item
            : null;
    }


    private void PlayTrashSound(FMODUnity.EventReference eventReference)
    {
        if (AudioManager.instance == null ||
            FMODEvents.instance == null ||
            eventReference.IsNull)
        {
            return;
        }

        AudioManager.instance.PlayOneShot(eventReference, transform.position);
    }


    private void UpdateNewIndicator()
    {
        if (interactable == null)
            return;

        interactable.SetNewIndicatorEnabled(
            !itemsCollected && CanGiveAnything()
        );
    }


    private static void EnsureCacheLoaded()
    {
        if (itemCache != null)
            return;

        itemCache = new Dictionary<int, ObjectData>();

        ObjectData[] availableItems =
            Resources.LoadAll<ObjectData>("Items");

        foreach (ObjectData item in availableItems)
        {
            if (item == null)
                continue;

            if (!itemCache.ContainsKey(item.id))
                itemCache.Add(item.id, item);
        }
    }
}