using System.Collections.Generic;
using UnityEngine;
using FMODUnity;


public class BuyerManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> buyerPrefabs = new();

    [SerializeField] private Transform[] spawnPoints;

    [SerializeField] private PlayerMovement playerMovement;

    [SerializeField] private float spawnInterval = 10f;

    [SerializeField]
    [Range(0f, 100f)]
    private float spawnProbability = 33f;

    [SerializeField] private string buyerCountParameterName = "TouristCount";

    [Header("Crowd")]
    [SerializeField] private int maxBuyersInShop = 10;

    [System.Serializable]
    private class DayBuyerSettings
    {
        public int day = 1;

        [Range(0f, 100f)]
        public float spawnProbability = 33f;

        public int maxBuyersInShop = 10;
    }

    [Header("Progression")]
    [SerializeField] private List<DayBuyerSettings> buyersPerDay = new();
    [SerializeField] private int maxQueueSize = 3;
    [SerializeField] private Vector3 queueOffset = new Vector3(1f, 0f, 0f);

    [Header("Browsing")]
    [SerializeField] private Transform[] browseAnchors;


    private class ArtworkQueue
    {
        public ArtworkData artwork;
        public readonly List<Buyer> buyers = new List<Buyer>();
    }

    private readonly HashSet<Buyer> activeBuyers = new();
    private readonly List<ArtworkQueue> artworkQueues = new();
    private readonly Dictionary<Transform, Buyer> anchorOccupants = new Dictionary<Transform, Buyer>();

    private bool hasResolvedMuseumAmbienceState;
    private bool isMuseumAmbienceScene;

    private float timer;

    private void Start()
    {
        TryResolveMuseumAmbienceState();
        UpdateBuyerCountParameter();
    }


    private void Update()
    {
        GetCurrentDaySettings(out float probability, out int maxBuyers);

        if (activeBuyers.Count >= maxBuyers)
            return;

        timer += Time.deltaTime;

        if (timer < spawnInterval)
            return;

        timer = 0f;

        if (Random.Range(0f, 100f) > probability)
            return;

        SpawnBuyer();
    }


    private void GetCurrentDaySettings(out float probability, out int maxBuyers)
    {
        probability = spawnProbability;
        maxBuyers = maxBuyersInShop;

        int currentDay =
            DayManager.Instance != null ? DayManager.Instance.CurrentDay : 1;

        DayBuyerSettings bestMatch = null;

        foreach (DayBuyerSettings settings in buyersPerDay)
        {
            if (settings == null || settings.day > currentDay)
                continue;

            if (bestMatch == null || settings.day > bestMatch.day)
                bestMatch = settings;
        }

        if (bestMatch != null)
        {
            probability = bestMatch.spawnProbability;
            maxBuyers = bestMatch.maxBuyersInShop;
        }
    }


    private void SpawnBuyer()
    {
        if (buyerPrefabs.Count == 0 || spawnPoints == null || spawnPoints.Length == 0)
            return;

        int randomIndex = Random.Range(0, buyerPrefabs.Count);
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        if (spawnPoint == null)
            return;

        GameObject buyerObject = Instantiate(
            buyerPrefabs[randomIndex],
            spawnPoint.position,
            Quaternion.identity
        );

        Buyer buyer = buyerObject.GetComponent<Buyer>();

        if (buyer != null)
        {
            buyer.Initialize(spawnPoint, this);

            if (activeBuyers.Add(buyer))
                UpdateBuyerCountParameter();
        }
        else
        {
            Destroy(buyerObject);
        }
    }


    public Transform RequestBrowseAnchor(Buyer buyer, Transform excludeAnchor)
    {
        if (browseAnchors == null || browseAnchors.Length == 0)
            return null;

        List<Transform> freeAnchors = null;

        foreach (Transform anchor in browseAnchors)
        {
            if (anchor == null || anchor == excludeAnchor)
                continue;

            if (anchorOccupants.ContainsKey(anchor))
                continue;

            if (freeAnchors == null)
                freeAnchors = new List<Transform>();

            freeAnchors.Add(anchor);
        }

        if (freeAnchors == null || freeAnchors.Count == 0)
            return null;

        Transform chosen = freeAnchors[Random.Range(0, freeAnchors.Count)];
        anchorOccupants[chosen] = buyer;
        return chosen;
    }


    public void ReleaseBrowseAnchor(Transform anchor)
    {
        if (anchor != null)
            anchorOccupants.Remove(anchor);
    }


    public void ReleaseBrowseAnchorsOf(Buyer buyer)
    {
        if (buyer == null)
            return;

        List<Transform> toRemove = null;

        foreach (KeyValuePair<Transform, Buyer> pair in anchorOccupants)
        {
            if (pair.Value == buyer)
            {
                if (toRemove == null)
                    toRemove = new List<Transform>();

                toRemove.Add(pair.Key);
            }
        }

        if (toRemove == null)
            return;

        foreach (Transform anchor in toRemove)
            anchorOccupants.Remove(anchor);
    }


    public int TryJoinQueue(ArtworkData artwork, Buyer buyer)
    {
        if (artwork == null || buyer == null)
            return -1;

        ArtworkQueue queue = GetOrCreateQueue(artwork);

        if (queue.buyers.Count >= maxQueueSize)
            return -1;

        queue.buyers.Add(buyer);
        return queue.buyers.Count - 1;
    }


    public Vector3? GetQueuePosition(ArtworkData artwork, int queueIndex, Vector3 shelfOffset)
    {
        Transform shelf = FindShelf(artwork);

        if (shelf == null)
            return null;

        return shelf.position + shelfOffset + queueOffset * queueIndex;
    }


    public bool IsArtworkOnDisplay(ArtworkData artwork)
    {
        return artwork != null && IndexOfArtwork(artwork) >= 0;
    }


    public void LeaveQueue(Buyer buyer)
    {
        if (buyer == null)
            return;

        for (int q = artworkQueues.Count - 1; q >= 0; q--)
        {
            ArtworkQueue queue = artworkQueues[q];

            int index = queue.buyers.IndexOf(buyer);

            if (index < 0)
                continue;

            queue.buyers.RemoveAt(index);

            for (int i = 0; i < queue.buyers.Count; i++)
                queue.buyers[i].SetQueueIndex(i);

            if (queue.buyers.Count == 0)
                artworkQueues.RemoveAt(q);

            return;
        }
    }


    public void OnArtworkSold(ArtworkData artwork, Buyer sellingBuyer)
    {
        for (int q = artworkQueues.Count - 1; q >= 0; q--)
        {
            ArtworkQueue queue = artworkQueues[q];

            if (queue.artwork != artwork)
                continue;

            queue.buyers.Remove(sellingBuyer);

            List<Buyer> remaining = new List<Buyer>(queue.buyers);
            artworkQueues.RemoveAt(q);

            foreach (Buyer buyer in remaining)
            {
                if (buyer != null)
                    buyer.OnTargetArtworkGone();
            }

            return;
        }
    }


    public void SetPlayerMovement(bool enabled)
    {
        if (playerMovement != null)
            playerMovement.SetMovementEnabled(enabled);
    }


    public void BuyerFinished(Buyer buyer)
    {
        LeaveQueue(buyer);
        ReleaseBrowseAnchorsOf(buyer);

        if (buyer != null && activeBuyers.Remove(buyer))
            UpdateBuyerCountParameter();
    }


    private ArtworkQueue GetOrCreateQueue(ArtworkData artwork)
    {
        foreach (ArtworkQueue queue in artworkQueues)
        {
            if (queue.artwork == artwork)
                return queue;
        }

        ArtworkQueue newQueue = new ArtworkQueue { artwork = artwork };
        artworkQueues.Add(newQueue);
        return newQueue;
    }


    private Transform FindShelf(ArtworkData artwork)
    {
        if (ArtworkDisplayVisual.Instance == null)
            return null;

        int index = IndexOfArtwork(artwork);

        if (index < 0)
            return null;

        return ArtworkDisplayVisual.Instance.GetShelf(index);
    }


    private int IndexOfArtwork(ArtworkData artwork)
    {
        if (artwork == null || ArtworkDisplayData.Instance == null)
            return -1;

        IReadOnlyList<ArtworkData> artworks = ArtworkDisplayData.Instance.Artworks;

        for (int i = 0; i < artworks.Count; i++)
        {
            if (artworks[i] == artwork)
                return i;
        }

        return -1;
    }


    private bool TryResolveMuseumAmbienceState()
    {
        if (hasResolvedMuseumAmbienceState)
            return true;

        if (AudioConfig.instance == null || FMODEvents.instance == null)
            return false;

        if (AudioConfig.instance.sceneAmbience.IsNull || FMODEvents.instance.museo.IsNull)
            return false;

        isMuseumAmbienceScene = AudioConfig.instance.sceneAmbience.Guid == FMODEvents.instance.museo.Guid;
        hasResolvedMuseumAmbienceState = true;
        return true;
    }


    private void UpdateBuyerCountParameter()
    {
        if (!TryResolveMuseumAmbienceState() || !isMuseumAmbienceScene)
            return;

        if (AudioManager.instance == null)
            return;

        AudioManager.instance.SetAmbienceParameter(buyerCountParameterName, activeBuyers.Count);
    }
}