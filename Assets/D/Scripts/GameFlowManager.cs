using System;
using UnityEngine;

public enum GamePhase
{
    Placement,
    Cleaning,
    Cutting,
    Welding,
    Painting,
    Completed
}

public class GameFlowManager : MonoBehaviour
{
    [Header("Phase Managers")]
    [SerializeField] private PieceSelectionManager pieceSelectionManager;
    [SerializeField] private CleaningManager cleaningManager;
    [SerializeField] private CuttingController cuttingController;
    [SerializeField] private WeldingManager weldingManager;
    [SerializeField] private PaintingManager paintingManager;

    public GamePhase CurrentPhase { get; private set; }
    
    public event Action OnPlacementCompleted;
    public event Action OnCleaningCompleted;
    public event Action OnCuttingCompleted;
    public event Action OnWeldingCompleted;
    public event Action OnPaintingCompleted;
    public event Action OnPhaseCompleted;

    private void Awake()
    {
        if (pieceSelectionManager == null)
            pieceSelectionManager = FindFirstObjectByType<PieceSelectionManager>();

        if (cleaningManager == null)
            cleaningManager = FindFirstObjectByType<CleaningManager>();

        if (cuttingController == null)
            cuttingController = FindFirstObjectByType<CuttingController>();

        if (weldingManager == null)
        weldingManager = FindFirstObjectByType<WeldingManager>();

        if (paintingManager == null)
        paintingManager = FindFirstObjectByType<PaintingManager>();

        if (pieceSelectionManager != null)
            pieceSelectionManager.OnSelectionPhaseCompleted += HandleSelectionPhaseCompleted;

        if (cleaningManager != null)
            cleaningManager.PhaseCompleted += HandleCleaningPhaseCompleted;
    
         if (cuttingController != null)
            cuttingController.OnCuttingCompleted += HandleCuttingPhaseCompleted;
        
        if (weldingManager != null)
            weldingManager.OnWeldingCompleted += HandleWeldingPhaseCompleted;

        if (paintingManager != null)
            paintingManager.OnPaintingCompleted += HandlePaintingPhaseCompleted;
        

        SetComponentEnabled(cleaningManager, false);
        SetComponentEnabled(cuttingController, false);
        SetComponentEnabled(weldingManager, false);
        SetComponentEnabled(paintingManager, false);
        SetCleaningObjectsEnabled(false);
        SetComponentEnabled(pieceSelectionManager, true);
        SetPhase(GamePhase.Placement);
    }

    private void OnDestroy()
    {
        if (pieceSelectionManager != null)
            pieceSelectionManager.OnSelectionPhaseCompleted -= HandleSelectionPhaseCompleted;

        if (cleaningManager != null)
            cleaningManager.PhaseCompleted -= HandleCleaningPhaseCompleted;

        if (cuttingController != null)
            cuttingController.OnCuttingCompleted -= HandleCuttingPhaseCompleted;

        if(weldingManager != null)
            weldingManager.OnWeldingCompleted -= HandleWeldingPhaseCompleted;
        
        if (paintingManager != null)
            paintingManager.OnPaintingCompleted -= HandlePaintingPhaseCompleted;
    }

    private void HandleSelectionPhaseCompleted()
    {
        Debug.Log("[GAME FLOW] Placement completado.");

        OnPlacementCompleted?.Invoke();

        SetPhase(GamePhase.Cleaning);

        SetComponentEnabled(pieceSelectionManager, false);
        SetComponentEnabled(cleaningManager, true);
        SetCleaningObjectsEnabled(true);

        Debug.Log("[GAME FLOW] Cambiando a: Cleaning");
    }

    private void HandleCleaningPhaseCompleted()
    {
        Debug.Log("[GAME FLOW] Cleaning completado.");
        OnCleaningCompleted?.Invoke();
        SetPhase(GamePhase.Cutting);
        SetComponentEnabled(cleaningManager, false);
        SetComponentEnabled(cuttingController, true);
        SetCleaningObjectsEnabled(false);
        Debug.Log("[GAME FLOW] Cambiando a: Cutting");
    }

    private void HandleCuttingPhaseCompleted()
   {
    Debug.Log("[GAME FLOW] Corte confirmado.");

    OnCuttingCompleted?.Invoke();

    // Pasar a Welding
    SetPhase(GamePhase.Welding);

    // Desactivar Cutting
    SetComponentEnabled(cuttingController, false);

    // Activar Welding
    SetComponentEnabled(weldingManager, true);

    SetCleaningObjectsEnabled(false);

    Debug.Log("[GAME FLOW] Cutting completado.");
    Debug.Log("[GAME FLOW] Cambiando a: Welding");
    }

    private void HandleWeldingPhaseCompleted()
    {
    Debug.Log("[GAME FLOW] Soldadura completada.");

    OnWeldingCompleted?.Invoke();

    SetPhase(GamePhase.Painting);

    SetComponentEnabled(weldingManager, false);

    SetComponentEnabled(paintingManager, true);

    SetCleaningObjectsEnabled(false);

    Debug.Log("[GAME FLOW] Welding completado.");
    Debug.Log("[GAME FLOW] Cambiando a: Painting");
  }

  private void HandlePaintingPhaseCompleted()
  {
    Debug.Log("[GAME FLOW] Pintura completada.");

    OnPaintingCompleted?.Invoke();

    SetPhase(GamePhase.Completed);

    SetComponentEnabled(paintingManager, false);

    Debug.Log("[GAME FLOW] Painting completado.");
    Debug.Log("[GAME FLOW] Fase completada.");

    OnPhaseCompleted?.Invoke();
  }  

    private void SetPhase(GamePhase phase)
    {
        CurrentPhase = phase;
        Debug.Log($"[GAME FLOW] Cambiando a: {phase}");
    }

    private void SetComponentEnabled(Behaviour component, bool isEnabled)
    {
        if (component != null)
            component.enabled = isEnabled;
    }

    private void SetCleaningObjectsEnabled(bool isEnabled)
    {
        CleaningObject[] cleaningObjects = FindObjectsByType<CleaningObject>(FindObjectsSortMode.None);

        foreach (CleaningObject cleaningObject in cleaningObjects)
            cleaningObject.SetInteractionEnabled(isEnabled);
    }
}
