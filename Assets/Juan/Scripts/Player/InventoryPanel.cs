using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public class InventoryPanel : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;

    [SerializeField] private List<Image> itemImages = new();
    [SerializeField] private List<Button> itemButtons = new();
    [SerializeField] private List<Button> discardButtons = new();

    [SerializeField] private PlayerMovement playerMovement;


    private PlayerInputActions inputActions;

    private int selectedIndex = -1;


    private static Dictionary<int, Sprite> spriteCache;


    private void Awake()
    {
        inputActions = new PlayerInputActions();

        EnsureSpriteCacheLoaded();

        for (int i = 0; i < itemButtons.Count; i++)
        {
            int index = i;

            itemButtons[i].onClick.AddListener(() => SelectItem(index));
        }


        for (int i = 0; i < discardButtons.Count; i++)
        {
            int index = i;

            discardButtons[i].onClick.AddListener(() => DiscardItem(index));

            discardButtons[i].gameObject.SetActive(false);
        }
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


        if (selectedIndex != -1 &&
            selectedIndex < itemButtons.Count &&
            selectedIndex < discardButtons.Count &&
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            bool clickedItem = RectTransformUtility.RectangleContainsScreenPoint(
                itemButtons[selectedIndex].GetComponent<RectTransform>(),
                mousePosition
            );

            bool clickedDiscard = RectTransformUtility.RectangleContainsScreenPoint(
                discardButtons[selectedIndex].GetComponent<RectTransform>(),
                mousePosition
            );


            if (!clickedItem && !clickedDiscard)
            {
                CloseDiscardButton();
            }
        }
    }


    private void ToggleInventory()
    {
        AudioManager.instance.PlayOneShot(FMODEvents.instance.abrirInventario, transform.position);
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
        CloseDiscardButton();

        inventoryPanel.SetActive(false);

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);
    }


    private void SelectItem(int index)
    {
        if (InventoryData.Instance == null)
            return;

        if (index >= InventoryData.Instance.Items.Count)
            return;

        if (index >= discardButtons.Count)
            return;


        if (selectedIndex == index)
        {
            CloseDiscardButton();
            return;
        }


        CloseDiscardButton();

        selectedIndex = index;

        discardButtons[index].gameObject.SetActive(true);
    }


    private void DiscardItem(int index)
    {
        if (InventoryData.Instance == null)
            return;

        InventoryData.Instance.RemoveItem(index);

        CloseDiscardButton();
    }


    private void CloseDiscardButton()
    {
        if (selectedIndex == -1)
            return;


        if (selectedIndex < discardButtons.Count)
            discardButtons[selectedIndex].gameObject.SetActive(false);


        selectedIndex = -1;
    }


    private void UpdateVisual()
    {
        if (InventoryData.Instance == null)
            return;


        for (int i = 0; i < itemButtons.Count; i++)
        {
            bool slotBought = i < InventoryData.Instance.MaxSlots;
            bool hasItem = i < InventoryData.Instance.Items.Count;


            itemButtons[i].gameObject.SetActive(slotBought);


            if (slotBought && hasItem)
            {
                ObjectData item = InventoryData.Instance.Items[i];

                itemImages[i].gameObject.SetActive(true);
                itemImages[i].sprite = GetSpriteByID(item.id, item.sprite);
            }
            else
            {
                itemImages[i].gameObject.SetActive(false);
                itemImages[i].sprite = null;
            }


            if (!slotBought && i < discardButtons.Count)
            {
                discardButtons[i].gameObject.SetActive(false);
            }
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