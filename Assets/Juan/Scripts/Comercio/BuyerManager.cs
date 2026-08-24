using System.Collections.Generic;
using UnityEngine;
using FMODUnity;


public class BuyerManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> buyerPrefabs = new();

    [SerializeField] private Transform spawnPoint;

    [SerializeField] private PlayerMovement playerMovement;

    [SerializeField] private float spawnInterval = 10f;

    [SerializeField]
    [Range(0f, 100f)]
    private float spawnProbability = 33f;

    [SerializeField] private string buyerCountParameterName = "TouristCount";


    private Buyer currentBuyer;
    private readonly HashSet<Buyer> activeBuyers = new();
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
        if (currentBuyer != null)
            return;

        if (ArtworkDisplayData.Instance == null)
            return;

        if (ArtworkDisplayData.Instance.Artworks.Count == 0)
            return;


        timer += Time.deltaTime;


        if (timer < spawnInterval)
            return;


        timer = 0f;


        if (Random.Range(0f, 100f) > spawnProbability)
            return;


        SpawnBuyer();
    }


    private void SpawnBuyer()
    {
        if (buyerPrefabs.Count == 0)
            return;


        int randomIndex = Random.Range(0, buyerPrefabs.Count);


        GameObject buyerObject = Instantiate(
            buyerPrefabs[randomIndex],
            spawnPoint.position,
            Quaternion.identity
        );


        currentBuyer = buyerObject.GetComponent<Buyer>();


        if (currentBuyer != null)
        {
            currentBuyer.Initialize(spawnPoint, this);
            if (activeBuyers.Add(currentBuyer))
                UpdateBuyerCountParameter();
        }
        else
        {
            Destroy(buyerObject);
        }
    }


    public void SetPlayerMovement(bool enabled)
    {
        if (playerMovement != null)
            playerMovement.SetMovementEnabled(enabled);
    }


    public void BuyerFinished(Buyer buyer)
    {
        if (currentBuyer == buyer)
            currentBuyer = null;

        if (buyer != null && activeBuyers.Remove(buyer))
            UpdateBuyerCountParameter();
    }

    private bool TryResolveMuseumAmbienceState()
    {
        if (hasResolvedMuseumAmbienceState)
            return true;

        if (AudioConfig.instance == null || FMODEvents.instance == null)
            return false;

        if (AudioConfig.instance.sceneAmbience.IsNull || FMODEvents.instance.museo.IsNull)
            return false;

        isMuseumAmbienceScene = AudioConfig.instance.sceneAmbience.Path == FMODEvents.instance.museo.Path;
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