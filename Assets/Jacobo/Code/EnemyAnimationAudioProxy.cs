using UnityEngine;
using FMOD.Studio;
using FMODUnity;


public class EnemyAnimationAudioProxy : MonoBehaviour
{
    private EventInstance currentFootstepInstance;

    [Header("Attenuation Settings")]
    [SerializeField] private float maxDistance = 20f; // Distancia máxima a la que se escucha

    // Función llamada desde el Animation Event del enemigo
    public void PlayEnemyFootstep()
    {
        CleanupCurrentFootstep();

        // 1. Crear la instancia del evento
        currentFootstepInstance = RuntimeManager.CreateInstance(FMODEvents.instance.pasosEnemigo);

        // 2. LA CLAVE DE LA ESPACIALIZACIÓN: 
        // Esto vincula el evento de FMOD a la posición 3D exacta de este enemigo específico en Unity.
        RuntimeManager.AttachInstanceToGameObject(currentFootstepInstance, gameObject);

        // 3. Disparar y liberar memoria
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
}