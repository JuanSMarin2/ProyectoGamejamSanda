using TMPro;
using UnityEngine;


public class ArtworkTester : MonoBehaviour
{
    [SerializeField] private TMP_Text shineText;
    [SerializeField] private TMP_Text rustText;
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private TMP_Text valueText;


    public void AddShine()
    {
        if (CurrentArtworkData.Instance == null)
            return;

        CurrentArtworkData.Instance.Artwork.shine += 5;
        CurrentArtworkData.Instance.Artwork.shine =
            Mathf.Clamp(CurrentArtworkData.Instance.Artwork.shine, 0, 100);
    }


    public void AddRust()
    {
        if (CurrentArtworkData.Instance == null)
            return;

        CurrentArtworkData.Instance.Artwork.rust += 5;
        CurrentArtworkData.Instance.Artwork.rust =
            Mathf.Clamp(CurrentArtworkData.Instance.Artwork.rust, 0, 100);
    }


    public void AddWeight()
    {
        if (CurrentArtworkData.Instance == null)
            return;

        CurrentArtworkData.Instance.Artwork.weight += 5;
        CurrentArtworkData.Instance.Artwork.weight =
            Mathf.Clamp(CurrentArtworkData.Instance.Artwork.weight, 0, 100);
    }


    public void AddValue()
    {
        if (CurrentArtworkData.Instance == null)
            return;

        CurrentArtworkData.Instance.Artwork.baseValue += 5;
    }


    private void Update()
    {
        if (CurrentArtworkData.Instance == null)
            return;


        shineText.text = "Brillante: " +
            CurrentArtworkData.Instance.Artwork.shine;


        rustText.text = "Oxido: " +
            CurrentArtworkData.Instance.Artwork.rust;


        weightText.text = "Peso: " +
            CurrentArtworkData.Instance.Artwork.weight;


        valueText.text = "Valor: " +
            CurrentArtworkData.Instance.Artwork.baseValue;
    }
}