using UnityEngine;

public class NpcAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float directionDeadZone = 0.01f;
    [SerializeField] private float directionHysteresis = 0.2f;

    private enum Facing
    {
        None,
        Front,
        Up,
        Right
    }

    private Facing currentFacing = Facing.None;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void SetMoveDirection(Vector2 direction)
    {
        if (animator == null)
            return;

        if (direction.sqrMagnitude < directionDeadZone)
        {
            SetFacing(Facing.None);
            return;
        }

        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);

        Facing newFacing = currentFacing;

        if (absX > absY + directionHysteresis)
        {
            newFacing = Facing.Right;
        }
        else if (absY > absX + directionHysteresis)
        {
            newFacing = direction.y > 0f ? Facing.Up : Facing.Front;
        }
        else if (currentFacing == Facing.None)
        {
            newFacing = absX >= absY
                ? Facing.Right
                : (direction.y > 0f ? Facing.Up : Facing.Front);
        }

        if (newFacing == Facing.Right && spriteRenderer != null && absX > directionDeadZone)
            spriteRenderer.flipX = direction.x < 0f;

        SetFacing(newFacing);
    }

    public void PlayIdle()
    {
        SetFacing(Facing.None);
    }

    public void PlayFront()
    {
        SetFacing(Facing.Front);
    }

    public void PlayDown()
    {
        SetFacing(Facing.Front);
    }

    public void PlayUp()
    {
        SetFacing(Facing.Up);
    }

    public void PlayRight(bool faceLeft)
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = faceLeft;

        SetFacing(Facing.Right);
    }

    private void SetFacing(Facing newFacing)
    {
        if (newFacing == currentFacing)
            return;

        if (animator == null)
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