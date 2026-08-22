using UnityEngine;
using UnityEngine.Events;


public class InteractableObject : MonoBehaviour
{
    [SerializeField] private GameObject interactableFeedback;
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private UnityEvent onInteract;

    private bool playerInside = false;
    private bool interactionEnabled = true;

    private PlayerInputActions inputActions;
    private SpriteRenderer feedbackRenderer;


    private void Awake()
    {
        inputActions = new PlayerInputActions();

        if (interactableFeedback != null)
        {
            feedbackRenderer = interactableFeedback.GetComponent<SpriteRenderer>();

            if (feedbackRenderer != null)
            {
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
        if (!interactionEnabled)
        {
            playerInside = false;
            HideFeedback();
            return;
        }


        if (playerInside && inputActions.Player.Interact.WasPressedThisFrame())
        {
            onInteract?.Invoke();
        }


        if (feedbackRenderer == null)
            return;


        Color color = feedbackRenderer.color;

        if (playerInside)
        {
            color.a = Mathf.MoveTowards(
                color.a,
                1f,
                fadeSpeed * Time.deltaTime
            );
        }
        else
        {
            color.a = Mathf.MoveTowards(
                color.a,
                0f,
                fadeSpeed * Time.deltaTime
            );
        }

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
            HideFeedback();
        }
    }


    private void HideFeedback()
    {
        if (feedbackRenderer == null)
            return;

        Color color = feedbackRenderer.color;
        color.a = 0f;
        feedbackRenderer.color = color;
    }
}