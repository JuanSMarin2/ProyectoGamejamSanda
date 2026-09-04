using System.Collections;
using UnityEngine;

public class FadeRoof : MonoBehaviour
{
    [SerializeField] private float alphaObjetivo = 0.3f;
    [SerializeField] private float velocidadFade = 2f;

    private SpriteRenderer spriteRenderer;
    private Coroutine corrutinaFade;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            IniciarFade(alphaObjetivo);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            IniciarFade(1f);
        }
    }

    private void IniciarFade(float alphaObjetivo)
    {
        if (corrutinaFade != null)
        {
            StopCoroutine(corrutinaFade);
        }

        corrutinaFade = StartCoroutine(CambiarAlpha(alphaObjetivo));
    }

    private IEnumerator CambiarAlpha(float alphaObjetivo)
    {
        Color color = spriteRenderer.color;

        while (!Mathf.Approximately(color.a, alphaObjetivo))
        {
            color.a = Mathf.MoveTowards(color.a, alphaObjetivo, velocidadFade * Time.deltaTime);
            spriteRenderer.color = color;
            yield return null;
        }

        color.a = alphaObjetivo;
        spriteRenderer.color = color;
        corrutinaFade = null;
    }
}
