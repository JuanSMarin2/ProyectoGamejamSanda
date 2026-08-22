using System.Collections.Generic;
using UnityEngine;


public class ArtworkDisplayVisual : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> shelves = new();
public static ArtworkDisplayVisual Instance { get; private set; }

private void Awake()
{
    Instance = this;
}
public Transform GetShelf(int index)
{
    if (index < 0 || index >= shelves.Count)
        return null;

    return shelves[index].transform;
}
    private void OnEnable()
    {
        if (ArtworkDisplayData.Instance != null)
            ArtworkDisplayData.Instance.OnArtworkDisplayChanged += UpdateVisual;

        UpdateVisual();
    }


    private void OnDisable()
    {
        if (ArtworkDisplayData.Instance != null)
            ArtworkDisplayData.Instance.OnArtworkDisplayChanged -= UpdateVisual;
    }


    public void UpdateVisual()
    {
        for (int i = 0; i < shelves.Count; i++)
        {
            bool hasArtwork = ArtworkDisplayData.Instance != null &&
                              i < ArtworkDisplayData.Instance.Artworks.Count;

            shelves[i].gameObject.SetActive(hasArtwork);

            if (hasArtwork)
                shelves[i].sprite = ArtworkDisplayData.Instance.Artworks[i].artworkSprite;
            else
                shelves[i].sprite = null;
        }
    }
}