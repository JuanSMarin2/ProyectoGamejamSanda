using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;


public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private List<string> dialogues;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private float typingSpeed = 0.03f;
    [SerializeField] private bool isInteractable = true;

    [Header("Dialogue Space")]
    [SerializeField] private GameObject dialogueSpace;
    [SerializeField] private float scaleInDuration = 0.2f;
    [SerializeField] private float scaleOutDuration = 0.15f;

    [Header("Interaction")]
    [SerializeField] private GameObject interactableFeedback;
    [SerializeField] private PlayerMovement playerMovement;

    [System.Serializable]
    private class DialogueEvent
    {
        public int dialogueIndex;
        public UnityEvent onDialogueReached;
    }

    [Header("Events")]
    [SerializeField] private List<DialogueEvent> dialogueEvents;
    [SerializeField] private UnityEvent onDialogueEnd;

    private int currentIndex = -1;
    private Coroutine typingCoroutine;

    private bool isTyping;
    private bool dialogueActive;

    private Vector3 dialogueSpaceScale;

    private PlayerInputActions inputActions;


    private void Awake()
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


    public void StartDialogue()
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


    public void NextDialogue()
    {
        if (!dialogueActive)
            return;

        if (isTyping)
        {
            CompleteTextInstantly();
            return;
        }

        currentIndex++;

        if (currentIndex >= dialogues.Count)
        {
            EndText();
            return;
        }

        TriggerDialogueEvent(currentIndex);

        typingCoroutine = StartCoroutine(TypeText(dialogues[currentIndex]));
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


    private void EndText()
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