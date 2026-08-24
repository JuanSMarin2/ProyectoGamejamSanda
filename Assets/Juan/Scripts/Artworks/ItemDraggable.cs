using UnityEngine;
using UnityEngine.InputSystem;


public class ItemDraggable : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float returnSpeed = 5f;

    private ObjectData item;

    private Vector3 originalPosition;
    private Vector3 mouseOffset;

    private bool dragging;
    private bool placed;
    private bool returning;


    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        originalPosition = transform.position;

        gameObject.SetActive(false);
    }


    private void Update()
    {
        if (returning)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                originalPosition,
                returnSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, originalPosition) < 0.01f)
            {
                transform.position = originalPosition;
                returning = false;
            }
        }


        if (Mouse.current == null)
            return;


        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);


        if (Mouse.current.leftButton.wasPressedThisFrame && !dragging)
        {
            Collider2D hit = Physics2D.OverlapPoint(worldPosition);

            if (hit != null && hit.gameObject == gameObject)
            {
                dragging = true;
                returning = false;

                mouseOffset = transform.position - (Vector3)worldPosition;
            }
        }


        if (dragging && Mouse.current.leftButton.isPressed)
        {
            transform.position = (Vector3)worldPosition + mouseOffset;
        }


        if (dragging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            dragging = false;

            if (DraggableManager.Instance != null &&
                DraggableManager.Instance.IsCompletelyInsideWorkspace(this))
            {
                placed = true;
                gameObject.tag = "ItemDraggablePlaced";
            }
            else
            {
                placed = false;
                gameObject.tag = "ItemDraggable";
                returning = true;
            }
        }
    }


    public void SetItem(ObjectData newItem)
    {
        item = newItem;

        if (item == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (spriteRenderer != null)
            spriteRenderer.sprite = item.sprite;
    }


    public ObjectData GetItem()
    {
        return item;
    }


    public bool IsPlaced()
    {
        return placed;
    }


    public void ResetPosition()
    {
        transform.position = originalPosition;

        dragging = false;
        returning = false;
        placed = false;

        gameObject.tag = "ItemDraggable";
    }
}