using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float initialSpeed = 3f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float acceleration = 12f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float directionDeadZone = 0.01f;

    private enum Facing
    {
        None,
        Front,
        Up,
        Right
    }

    private bool movementEnabled = true;

    private Rigidbody2D rb;
    private PlayerInputActions inputActions;

    private Vector2 movementInput;
    private Facing currentFacing = Facing.None;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputActions = new PlayerInputActions();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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
        movementInput = inputActions.Player.Move.ReadValue<Vector2>();

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        Move();
    }

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;

        if (!enabled)
        {
            rb.linearVelocity = Vector2.zero;
            SetFacing(Facing.None);
        }
    }

    private void Move()
    {
        if (!movementEnabled)
            return;

        if (movementInput.sqrMagnitude < 0.01f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = movementInput.normalized;
        float currentSpeed = rb.linearVelocity.magnitude;

        if (currentSpeed < 0.01f)
        {
            currentSpeed = initialSpeed;
        }
        else if (Vector2.Dot(rb.linearVelocity.normalized, direction) < 0f)
        {
            rb.linearVelocity = direction * currentSpeed;
            return;
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                maxSpeed,
                acceleration * Time.fixedDeltaTime
            );
        }

        rb.linearVelocity = direction * currentSpeed;
    }

    private void UpdateAnimation()
    {
        if (animator == null)
            return;

        if (!movementEnabled || movementInput.sqrMagnitude < directionDeadZone)
        {
            SetFacing(Facing.None);
            return;
        }

        Facing newFacing;

        if (Mathf.Abs(movementInput.x) > Mathf.Abs(movementInput.y))
        {
            newFacing = Facing.Right;

            if (spriteRenderer != null)
                spriteRenderer.flipX = movementInput.x < 0f;
        }
        else if (movementInput.y > 0f)
        {
            newFacing = Facing.Up;
        }
        else
        {
            newFacing = Facing.Front;
        }

        SetFacing(newFacing);
    }

    private void SetFacing(Facing newFacing)
    {
        if (newFacing == currentFacing)
            return;

        currentFacing = newFacing;

        switch (newFacing)
        {
            case Facing.Front:
                animator.SetTrigger("IsFront");
                break;
            case Facing.Up:
                animator.SetTrigger("IsUp");
                break;
            case Facing.Right:
                animator.SetTrigger("IsRight");
                break;
            case Facing.None:
                animator.SetTrigger("IsIdle");
                break;
        }
    }
}
