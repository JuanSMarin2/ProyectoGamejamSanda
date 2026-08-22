using System.Collections.Generic;
using UnityEngine;


public class BuyerManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> buyerPrefabs = new();

    [SerializeField] private Transform spawnPoint;

    [SerializeField] private float spawnInterval = 10f;

    [SerializeField]
    [Range(0f, 100f)]
    private float spawnProbability = 33f;


    private Buyer currentBuyer;

    private float timer;


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


        int randomIndex = Random.Range(
            0,
            buyerPrefabs.Count
        );


        GameObject buyerObject = Instantiate(
            buyerPrefabs[randomIndex],
            spawnPoint.position,
            Quaternion.identity
        );


        currentBuyer = buyerObject.GetComponent<Buyer>();


        if (currentBuyer != null)
        {
            currentBuyer.Initialize(spawnPoint);
        }
        else
        {
            Destroy(buyerObject);
        }
    }
}