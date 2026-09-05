using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    [field: Header("Personaje")]
    [field: SerializeField] public EventReference agarrarObjeto { get; private set; }
    [field: SerializeField] public EventReference moverPilaChatarra { get; private set; }
    [field: SerializeField] public EventReference abrirCaneca { get; private set; }
    [field: SerializeField] public EventReference cerrarCaneca { get; private set; }

    [field: SerializeField] public EventReference abrirPuerta { get; private set; }

    [field: SerializeField] public EventReference abrirPuertaTienda { get; private set; }
    [field: SerializeField] public EventReference pasosPersonaje { get; private set; }
    [field: SerializeField] public EventReference recolectarDinero { get; private set; }
    [field: SerializeField] public EventReference abrirInventario { get; private set; }
    [field: SerializeField] public EventReference abrirTaller { get; private set; }

    [field: Header("Mejora/Herramientas")]
    [field: SerializeField] public EventReference construccionMejora { get; private set; }
    [field: SerializeField] public EventReference cachingComprar { get; private set; }
    [field: SerializeField] public EventReference martilloContrayunque { get; private set; }
    [field: SerializeField] public EventReference lijar { get; private set; }
    [field: SerializeField] public EventReference martillo { get; private set; }
    [field: SerializeField] public EventReference soldar { get; private set; }
    [field: SerializeField] public EventReference sierraElectrica { get; private set; }
    [field: SerializeField] public EventReference serrucho { get; private set; }
    [field: SerializeField] public EventReference feedbackMegafono { get; private set; }

    [field: Header("Enemigos")]
    [field: SerializeField] public EventReference sirenaPolicia { get; private set; }
    [field: SerializeField] public EventReference pasosEnemigo { get; private set; }
    [field: SerializeField] public EventReference risaVagabundo { get; private set; }
    [field: SerializeField] public EventReference ajaVagabundo { get; private set; }
    [field: SerializeField] public EventReference slapRapar { get; private set; }
    [field: SerializeField] public EventReference layeringRobo { get; private set; }

    [field: Header("UI")]
    [field: SerializeField] public EventReference pausa { get; private set; }
    [field: SerializeField] public EventReference boton { get; private set; }
    [field: SerializeField] public EventReference uiBotonClick { get; private set; }
    [field: SerializeField] public EventReference beep { get; private set; }
    [field: SerializeField] public EventReference reactivarAudio { get; private set; }
    [field: SerializeField] public EventReference victoria { get; private set; }
    [field: SerializeField] public EventReference derrota { get; private set; }
    [field: SerializeField] public EventReference error { get; private set; }
    [field: SerializeField] public EventReference positivo { get; private set; }
    [field: SerializeField] public EventReference calendarioAbierto { get; private set; }
    [field: SerializeField] public EventReference aparicionDinero { get; private set; }
    [field: SerializeField] public EventReference SeleccionarColor { get; private set; }
    [field: SerializeField] public EventReference WooshAgudo { get; private set; }
    [field: SerializeField] public EventReference WooshGrave { get; private set; }
    [field: SerializeField] public EventReference ClockTicking { get; private set; }

    [field: SerializeField] public EventReference AplicarColor { get; private set; }

    [field: Header("Objetos")]
    [field: SerializeField] public EventReference metalesPequenos { get; private set; }
    [field: SerializeField] public EventReference metalesMedianos { get; private set; }
    [field: SerializeField] public EventReference metalesGrandes { get; private set; }

    [field: Header("Ambientes")]

    [field: SerializeField] public EventReference AmbienteGeneral { get; private set; }
    [field: SerializeField] public EventReference ciudad { get; private set; }
    [field: SerializeField] public EventReference museo { get; private set; }
    [field: SerializeField] public EventReference taller { get; private set; }
    [field: SerializeField] public EventReference parque { get; private set; }
    [field: SerializeField] public EventReference ScatterHerramientas { get; private set; } 

    [field: Header("Music")]

    [field: SerializeField] public EventReference MusicaGeneral { get; private set; }
    [field: SerializeField] public EventReference vals { get; private set; }
    [field: SerializeField] public EventReference reggae { get; private set; }
    [field: SerializeField] public EventReference funk { get; private set; }

    public static FMODEvents instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one FMOD Events instance in the scene.");
        }
        instance = this;
    }
}