using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArtworksPanel : MonoBehaviour
{
    [Serializable]
    private class ArtworkEntryUI
    {
        public Image artworkImage;
        public TMP_Text rustText;
        public TMP_Text shineText;
        public TMP_Text weightText;
    }

    [Header("Panel")]
    [SerializeField] private GameObject artworksPanel;
    [SerializeField] private Button closeButton;

    [Header("Obras (una entrada por obra)")]
    [SerializeField] private List<ArtworkEntryUI> artworkEntries = new List<ArtworkEntryUI>();

    private void Awake()
    {
        if (artworksPanel == null)
            artworksPanel = gameObject;

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    private void Start()
    {
        artworksPanel.SetActive(false);
    }

    public void OpenPanel()
    {
        RefreshEntries();
        artworksPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        artworksPanel.SetActive(false);
    }

    private void RefreshEntries()
    {
        IReadOnlyList<ArtworkData> artworks =
            ArtworkDisplayData.Instance != null
                ? ArtworkDisplayData.Instance.Artworks
                : null;

        for (int i = 0; i < artworkEntries.Count; i++)
        {
            ArtworkData artwork =
                artworks != null && i < artworks.Count
                    ? artworks[i]
                    : null;

            SetEntry(artworkEntries[i], artwork);
        }
    }

    private void SetEntry(ArtworkEntryUI entry, ArtworkData artwork)
    {
        bool hasArtwork = artwork != null;

        if (entry.artworkImage != null)
        {
            entry.artworkImage.gameObject.SetActive(hasArtwork);

            if (hasArtwork)
            {
                entry.artworkImage.sprite = artwork.artworkSprite;
                entry.artworkImage.preserveAspect = true;
            }
        }

        SetStatText(entry.rustText, hasArtwork, "Oxido: " + (hasArtwork ? artwork.rust.ToString("0") : ""));
        SetStatText(entry.shineText, hasArtwork, "Brillo: " + (hasArtwork ? artwork.shine.ToString("0") : ""));
        SetStatText(entry.weightText, hasArtwork, "Peso: " + (hasArtwork ? artwork.weight.ToString("0") : ""));
    }

    private void SetStatText(TMP_Text text, bool active, string value)
    {
        if (text == null)
            return;

        text.gameObject.SetActive(active);

        if (active)
            text.text = value;
    }
}