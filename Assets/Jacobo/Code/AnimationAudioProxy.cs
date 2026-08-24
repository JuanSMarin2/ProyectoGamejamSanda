using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class AnimationAudioProxy : MonoBehaviour
{
    private EventInstance currentFootstepInstance;

    [Header("Raycast Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastDistance = 1.5f;

    // Esta es la función que llamarás directamente desde el Animation Event en Unity
    public void PlayFootstep()
    {
        CleanupCurrentFootstep();

        // 1. Detectar la superficie usando un Raycast hacia abajo
        int surfaceValue = GetSurfaceType();

        // 2. Crear, configurar y disparar la instancia del evento de FMOD
        currentFootstepInstance = RuntimeManager.CreateInstance(FMODEvents.instance.pasosPersonaje);
        
        // Posicionar el sonido en el espacio 3D del personaje
        RuntimeManager.AttachInstanceToGameObject(currentFootstepInstance, gameObject);

        // Asignar el parámetro de superficie que creamos en FMOD (0=Piedra, 1=Metal, 2=Madera, 3=Pasto)
        currentFootstepInstance.setParameterByName("SurfaceType", surfaceValue);

        // Disparar y liberar memoria automáticamente
        currentFootstepInstance.start();
        currentFootstepInstance.release();
    }

    private void OnDisable()
    {
        CleanupCurrentFootstep();
    }

    private void OnDestroy()
    {
        CleanupCurrentFootstep();
    }

    private void CleanupCurrentFootstep()
    {
        if (!currentFootstepInstance.isValid())
            return;

        currentFootstepInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        currentFootstepInstance.release();
        currentFootstepInstance.clearHandle();
    }

    private int GetSurfaceType()
    {
        Vector2 origin = new Vector2(transform.position.x, transform.position.y);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, raycastDistance, groundLayer);

        // Lanzamos un rayo desde los pies/centro del personaje hacia abajo
        if (hit.collider != null)
        {
            // Puedes identificar el piso por Tag o por Physic Material
            if (hit.collider.CompareTag("Metal")) return 2;
            if (hit.collider.CompareTag("Madera")) return 1;
            if (hit.collider.CompareTag("Pasto")) return 3;
            return 0;
        }
        
        return 0; // Valor por defecto (Piedra u otros)
    }
}