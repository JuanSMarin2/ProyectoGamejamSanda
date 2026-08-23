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
public class ObjectData : ScriptableObject
{
    [SerializeField] private string pieceId;
    [SerializeField] private PieceCategory category;
    [SerializeField] private FeatureRating elegance;
    [SerializeField] private FeatureRating robustness;
    [SerializeField] private FeatureRating brightness;

    public int id;
    public string itemName;
    public Sprite sprite;

    public string PieceId => pieceId;
    public PieceCategory Category => category;
    public FeatureRating Elegance => elegance;
    public FeatureRating Robustness => robustness;
    public FeatureRating Brightness => brightness;
}
