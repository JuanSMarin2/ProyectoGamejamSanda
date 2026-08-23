using UnityEngine;

public class HeartBeath : MonoBehaviour
{

    [SerializeField] private float escalaMinima = 0.2f;


    [SerializeField] private float velocidad = 2f;

    private Vector3 escalaOriginal;

    private void Start()
    {
        escalaOriginal = transform.localScale;
    }

    private void Update()
    {

        float oscilacion = (Mathf.Sin(Time.time * velocidad) + 1f) / 2f;


        float escalaActual = Mathf.Lerp(escalaMinima, 1f, oscilacion);


        transform.localScale = escalaOriginal * escalaActual;
    }
}