using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class ShopDialogue : DialogueManager
{
    [Header("Shop")]
    [SerializeField] private int decisionDialogueIndex;
    [SerializeField] private int price;
    [SerializeField] private bool isItemSeller;
    [SerializeField] private ItemSeller itemSeller;
    [SerializeField] private InventoryIncreaseSeller inventoryIncreaseSeller;
    [SerializeField] private GameObject decisionButtons;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button rejectButton;
    [SerializeField] private UnityEvent onBuy;

    private bool waitingDecision;
    private bool purchaseCompleted;
    private bool sellerSoldOut;

    protected override int EndLimit =>
        purchaseCompleted ? dialogues.Count - 2 : dialogues.Count;


    protected override void Awake()
    {
        base.Awake();

        if (itemSeller == null)
            itemSeller = GetComponent<ItemSeller>();

        if (inventoryIncreaseSeller == null)
            inventoryIncreaseSeller = GetComponent<InventoryIncreaseSeller>();

        if (acceptButton != null)
            acceptButton.onClick.AddListener(AcceptOffer);

        if (rejectButton != null)
            rejectButton.onClick.AddListener(RejectOffer);

        if (decisionButtons != null)
            decisionButtons.SetActive(false);
    }


    public override void StartDialogue()
    {
        if (dialogueActive)
            return;

        purchaseCompleted = false;
        waitingDecision = false;

        sellerSoldOut =
            isItemSeller &&
            ((itemSeller != null && itemSeller.HasBoughtToday()) ||
             (inventoryIncreaseSeller != null && inventoryIncreaseSeller.HasBoughtToday()));

        HideButtons();

        base.StartDialogue();
    }


    public override void NextDialogue()
    {
        if (waitingDecision)
            return;

        if (sellerSoldOut && currentIndex < 0)
            currentIndex = dialogues.Count - 2;

        base.NextDialogue();

        if (!sellerSoldOut && dialogueActive && currentIndex == decisionDialogueIndex)
        {
            waitingDecision = true;

            if (decisionButtons != null)
                decisionButtons.SetActive(true);
        }
    }


    public void AcceptOffer()
    {
        if (waitingDecision == false || dialogueActive == false)
            return;

        waitingDecision = false;
        HideButtons();
        ForceCompleteTyping();

        bool canPay =
            MoneyData.Instance != null && MoneyData.Instance.CanAfford(price);

        bool canReceiveItem =
            !isItemSeller ||
            (itemSeller == null && inventoryIncreaseSeller == null) ||
            (itemSeller != null && itemSeller.HasSpaceForAnyItem()) ||
            (inventoryIncreaseSeller != null && inventoryIncreaseSeller.CanSellIncrease());

        if (canPay && canReceiveItem)
        {
            MoneyData.Instance.RemoveMoney(price);
            purchaseCompleted = true;

            if (isItemSeller && itemSeller != null)
                itemSeller.BuyItem();

            if (isItemSeller && inventoryIncreaseSeller != null)
                inventoryIncreaseSeller.BuyIncrease();

            onBuy?.Invoke();
        }
        else
        {
            currentIndex = dialogues.Count - 3;
        }

        base.NextDialogue();
    }


    public void RejectOffer()
    {
        if (waitingDecision == false || dialogueActive == false)
            return;

        waitingDecision = false;
        HideButtons();
        ForceCompleteTyping();

        currentIndex = dialogues.Count - 2;

        base.NextDialogue();
    }


    protected override void EndText()
    {
        waitingDecision = false;
        HideButtons();

        base.EndText();
    }


    private void HideButtons()
    {
        if (decisionButtons != null)
            decisionButtons.SetActive(false);
    }
}