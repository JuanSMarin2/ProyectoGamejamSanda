using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;


public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] protected List<string> dialogues;
    [SerializeField] protected TextMeshProUGUI dialogueText;
    [SerializeField] protected float typingSpeed = 0.03f;
    [SerializeField] protected bool isInteractable = true;

    [Header("Dialogue Space")]
    [SerializeField] protected GameObject dialogueSpace;
    [SerializeField] protected float scaleInDuration = 0.2f;
    [SerializeField] protected float scaleOutDuration = 0.15f;

    [Header("Interaction")]
    [SerializeField] protected GameObject interactableFeedback;
    [SerializeField] protected PlayerMovement playerMovement;

    [System.Serializable]
    protected class DialogueEvent
    {
        public int dialogueIndex;
        public UnityEvent onDialogueReached;
    }

    [Header("Events")]
    [SerializeField] protected List<DialogueEvent> dialogueEvents;
    [SerializeField] protected UnityEvent onDialogueEnd;

    protected int currentIndex = -1;
    protected Coroutine typingCoroutine;

    protected bool isTyping;
    protected bool dialogueActive;

    protected Vector3 dialogueSpaceScale;

    protected PlayerInputActions inputActions;

    protected virtual int EndLimit => dialogues.Count;


    protected virtual void Awake()
    {
        inputActions = new PlayerInputActions();
    }


    private void Start()
    {
        if (dialogueSpace != null)
        {
            dialogueSpaceScale = dialogueSpace.transform.localScale;
            dialogueSpace.SetActive(false);
        }

        if (isInteractable)
            return;

        StartDialogue();
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
        if (!dialogueActive)
            return;

        if (inputActions.Player.NextText.WasPressedThisFrame())
        {
            NextDialogue();
        }
    }


    public virtual void StartDialogue()
    {
        if (dialogueActive)
            return;

        dialogueActive = true;
        currentIndex = -1;

        if (interactableFeedback != null)
            interactableFeedback.SetActive(false);

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(false);

        if (dialogueSpace != null)
        {
            dialogueSpace.SetActive(true);
            StartCoroutine(ScaleIn());
        }

        NextDialogue();
    }


    public virtual void NextDialogue()
    {
        if (!dialogueActive)
            return;

        PlayDialogueSound();

        if (isTyping)
        {
            CompleteTextInstantly();
            return;
        }

        currentIndex++;

        if (currentIndex >= EndLimit)
        {
            EndText();
            return;
        }

        TriggerDialogueEvent(currentIndex);

        typingCoroutine = StartCoroutine(TypeText(dialogues[currentIndex]));
    }

    private void PlayDialogueSound()
    {
        if (AudioManager.instance != null && FMODEvents.instance != null && !FMODEvents.instance.uiBotonClick.IsNull)
            AudioManager.instance.PlayOneShot(FMODEvents.instance.uiBotonClick, transform.position);
    }


    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in text)
        {
            dialogueText.text += letter;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }


    private void CompleteTextInstantly()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.text = dialogues[currentIndex];
        isTyping = false;
    }


    protected void ForceCompleteTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        isTyping = false;
    }


    private IEnumerator ScaleIn()
    {
        float time = 0f;

        dialogueSpace.transform.localScale = dialogueSpaceScale * 0.01f;

        while (time < scaleInDuration)
        {
            time += Time.deltaTime;

            float progress = time / scaleInDuration;
            progress = Mathf.SmoothStep(0f, 1f, progress);

            dialogueSpace.transform.localScale =
                Vector3.Lerp(
                    dialogueSpaceScale * 0.01f,
                    dialogueSpaceScale,
                    progress
                );

            yield return null;
        }

        dialogueSpace.transform.localScale = dialogueSpaceScale;
    }


    private IEnumerator ScaleOut()
    {
        float time = 0f;

        Vector3 startingScale = dialogueSpace.transform.localScale;

        while (time < scaleOutDuration)
        {
            time += Time.deltaTime;

            float progress = time / scaleOutDuration;
            progress = Mathf.SmoothStep(0f, 1f, progress);

            dialogueSpace.transform.localScale =
                Vector3.Lerp(
                    startingScale,
                    dialogueSpaceScale * 0.01f,
                    progress
                );

            yield return null;
        }

        dialogueSpace.transform.localScale = dialogueSpaceScale * 0.01f;
        dialogueSpace.SetActive(false);
    }


    private void TriggerDialogueEvent(int index)
    {
        foreach (var dialogueEvent in dialogueEvents)
        {
            if (dialogueEvent.dialogueIndex == index)
            {
                dialogueEvent.onDialogueReached?.Invoke();
            }
        }
    }


    protected virtual void EndText()
    {
        dialogueActive = false;
        isTyping = false;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (dialogueSpace != null)
            StartCoroutine(ScaleOut());

        if (interactableFeedback != null)
            interactableFeedback.SetActive(true);

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);

        onDialogueEnd?.Invoke();
    }
}