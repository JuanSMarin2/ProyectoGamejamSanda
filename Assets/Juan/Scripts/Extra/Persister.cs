using UnityEngine;

public class Persister : MonoBehaviour
{
      public static Persister Instance { get; private set; }

        private void Awake()
        {
                if (Instance != null && Instance != this)
                {
                    Destroy(gameObject);
                    return;
                }
    
                Instance = this;
                DontDestroyOnLoad(gameObject);
        }
}
