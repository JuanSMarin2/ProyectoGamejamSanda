using UnityEngine;

public class BoardSlot : MonoBehaviour
{
    [SerializeField] private PieceCategory allowedCategory;
    [SerializeField] private bool isOccupied;

    public PieceCategory AllowedCategory => allowedCategory;
    public bool IsOccupied => isOccupied;
    public Vector3 Position => transform.position;

    public bool CanAccept(PieceCategory category)
    {
        return !isOccupied && allowedCategory == category;
    }

    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;
    }
}
