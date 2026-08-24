using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FMODUnity;

[RequireComponent(typeof(Button))]
public class FMODButtonSFX : MonoBehaviour, IPointerDownHandler
{
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    // Se dispara automáticamente cada vez que el usuario hace clic o presiona el botón
    public void OnPointerDown(PointerEventData eventData)
    {
        // Solo suena si el botón está interactuable (activo)
        if (button != null && button.interactable)
        {
            // Reproducir el sonido del botón usando FMOD
            AudioManager.instance.PlayOneShot(FMODEvents.instance.boton, transform.position);
        }
    }

    //Botones de crafteo:
    public void PlaySoldarButtonSound()
    {
        AudioManager.instance.PlayOneShot(FMODEvents.instance.soldar, transform.position);
    }
}