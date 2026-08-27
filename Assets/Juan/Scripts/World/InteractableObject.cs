using UnityEngine;
using UnityEngine.Events;


public class InteractableObject : MonoBehaviour
{
    [SerializeField] private GameObject interactableFeedback;
    [SerializeField] private Sprite newInteractionSprite;
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private UnityEvent onInteract;

    private bool playerInside = false;
    private bool interactionEnabled = true;
    private bool newIndicatorEnabled = true;

    private PlayerInputActions inputActions;
    private SpriteRenderer feedbackRenderer;

    private Sprite feedbackSprite;
    private Sprite currentSprite;
    private float currentAlpha;


    private void Awake()
    {
        inputActions = new PlayerInputActions();

        if (interactableFeedback != null)
        {
            feedbackRenderer = interactableFeedback.GetComponent<SpriteRenderer>();

            if (feedbackRenderer != null)
            {
                feedbackSprite = feedbackRenderer.sprite;
                currentSprite = feedbackSprite;
                currentAlpha = 0f;

                Color color = feedbackRenderer.color;
                color.a = 0f;
                feedbackRenderer.color = color;
            }

            interactableFeedback.SetActive(true);
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
        if (!interactionEnabled || feedbackRenderer == null)
        {
            playerInside = false;
            FadeTo(0f);
            return;
        }


        if (playerInside && inputActions.Player.Interact.WasPressedThisFrame())
        {
            onInteract?.Invoke();
        }


        Sprite desiredSprite;
        float targetAlpha;

        if (playerInside)
        {
            desiredSprite = feedbackSprite;
            targetAlpha = 1f;
        }
        else if (newIndicatorEnabled && newInteractionSprite != null)
        {
            desiredSprite = newInteractionSprite;
            targetAlpha = 1f;
        }
        else
        {
            desiredSprite = currentSprite;
            targetAlpha = 0f;
        }


        if (desiredSprite != currentSprite)
        {
            FadeTo(0f);

            if (currentAlpha <= 0.001f)
            {
                currentSprite = desiredSprite;
                feedbackRenderer.sprite = currentSprite;
            }
        }
        else
        {
            FadeTo(targetAlpha);
        }
    }


    private void FadeTo(float targetAlpha)
    {
        if (feedbackRenderer == null)
            return;

        currentAlpha = Mathf.MoveTowards(
            currentAlpha,
            targetAlpha,
            fadeSpeed * Time.deltaTime
        );

        Color color = feedbackRenderer.color;
        color.a = currentAlpha;
        feedbackRenderer.color = color;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!interactionEnabled)
            return;

        if (!collision.CompareTag("Player"))
            return;

        playerInside = true;
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        playerInside = false;
    }


    public void SetInteractionEnabled(bool enabled)
    {
        interactionEnabled = enabled;

        if (!enabled)
        {
            playerInside = false;
        }
    }


    public void SetNewIndicatorEnabled(bool enabled)
    {
        newIndicatorEnabled = enabled;
    }
}