using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PieceSelectionManager : MonoBehaviour
{
    private const int MaxBasePieces = 1;
    private const int MaxLargeAccessories = 2;
    private const int MaxSmallAccessories = 3;

    [SerializeField] private bool discoverSceneObjects = true;
    [SerializeField] private PieceObjectData[] selectablePieces;
    [SerializeField] private BoardSlot[] boardSlots;

    public event Action OnSelectionPhaseCompleted;
    public bool IsSelectionPhaseCompleted { get; private set; }

    private Camera mainCamera;
    private readonly List<PieceObjectData> selectedPieces = new List<PieceObjectData>();

    private void Start()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
            mainCamera = FindFirstObjectByType<Camera>();

        if (discoverSceneObjects)
        {
            selectablePieces = FindObjectsByType<PieceObjectData>(FindObjectsSortMode.None);
            boardSlots = FindObjectsByType<BoardSlot>(FindObjectsSortMode.None);
        }

        CacheSelectedPieces();
        CheckSelectionPhaseCompletion();
    }

    private void Update()
    {
        if (IsSelectionPhaseCompleted ||
            mainCamera == null ||
            Mouse.current == null ||
            !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        TrySelectPiece(Mouse.current.position.ReadValue());
    }

    public bool TrySelectPiece(Vector2 screenPosition)
    {
        if (IsSelectionPhaseCompleted || mainCamera == null)
            return false;

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        Collider2D[] hitColliders = Physics2D.OverlapPointAll(worldPosition);

        foreach (Collider2D hitCollider in hitColliders)
        {
            PieceObjectData piece = hitCollider.GetComponentInParent<PieceObjectData>();

            if (TrySelectPiece(piece))
                return true;
        }

        return false;
    }

    public bool TrySelectPiece(PieceObjectData piece)
    {
        if (piece == null ||
            piece.Data == null ||
            piece.IsSelected ||
            !CanSelectCategory(piece.Category))
        {
            return false;
        }

        BoardSlot slot = FindFreeSlot(piece.Category);

        if (slot == null)
            return false;

        piece.transform.position = slot.Position;
        slot.SetOccupied(true);
        piece.SetSelected(true);
        selectedPieces.Add(piece);

        CheckSelectionPhaseCompletion();
        return true;
    }

    private BoardSlot FindFreeSlot(PieceCategory category)
    {
        if (boardSlots == null)
            return null;

        foreach (BoardSlot slot in boardSlots)
        {
            if (slot != null && slot.CanAccept(category))
                return slot;
        }

        return null;
    }

    private bool CanSelectCategory(PieceCategory category)
    {
        int selectedCount = CountSelectedPieces(category);

        switch (category)
        {
            case PieceCategory.Base:
                return selectedCount < MaxBasePieces;
            case PieceCategory.LargeAccessory:
                return selectedCount < MaxLargeAccessories;
            case PieceCategory.SmallAccessory:
                return selectedCount < MaxSmallAccessories;
            default:
                return false;
        }
    }

    private int CountSelectedPieces(PieceCategory category)
    {
        int count = 0;

        foreach (PieceObjectData piece in selectedPieces)
        {
            if (piece != null && piece.Category == category)
                count++;
        }

        return count;
    }

    private void CacheSelectedPieces()
    {
        selectedPieces.Clear();

        if (selectablePieces == null)
            return;

        foreach (PieceObjectData piece in selectablePieces)
        {
            if (piece != null && piece.IsSelected)
                selectedPieces.Add(piece);
        }
    }

    private void CheckSelectionPhaseCompletion()
    {
        if (selectedPieces.Count == MaxBasePieces + MaxLargeAccessories + MaxSmallAccessories &&
            CountSelectedPieces(PieceCategory.Base) == MaxBasePieces &&
            CountSelectedPieces(PieceCategory.LargeAccessory) == MaxLargeAccessories &&
            CountSelectedPieces(PieceCategory.SmallAccessory) == MaxSmallAccessories)
        {
            IsSelectionPhaseCompleted = true;
            OnSelectionPhaseCompleted?.Invoke();
        }
    }
}
