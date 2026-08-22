using UnityEngine;


public class CurrentArtworkData : MonoBehaviour
{
    public static CurrentArtworkData Instance { get; private set; }

    [SerializeField] private ArtworkData artwork = new ArtworkData();

    public ArtworkData Artwork => artwork;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }


    public void SetArtwork(ArtworkData newArtwork)
    {
        artwork = newArtwork;
    }


    public void ClearArtwork()
    {
        artwork = new ArtworkData();
    }


    public bool HasArtwork()
    {
        return artwork.artworkSprite != null;
    }
}