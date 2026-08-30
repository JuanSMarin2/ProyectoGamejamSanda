using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class AnimationAudioProxy : MonoBehaviour
{
    private EventInstance currentFootstepInstance;

    [Header("Surface Detection")]
    [SerializeField] private GroundSurfaceDetector2D surfaceDetector;

    private void Awake()
    {
        if (surfaceDetector == null)
            surfaceDetector = GetComponentInChildren<GroundSurfaceDetector2D>();
    }

    // Esta es la función que llamarás directamente desde el Animation Event en Unity
    public void PlayFootstep()
    {
        CleanupCurrentFootstep();

        // Crear, configurar y disparar la instancia del evento de FMOD
        currentFootstepInstance = RuntimeManager.CreateInstance(FMODEvents.instance.pasosPersonaje);
        
        // Posicionar el sonido en el espacio 3D del personaje
        RuntimeManager.AttachInstanceToGameObject(currentFootstepInstance, gameObject);

        int surfaceType = surfaceDetector != null ? surfaceDetector.CurrentSurfaceType : 0;

        // Asignar el parámetro de superficie que creamos en FMOD
        currentFootstepInstance.setParameterByName("SurfaceType", surfaceType);

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
}