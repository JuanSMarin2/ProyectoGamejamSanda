using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class ShopDialogue : DialogueManager
{
    [Header("Shop")]
    [SerializeField] private int decisionDialogueIndex;
    [SerializeField] private int price;
    [SerializeField] private GameObject decisionButtons;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button rejectButton;
    [SerializeField] private UnityEvent onBuy;

    private bool waitingDecision;
    private bool purchaseCompleted;

    protected override int EndLimit =>
        purchaseCompleted ? dialogues.Count - 2 : dialogues.Count;


    protected override void Awake()
    {
        base.Awake();

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

        HideButtons();

        base.StartDialogue();
    }


    public override void NextDialogue()
    {
        if (waitingDecision)
            return;

        base.NextDialogue();

        if (dialogueActive && currentIndex == decisionDialogueIndex)
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

        if (MoneyData.Instance != null && MoneyData.Instance.CanAfford(price))
        {
            MoneyData.Instance.RemoveMoney(price);
            purchaseCompleted = true;
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
