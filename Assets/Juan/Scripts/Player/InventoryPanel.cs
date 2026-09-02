using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    [Header("Selection Mode")]
    [SerializeField] private GameObject selectionItemText;
    [SerializeField] private Button confirmSelectionButton;
    [SerializeField] private GameObject selectedObjectFeedbackPrefab;

    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private ArtworksPanel artworksPanel;


    private readonly List<InventoryItemButton> baseSlotList = new();
    private readonly List<InventoryItemButton> smallSlotList = new();
    private readonly List<InventoryItemButton> largeSlotList = new();

    private PlayerInputActions inputActions;

    private ObjectData selectedItem;

    private bool selectionMode;
    private string craftSceneName;

    private readonly HashSet<int> selectedBaseIndices = new();
    private readonly HashSet<int> selectedLargeIndices = new();
    private readonly HashSet<int> selectedSmallIndices = new();
    private readonly Dictionary<InventoryItemButton, GameObject> selectionFeedbacks = new();


    private static Dictionary<int, Sprite> spriteCache;


    private void Awake()
    {
        inputActions = new PlayerInputActions();

        if (artworksPanel == null)
            artworksPanel = FindFirstObjectByType<ArtworksPanel>(FindObjectsInactive.Include);

        EnsureSpriteCacheLoaded();

        if (backButton != null)
            backButton.onClick.AddListener(CloseSelectedPanel);

        if (discardButton != null)
            discardButton.onClick.AddListener(DiscardSelectedItem);

        if (confirmSelectionButton != null)
        {
            confirmSelectionButton.onClick.AddListener(ConfirmSelection);
            confirmSelectionButton.gameObject.SetActive(false);
        }

        if (selectionItemText != null)
            selectionItemText.SetActive(false);

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

        DestroyAllFeedbacks();

        selectionMode = false;
        craftSceneName = null;

        selectedBaseIndices.Clear();
        selectedLargeIndices.Clear();
        selectedSmallIndices.Clear();
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
        if (artworksPanel != null)
            artworksPanel.ClosePanel();

        ExitSelectionMode();

        CloseSelectedPanel();

        inventoryPanel.SetActive(false);

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);
    }


    public void EnterSelectionMode(string sceneName)
    {
        selectionMode = true;
        craftSceneName = sceneName;

        if (selectionItemText != null)
            selectionItemText.SetActive(true);

        if (confirmSelectionButton != null)
        {
            confirmSelectionButton.gameObject.SetActive(true);
            confirmSelectionButton.interactable = false;
        }

        OpenInventory();

        UpdateVisual();
    }


    private void ExitSelectionMode()
    {
        if (!selectionMode)
            return;

        selectionMode = false;
        craftSceneName = null;

        selectedBaseIndices.Clear();
        selectedLargeIndices.Clear();
        selectedSmallIndices.Clear();

        DestroyAllFeedbacks();

        if (selectionItemText != null)
            selectionItemText.SetActive(false);

        if (confirmSelectionButton != null)
            confirmSelectionButton.gameObject.SetActive(false);
    }


    private void DestroyAllFeedbacks()
    {
        foreach (KeyValuePair<InventoryItemButton, GameObject> pair in selectionFeedbacks)
        {
            if (pair.Value != null)
                Destroy(pair.Value);
        }

        selectionFeedbacks.Clear();
    }


    public void OnSlotClicked(InventorySection section, int localIndex)
    {
        if (selectionMode)
        {
            ToggleSelection(section, localIndex);
            return;
        }

        ObjectData item = GetSectionItem(section, localIndex);

        if (item == null)
            return;

        selectedItem = item;

        UpdateSelectedPanel(item);

        if (selectedPanelObject != null)
            selectedPanelObject.SetActive(true);
    }


    private void ToggleSelection(InventorySection section, int localIndex)
    {
        ObjectData item = GetSectionItem(section, localIndex);

        if (item == null)
            return;

        HashSet<int> selectedIndices = GetSelectedIndices(section);
        InventoryItemButton slot = GetSlot(section, localIndex);

        if (selectedIndices.Contains(localIndex))
        {
            selectedIndices.Remove(localIndex);

            if (slot != null &&
                selectionFeedbacks.TryGetValue(slot, out GameObject feedback))
            {
                if (feedback != null)
                    Destroy(feedback);

                selectionFeedbacks.Remove(slot);
            }
        }
        else
        {
            if (selectedIndices.Count >= GetRequiredCount(section))
                return;

            selectedIndices.Add(localIndex);

            if (slot != null && selectedObjectFeedbackPrefab != null)
            {
                GameObject feedback = Instantiate(
                    selectedObjectFeedbackPrefab,
                    slot.transform
                );

                feedback.transform.localPosition = Vector3.zero;

                selectionFeedbacks[slot] = feedback;
            }
        }

        UpdateSelectionUI();
    }


    private HashSet<int> GetSelectedIndices(InventorySection section)
    {
        switch (section)
        {
            case InventorySection.Base:
                return selectedBaseIndices;
            case InventorySection.Large:
                return selectedLargeIndices;
            case InventorySection.Small:
                return selectedSmallIndices;
            default:
                return null;
        }
    }


    private InventoryItemButton GetSlot(InventorySection section, int localIndex)
    {
        List<InventoryItemButton> slotList = null;

        switch (section)
        {
            case InventorySection.Base:
                slotList = baseSlotList;
                break;
            case InventorySection.Small:
                slotList = smallSlotList;
                break;
            case InventorySection.Large:
                slotList = largeSlotList;
                break;
        }

        if (slotList == null || localIndex < 0 || localIndex >= slotList.Count)
            return null;

        return slotList[localIndex];
    }


    private int GetRequiredCount(InventorySection section)
    {
        switch (section)
        {
            case InventorySection.Base:
                return SelectedItemsData.RequiredBase;
            case InventorySection.Large:
                return SelectedItemsData.RequiredLarge;
            case InventorySection.Small:
                return SelectedItemsData.RequiredSmall;
            default:
                return 0;
        }
    }


    private void UpdateSelectionUI()
    {
        if (baseItemSpaceText != null)
        {
            baseItemSpaceText.text =
                $"Seleccionados: {selectedBaseIndices.Count}/{SelectedItemsData.RequiredBase}";
        }

        if (largeItemSpaceText != null)
        {
            largeItemSpaceText.text =
                $"Seleccionados: {selectedLargeIndices.Count}/{SelectedItemsData.RequiredLarge}";
        }

        if (smallItemSpaceText != null)
        {
            smallItemSpaceText.text =
                $"Seleccionados: {selectedSmallIndices.Count}/{SelectedItemsData.RequiredSmall}";
        }

        if (confirmSelectionButton != null)
        {
            confirmSelectionButton.interactable =
                selectedBaseIndices.Count >= SelectedItemsData.RequiredBase &&
                selectedLargeIndices.Count >= SelectedItemsData.RequiredLarge &&
                selectedSmallIndices.Count >= SelectedItemsData.RequiredSmall;
        }
    }


    private void ConfirmSelection()
    {
        if (!selectionMode)
            return;

        if (selectedBaseIndices.Count < SelectedItemsData.RequiredBase ||
            selectedLargeIndices.Count < SelectedItemsData.RequiredLarge ||
            selectedSmallIndices.Count < SelectedItemsData.RequiredSmall)
        {
            return;
        }

        List<ObjectData> chosenItems = new List<ObjectData>();

        CollectSelectedItems(InventorySection.Base, selectedBaseIndices, chosenItems);
        CollectSelectedItems(InventorySection.Large, selectedLargeIndices, chosenItems);
        CollectSelectedItems(InventorySection.Small, selectedSmallIndices, chosenItems);

        SelectedItemsData.GetOrCreate().SetSelected(chosenItems);

        string targetScene = craftSceneName;

        ExitSelectionMode();

        inventoryPanel.SetActive(false);

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);

        if (!string.IsNullOrEmpty(targetScene))
            SceneManager.LoadScene(targetScene);
    }


    private void CollectSelectedItems(
        InventorySection section,
        HashSet<int> indices,
        List<ObjectData> target)
    {
        foreach (int index in indices)
        {
            ObjectData item = GetSectionItem(section, index);

            if (item != null)
                target.Add(item);
        }
    }


    private ObjectData GetSectionItem(InventorySection section, int localIndex)
    {
        if (InventoryData.Instance == null || localIndex < 0)
            return null;

        switch (section)
        {
            case InventorySection.Base:
                return localIndex < InventoryData.Instance.BaseItems.Count
                    ? InventoryData.Instance.BaseItems[localIndex]
                    : null;
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
            stat1Text.text =
                $"{GetStatName(item.Elegance, "Elegancia", "Extremadamente Elegante", "Majestuoso")}: {(int)item.Elegance}";

        if (stat2Text != null)
            stat2Text.text =
                $"{GetStatName(item.Robustness, "Robustez", "Extremadamente Robusto", "Indestructible")}: {(int)item.Robustness}";

        if (stat3Text != null)
            stat3Text.text =
                $"{GetStatName(item.Brightness, "Brillo", "Extremadamente Brillante", "Radiante")}: {(int)item.Brightness}";

        if (itemImage != null)
            itemImage.sprite = GetSpriteByID(item.id, item.sprite);
    }

    private string GetStatName(
        FeatureRating rating,
        string normalName,
        string fourStarName,
        string fiveStarName)
    {
        switch (rating)
        {
            case FeatureRating.FourStars:
                return fourStarName;
            case FeatureRating.FiveStars:
                return fiveStarName;
            default:
                return normalName;
        }
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

        if (selectionMode)
            UpdateSelectionUI();
        else
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

        EnsureZoneSlots(section, categoryLimit, slotList);

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


    private void EnsureZoneSlots(
        InventorySection section,
        int neededCount,
        List<InventoryItemButton> slotList)
    {
        Transform grid = GetGridForSection(section);

        if (grid == null || itemButtonPrefab == null)
            return;

        while (slotList.Count < neededCount)
        {
            InventoryItemButton slot = Instantiate(itemButtonPrefab, grid);

            slot.Setup(section, slotList.Count, this);

            slotList.Add(slot);
        }
    }


    private Transform GetGridForSection(InventorySection section)
    {
        switch (section)
        {
            case InventorySection.Base:
                return baseGrid;
            case InventorySection.Small:
                return smallGrid;
            case InventorySection.Large:
                return largeGrid;
            default:
                return null;
        }
    }


    private void SetSlotSprite(InventoryItemButton slot, ObjectData item)
    {
        if (item != null)
        {
            slot.SetSprite(GetSpriteByID(item.id, item.sprite));
            slot.SetDescription(ItemDescriptionBuilder.BuildDescription(item));
        }
        else
        {
            slot.SetSprite(null);
            slot.SetDescription(null);
        }
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