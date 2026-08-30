using UnityEngine;

public enum AmbienceZone
{
    Ciudad = 0,
    Taller = 1,    
    MuseoTienda = 2,
    Parque = 3,
}

public class AmbienceParameterTrigger : MonoBehaviour
{
    [Header("Ambience Zone Settings")]
    [SerializeField] private AmbienceZone ambienceZone;

    //Optional parameter name to set in FMOD, if you want to set a specific parameter instead of just the zone

    [Header("Optional additional parametter config")]
   [SerializeField] private string parameterName = "";

   [SerializeField] private float parameterValue = 0f;
    
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            AudioManager.instance.SetAmbienceArea(ambienceZone);
            if (!string.IsNullOrEmpty(parameterName))
            {
                AudioManager.instance.SetAmbienceParameter(parameterName, parameterValue);  
                Debug.Log($"Set ambience parameter '{parameterName}' to value {parameterValue} for zone {ambienceZone}");              
            }
        }
        
    }

  
}
