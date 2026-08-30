using System.Collections.Generic;
using UnityEngine;

public class GroundSurfaceDetector2D : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] private LayerMask groundLayer;

    private readonly List<Collider2D> activeGroundColliders = new List<Collider2D>();

    public int CurrentSurfaceType { get; private set; } = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsGround(other))
            return;

        if (!activeGroundColliders.Contains(other))
            activeGroundColliders.Add(other);

        RefreshSurfaceType();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsGround(other))
            return;

        activeGroundColliders.Remove(other);
        RefreshSurfaceType();
    }

    private bool IsGround(Collider2D other)
    {
        return ((1 << other.gameObject.layer) & groundLayer) != 0;
    }

    private void RefreshSurfaceType()
    {
        CurrentSurfaceType = 0;

        for (int i = activeGroundColliders.Count - 1; i >= 0; i--)
        {
            Collider2D groundCollider = activeGroundColliders[i];

            if (groundCollider == null)
                continue;

            int surfaceType = GetSurfaceType(groundCollider);
            if (surfaceType != 0)
            {
                CurrentSurfaceType = surfaceType;
                return;
            }
        }
    }

    private int GetSurfaceType(Collider2D groundCollider)
    {
        if (groundCollider.CompareTag("Madera"))
            return 1;

        if (groundCollider.CompareTag("Metal"))
            return 2;

        if (groundCollider.CompareTag("Pasto"))
            return 3;

        return 0;
    }
}