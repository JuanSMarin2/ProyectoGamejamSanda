using UnityEngine;


public class PersistentKiller : MonoBehaviour
{
    private void Start()
    {
       Time.timeScale = 1;
        GameObject[] roundDataObjects = GameObject.FindGameObjectsWithTag("RoundData");


        foreach (GameObject obj in roundDataObjects)
        {
            Destroy(obj);
        }
    }
}