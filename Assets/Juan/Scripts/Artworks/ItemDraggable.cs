using UnityEngine;
using UnityEngine.InputSystem;


public class ItemDraggable : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Item item;

    private Vector3 originalPosition;
    private Vector3 mouseOffset;

    private bool dragging = false;
    private bool placed = false;
    private bool returning = false;

    private float returnSpeed = 8f;


    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        originalPosition = transform.position;

        gameObject.tag = "ItemDraggable";

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


        if (!dragging)
            return;


        if (Mouse.current == null)
            return;


        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(
            Mouse.current.position.ReadValue()
        );

        mousePosition.z = transform.position.z;

        transform.position = mousePosition + mouseOffset;


        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            StopDragging();
        }
    }


    private void OnMouseDown()
    {
        if (Mouse.current == null)
            return;

        dragging = true;
        returning = false;


        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(
            Mouse.current.position.ReadValue()
        );

        mousePosition.z = transform.position.z;

        mouseOffset = transform.position - mousePosition;
    }


    private void StopDragging()
    {
        dragging = false;


        if (DraggableManager.Instance == null)
            return;


        if (DraggableManager.Instance.IsCompletelyInsideWorkspace(this))
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


    public void SetItem(Item newItem)
    {
        item = newItem;


        if (item == null)
        {
            gameObject.SetActive(false);
            return;
        }


        gameObject.SetActive(true);

        spriteRenderer.sprite = item.sprite;
    }


    public Item GetItem()
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


    private void OnDrawGizmos()
    {
        if (DraggableManager.Instance == null)
            return;

        if (DraggableManager.Instance.IsCompletelyInsideWorkspace(this))
        {
            Gizmos.DrawWireSphere(transform.position, 0.1f);
        }
    }
}