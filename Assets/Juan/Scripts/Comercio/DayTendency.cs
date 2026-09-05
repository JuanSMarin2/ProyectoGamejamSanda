using System.Collections.Generic;
using UnityEngine;

public class DayTendency : MonoBehaviour
{
    public enum TendencyStat
    {
        Elegance,
        Robustness,
        Brightness
    }

    [System.Serializable]
    private class DayTendencyEntry
    {
        public int day = 1;
        public TendencyStat tendency = TendencyStat.Elegance;
    }

    [Header("Tendency")]
    [SerializeField] private TendencyStat defaultTendency = TendencyStat.Elegance;
    [SerializeField] private List<DayTendencyEntry> tendenciesPerDay = new();

    [Header("Bonus")]
    [SerializeField] private float tendencyMultiplier = 1.2f;

    [SerializeField]
    [Range(0f, 100f)]
    private float minStatPercent = 50f;

    public static DayTendency Instance { get; private set; }

    public float TendencyMultiplier => tendencyMultiplier;

    public TendencyStat CurrentTendency
    {
        get
        {
            int currentDay =
                DayManager.Instance != null ? DayManager.Instance.CurrentDay : 1;

            DayTendencyEntry bestMatch = null;

            foreach (DayTendencyEntry entry in tendenciesPerDay)
            {
                if (entry == null || entry.day > currentDay)
                    continue;

                if (bestMatch == null || entry.day > bestMatch.day)
                    bestMatch = entry;
            }

            return bestMatch != null ? bestMatch.tendency : defaultTendency;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool MatchesTendency(ArtworkData artwork)
    {
        return artwork != null &&
            GetStatValue(artwork, CurrentTendency) >= minStatPercent;
    }

    private float GetStatValue(ArtworkData artwork, TendencyStat stat)
    {
        switch (stat)
        {
            case TendencyStat.Elegance:
                return artwork.rust;
            case TendencyStat.Robustness:
                return artwork.weight;
            case TendencyStat.Brightness:
                return artwork.shine;
            default:
                return 0f;
        }
    }
}