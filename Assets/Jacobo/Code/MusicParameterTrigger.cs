using UnityEngine;

public enum MusicZone
{
    TallerCiudad = 0,
    Tienda = 1,    
    Museo = 2,
}

public class MusicParameterTrigger : MonoBehaviour
{
    [Header("Music Zone Settings")]
    [SerializeField] private MusicZone musicZone;

    //Optional parameter name to set in FMOD, if you want to set a specific parameter instead of just the zone   [Header("Optional additional parametter config")]

    [Header("Optional additional parametter config")]
   [SerializeField] private string parameterName = "";

   [SerializeField] private float parameterValue = 0f;
    
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            AudioManager.instance.SetMusicArea(musicZone);
            if (!string.IsNullOrEmpty(parameterName))
            {
                AudioManager.instance.SetMusicParameter(parameterName, parameterValue);  
                Debug.Log($"Set music parameter '{parameterName}' to value {parameterValue} for zone {musicZone}");              
            }
        }
        
    }

  
}
