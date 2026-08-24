using System;
using System.Collections.Generic;
using UnityEngine;


public class ArtworkDisplayData : MonoBehaviour
{
    public static ArtworkDisplayData Instance { get; private set; }

    [SerializeField] private int maxArtworks = 6;
    [SerializeField] private List<ArtworkData> artworks = new();


    public event Action OnArtworkDisplayChanged;


    public IReadOnlyList<ArtworkData> Artworks => artworks;
    public int MaxArtworks => maxArtworks;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        maxArtworks = Mathf.Clamp(maxArtworks, 1, 6);
    }


    public bool AddArtwork(ArtworkData artwork)
    {
        if (artwork == null)
            return false;

        if (artworks.Count >= maxArtworks)
            return false;

        artworks.Add(artwork);

        OnArtworkDisplayChanged?.Invoke();

        return true;
    }


    public bool RemoveArtwork(int index)
    {
        if (index < 0 || index >= artworks.Count)
            return false;

        artworks.RemoveAt(index);

        OnArtworkDisplayChanged?.Invoke();

        return true;
    }


    public ArtworkData GetArtwork(int index)
    {
        if (index < 0 || index >= artworks.Count)
            return null;
        AudioManager.instance.PlayOneShot(FMODEvents.instance.cachingComprar, transform.position);
        AudioManager.instance.PlayOneShot(FMODEvents.instance.construccionMejora, transform.position);
        return artworks[index];

    }
}