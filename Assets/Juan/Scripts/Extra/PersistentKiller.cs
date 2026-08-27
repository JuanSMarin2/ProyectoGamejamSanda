using UnityEngine;


public class PersistentKiller : MonoBehaviour
{
    private void Start()
    {
        GameObject[] roundDataObjects = GameObject.FindGameObjectsWithTag("RoundData");


        foreach (GameObject obj in roundDataObjects)
        {
            Destroy(obj);
        }
    }
}