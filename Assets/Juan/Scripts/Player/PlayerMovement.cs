using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float initialSpeed = 3f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float acceleration = 12f;
    private bool movementEnabled = true;

    private Rigidbody2D rb;
    private PlayerInputActions inputActions;

    private Vector2 movementInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputActions = new PlayerInputActions();

    }


    private void OnEnable()
    { inputActions.Enable();
    }

    private void OnDisable()
    {  inputActions.Disable();
    }

    private void Update()
    {
        movementInput = inputActions.Player.Move.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Move();
    }

public void SetMovementEnabled(bool enabled)
{
    movementEnabled = enabled;

    if (!enabled)
        rb.linearVelocity = Vector2.zero;
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
        {rb.linearVelocity = direction * currentSpeed;
            return;
        }

        else
        { currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                maxSpeed,
                acceleration * Time.fixedDeltaTime
            );
        }

        rb.linearVelocity = direction * currentSpeed;
    }
}