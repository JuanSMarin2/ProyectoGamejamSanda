using UnityEngine;

public enum PieceCategory
{
    Base,
    LargeAccessory,
    SmallAccessory
}

public enum FeatureRating
{
    [InspectorName("1 estrella")]
    OneStar = 1,
    [InspectorName("2 estrellas")]
    TwoStars = 2,
    [InspectorName("3 estrellas")]
    ThreeStars = 3
}

[CreateAssetMenu(fileName = "ObjectData", menuName = "Game/Object Data")]
public class ObjectDataProfile : ScriptableObject
{
    [SerializeField] private string pieceId;
    [SerializeField] private PieceCategory category;
    [SerializeField] private FeatureRating elegance;
    [SerializeField] private FeatureRating robustness;
    [SerializeField] private FeatureRating brightness;

    public string PieceId => pieceId;
    public PieceCategory Category => category;
    public FeatureRating Elegance => elegance;
    public FeatureRating Robustness => robustness;
    public FeatureRating Brightness => brightness;
}

public class ObjectData : MonoBehaviour
{
    [SerializeField] private ObjectDataProfile data;
    [SerializeField] private bool isSelected;

    public ObjectDataProfile Data => data;
    public bool IsSelected => isSelected;
    public string PieceId => data != null ? data.PieceId : string.Empty;
    public PieceCategory Category => data != null ? data.Category : default;
    public FeatureRating Elegance => data != null ? data.Elegance : default;
    public FeatureRating Robustness => data != null ? data.Robustness : default;
    public FeatureRating Brightness => data != null ? data.Brightness : default;

    public void SetSelected(bool selected)
    {
        isSelected = selected;
    }
}
