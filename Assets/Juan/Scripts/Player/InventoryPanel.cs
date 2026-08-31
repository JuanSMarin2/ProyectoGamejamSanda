using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class InventoryPanel : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;

    [Header("Slots")]
    [SerializeField] private InventoryItemButton itemButtonPrefab;
    [SerializeField] private Transform baseGrid;
    [SerializeField] private Transform smallGrid;
    [SerializeField] private Transform largeGrid;
    [SerializeField] private int baseSlots = 1;
    [SerializeField] private int smallSlots = 3;
    [SerializeField] private int largeSlots = 2;

    [Header("Selected Panel")]
    [SerializeField] private GameObject selectedPanelObject;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text stat1Text;
    [SerializeField] private TMP_Text stat2Text;
    [SerializeField] private TMP_Text stat3Text;
    [SerializeField] private Image itemImage;
    [SerializeField] private Button discardButton;

    [Header("Space Texts")]
    [SerializeField] private TMP_Text baseItemSpaceText;
    [SerializeField] private TMP_Text largeItemSpaceText;
    [SerializeField] private TMP_Text smallItemSpaceText;

    [SerializeField] private PlayerMovement playerMovement;


    private readonly List<InventoryItemButton> baseSlotList = new();
    private readonly List<InventoryItemButton> smallSlotList = new();
    private readonly List<InventoryItemButton> largeSlotList = new();

    private PlayerInputActions inputActions;

    private ObjectData selectedItem;


    private static Dictionary<int, Sprite> spriteCache;


    private void Awake()
    {
        inputActions = new PlayerInputActions();

        EnsureSpriteCacheLoaded();

        if (backButton != null)
            backButton.onClick.AddListener(CloseSelectedPanel);

        if (discardButton != null)
            discardButton.onClick.AddListener(DiscardSelectedItem);

        if (selectedPanelObject != null)
            selectedPanelObject.SetActive(false);

        CreateZoneSlots(baseGrid, baseSlots, InventorySection.Base, baseSlotList);
        CreateZoneSlots(smallGrid, smallSlots, InventorySection.Small, smallSlotList);
        CreateZoneSlots(largeGrid, largeSlots, InventorySection.Large, largeSlotList);
    }


    private void OnEnable()
    {
        inputActions.Enable();
    }


    private void OnDisable()
    {
        inputActions.Disable();
    }


    private void Update()
    {
        UpdateVisual();

        if (inputActions.Player.Inventory.WasPressedThisFrame())
        {
            ToggleInventory();
        }
    }


    private void CreateZoneSlots(
        Transform grid,
        int slotCount,
        InventorySection section,
        List<InventoryItemButton> slotList)
    {
        if (grid == null || itemButtonPrefab == null)
            return;

        for (int i = 0; i < slotCount; i++)
        {
            InventoryItemButton slot = Instantiate(itemButtonPrefab, grid);

            slot.Setup(section, i, this);

            slotList.Add(slot);
        }
    }


    private void ToggleInventory()
    {
        if (AudioManager.instance != null &&
            FMODEvents.instance != null &&
            !FMODEvents.instance.abrirInventario.IsNull)
        {
            AudioManager.instance.PlayOneShot(
                FMODEvents.instance.abrirInventario,
                transform.position
            );
        }

        if (inventoryPanel == null)
        {
            Debug.LogError("[INVENTORY] No hay un panel de inventario asignado.");
            return;
        }

        if (inventoryPanel.activeSelf)
            CloseInventory();
        else
            OpenInventory();
    }


    public void OpenInventory()
    {
        inventoryPanel.SetActive(true);

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(false);
    }


    public void CloseInventory()
    {
        CloseSelectedPanel();

        inventoryPanel.SetActive(false);

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);
    }


    public void OnSlotClicked(InventorySection section, int localIndex)
    {
        ObjectData item = GetSectionItem(section, localIndex);

        if (item == null)
            return;

        selectedItem = item;

        UpdateSelectedPanel(item);

        if (selectedPanelObject != null)
            selectedPanelObject.SetActive(true);
    }


    private ObjectData GetSectionItem(InventorySection section, int localIndex)
    {
        if (InventoryData.Instance == null || localIndex < 0)
            return null;

        switch (section)
        {
            case InventorySection.Base:
                return localIndex == 0 ? InventoryData.Instance.BaseItem : null;
            case InventorySection.Small:
                return localIndex < InventoryData.Instance.SmallItems.Count
                    ? InventoryData.Instance.SmallItems[localIndex]
                    : null;
            case InventorySection.Large:
                return localIndex < InventoryData.Instance.LargeItems.Count
                    ? InventoryData.Instance.LargeItems[localIndex]
                    : null;
            default:
                return null;
        }
    }


    private void UpdateSelectedPanel(ObjectData item)
    {
        if (itemNameText != null)
            itemNameText.text = item.itemName;

        if (stat1Text != null)
            stat1Text.text = $"Elegancia: {(int)item.Elegance}";

        if (stat2Text != null)
            stat2Text.text = $"Robustez: {(int)item.Robustness}";

        if (stat3Text != null)
            stat3Text.text = $"Brillo: {(int)item.Brightness}";

        if (itemImage != null)
            itemImage.sprite = GetSpriteByID(item.id, item.sprite);
    }


    public void CloseSelectedPanel()
    {
        if (selectedPanelObject != null)
            selectedPanelObject.SetActive(false);

        selectedItem = null;
    }


    private void DiscardSelectedItem()
    {
        if (InventoryData.Instance == null)
            return;

        if (selectedItem == null)
        {
            CloseSelectedPanel();
            return;
        }

        InventoryData.Instance.RemoveItem(selectedItem);

        UpdateVisual();

        CloseSelectedPanel();
    }


    private void UpdateVisual()
    {
        if (InventoryData.Instance == null)
            return;

        UpdateZone(baseSlotList, InventorySection.Base);
        UpdateZone(smallSlotList, InventorySection.Small);
        UpdateZone(largeSlotList, InventorySection.Large);

        UpdateSpaceTexts();
    }


    private void UpdateSpaceTexts()
    {
        if (baseItemSpaceText != null)
            baseItemSpaceText.text = GetSpaceText(PieceCategory.Base);

        if (largeItemSpaceText != null)
            largeItemSpaceText.text = GetSpaceText(PieceCategory.LargeAccessory);

        if (smallItemSpaceText != null)
            smallItemSpaceText.text = GetSpaceText(PieceCategory.SmallAccessory);
    }


    private string GetSpaceText(PieceCategory category)
    {
        int current = InventoryData.Instance.CountByCategory(category);
        int limit = InventoryData.GetCategoryLimit(category);

        return $"Espacio {current}/{limit}";
    }


    private void UpdateZone(List<InventoryItemButton> slotList, InventorySection section)
    {
        PieceCategory category = PieceCategory.Base;

        switch (section)
        {
            case InventorySection.Base:
                category = PieceCategory.Base;
                break;
            case InventorySection.Small:
                category = PieceCategory.SmallAccessory;
                break;
            case InventorySection.Large:
                category = PieceCategory.LargeAccessory;
                break;
        }

        int categoryLimit = InventoryData.GetCategoryLimit(category);

        for (int i = 0; i < slotList.Count; i++)
        {
            bool slotVisible = i < categoryLimit;

            slotList[i].gameObject.SetActive(slotVisible);

            if (!slotVisible)
                continue;

            ObjectData item = GetSectionItem(section, i);

            SetSlotSprite(slotList[i], item);
        }
    }


    private void SetSlotSprite(InventoryItemButton slot, ObjectData item)
    {
        if (item != null)
            slot.SetSprite(GetSpriteByID(item.id, item.sprite));
        else
            slot.SetSprite(null);
    }


    private Sprite GetSpriteByID(int id, Sprite fallback)
    {
        EnsureSpriteCacheLoaded();

        if (spriteCache.TryGetValue(id, out Sprite sprite))
            return sprite;

        return fallback;
    }


    private static void EnsureSpriteCacheLoaded()
    {
        if (spriteCache != null)
            return;

        spriteCache = new Dictionary<int, Sprite>();

        ObjectData[] availableItems =
            Resources.LoadAll<ObjectData>("Items");

        foreach (ObjectData item in availableItems)
        {
            if (item == null)
                continue;

            if (!spriteCache.ContainsKey(item.id))
                spriteCache.Add(item.id, item.sprite);
        }
    }
}