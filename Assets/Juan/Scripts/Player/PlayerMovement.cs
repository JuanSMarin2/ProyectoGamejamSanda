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


    private bool movementEnabled = true;

    private Rigidbody2D rb;
    private PlayerInputActions inputActions;

    private Vector2 movementInput;


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


    public bool MovementEnabled => movementEnabled;

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;


        if (!enabled)
        {
            movementInput = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
        }
    }


    private void Move()
    {
        if (!movementEnabled)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }


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


        bool isMoving = movementEnabled && movementInput.sqrMagnitude > 0.01f;


        animator.SetBool("Moving", isMoving);


        if (!isMoving)
            return;


        if (Mathf.Abs(movementInput.x) > Mathf.Abs(movementInput.y))
        {
            animator.SetInteger("Direction", 2);


            if (spriteRenderer != null)
                spriteRenderer.flipX = movementInput.x < 0f;
        }
        else if (movementInput.y > 0f)
        {
            animator.SetInteger("Direction", 1);
        }
        else
        {
            animator.SetInteger("Direction", 0);
        }
    }
}