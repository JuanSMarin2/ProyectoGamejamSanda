using UnityEngine;

public class AudioDoor : MonoBehaviour
{
    
    public bool isStore; // Variable to determine if the door is for a store

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (isStore)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.abrirPuertaTienda, transform.position);
            }
            else
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.abrirPuerta, transform.position);
            }
        }
    }


}
