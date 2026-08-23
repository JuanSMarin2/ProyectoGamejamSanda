using UnityEngine;

public class PieceObjectData : MonoBehaviour
{
    [SerializeField] private ObjectData data;
    [SerializeField] private bool isSelected;

    private void Awake()
    {
        ResolveDataIfMissing();
    }

    public ObjectData Data => data;
    public bool IsSelected => isSelected;
    public string PieceId => data != null ? data.PieceId : string.Empty;
    public PieceCategory Category => data != null ? data.Category : default;
    public FeatureRating Elegance => data != null ? data.Elegance : default;

    public FeatureRating Robustness => data != null ? data.Robustness : default;
    public FeatureRating Brightness => data != null ? data.Brightness : default;

    public void SetData(ObjectData newData)
    {
        data = newData;
        UpdateSprite();
    }

    private void ResolveDataIfMissing()
    {
        if (data != null)
            return;

        ObjectData[] availableData = Resources.LoadAll<ObjectData>("Items");
        PieceCategory category = GetCategoryFromName();
        int categoryIndex = GetIndexFromName();
        int expectedId = GetExpectedId(category, categoryIndex);

        foreach (ObjectData candidate in availableData)
        {
            if (candidate != null &&
                candidate.Category == category &&
            candidate.id == expectedId)
            {
                SetData(candidate);
                return;
            }
        }

        Debug.LogError(
            $"[PIECE] '{name}' no tiene ObjectData asignado y no se pudo " +
            "resolver automáticamente. Asigna un ObjectData en el Inspector."
        );
    }

    private PieceCategory GetCategoryFromName()
    {
        if (name.StartsWith("Base"))
            return PieceCategory.Base;

        if (name.StartsWith("Large"))
            return PieceCategory.LargeAccessory;

        return PieceCategory.SmallAccessory;
    }

    private int GetIndexFromName()
    {
        string[] nameParts = name.Split(' ');

        if (nameParts.Length > 1 && int.TryParse(nameParts[nameParts.Length - 1], out int index))
            return index;

        return 1;
    }

    private int GetExpectedId(PieceCategory category, int index)
    {
        switch (category)
        {
            case PieceCategory.Base:
                return 1;
            case PieceCategory.LargeAccessory:
                return 100 + (index - 1) % 2;
            case PieceCategory.SmallAccessory:
                return 200 + (index - 1) % 2;
            default:
                return -1;
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
    }

    private void UpdateSprite()
    {
        if (data == null)
        {
            Debug.LogWarning($"[PIECE] '{name}': data es null, no se puede asignar sprite.");
            return;
        }

        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogWarning($"[PIECE] '{name}': no se encontró SpriteRenderer.");
            return;
        }

        if (data.sprite == null)
        {
            Debug.LogWarning($"[PIECE] El SO '{data.itemName}' (id {data.id}) no tiene sprite asignado.");
            return;
        }

        spriteRenderer.sprite = data.sprite;
    }
}
