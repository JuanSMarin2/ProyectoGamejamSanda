using System;
using UnityEngine;
using UnityEngine.UI; // Requerido para Button

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

    [Header("UI")]
    [SerializeField] private Button continueButton; // Arrastra tu botón único aquí
    [SerializeField] private CanvasGroup continueButtonCanvasGroup; // Arrastra el mismo objeto del botón aquí (debes añadirle un CanvasGroup en el inspector)

    public GamePhase CurrentPhase { get; private set; }
    
    public event Action OnPlacementCompleted;
    public event Action OnCleaningCompleted;
    public event Action OnCuttingCompleted;
    public event Action OnWeldingCompleted;
    public event Action OnPaintingCompleted;
    public event Action OnPhaseCompleted;

    private void Awake()
    {
        if (pieceSelectionManager == null) pieceSelectionManager = FindFirstObjectByType<PieceSelectionManager>();
        if (cleaningManager == null) cleaningManager = FindFirstObjectByType<CleaningManager>();
        if (cuttingController == null) cuttingController = FindFirstObjectByType<CuttingController>();
        if (weldingManager == null) weldingManager = FindFirstObjectByType<WeldingManager>();
        if (paintingManager == null) paintingManager = FindFirstObjectByType<PaintingManager>();

        if (pieceSelectionManager != null) pieceSelectionManager.OnSelectionPhaseCompleted += HandleSelectionPhaseCompleted;
        if (cleaningManager != null) cleaningManager.PhaseCompleted += HandleCleaningPhaseCompleted;
        if (cuttingController != null) cuttingController.OnCuttingCompleted += HandleCuttingPhaseCompleted;
        if (weldingManager != null) weldingManager.OnWeldingCompleted += HandleWeldingPhaseCompleted;
        if (paintingManager != null) paintingManager.OnPaintingCompleted += HandlePaintingPhaseCompleted;

        // Configurar el botón
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        }

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
        if (pieceSelectionManager != null) pieceSelectionManager.OnSelectionPhaseCompleted -= HandleSelectionPhaseCompleted;
        if (cleaningManager != null) cleaningManager.PhaseCompleted -= HandleCleaningPhaseCompleted;
        if (cuttingController != null) cuttingController.OnCuttingCompleted -= HandleCuttingPhaseCompleted;
        if (weldingManager != null) weldingManager.OnWeldingCompleted -= HandleWeldingPhaseCompleted;
        if (paintingManager != null) paintingManager.OnPaintingCompleted -= HandlePaintingPhaseCompleted;
        
        if (continueButton != null) continueButton.onClick.RemoveListener(OnContinueButtonClicked);
    }

    // Este método es llamado cuando se presiona el botón "Continuar"
    private void OnContinueButtonClicked()
    {
        switch (CurrentPhase)
        {
            case GamePhase.Cleaning:
                HandleCleaningPhaseCompleted(); // Salta de Cleaning a Cutting
                break;
            case GamePhase.Cutting:
                HandleCuttingPhaseCompleted(); // Salta de Cutting a Welding
                break;
            case GamePhase.Welding:
                if (weldingManager != null) weldingManager.ConfirmWelding(); 
                else HandleWeldingPhaseCompleted(); // Por seguridad
                break;
            case GamePhase.Painting:
                if (paintingManager != null) paintingManager.ConfirmPainting();
                break;
        }
    }

    private void HandleSelectionPhaseCompleted()
    {
        Debug.Log("[GAME FLOW] Placement completado.");
        OnPlacementCompleted?.Invoke();
        SetPhase(GamePhase.Cleaning);

        SetComponentEnabled(pieceSelectionManager, false);
        SetComponentEnabled(cleaningManager, true);
        SetCleaningObjectsEnabled(true);
    }

    private void HandleCleaningPhaseCompleted()
    {
        Debug.Log("[GAME FLOW] Cleaning completado.");
        OnCleaningCompleted?.Invoke();
        SetPhase(GamePhase.Cutting);

        SetComponentEnabled(cleaningManager, false);
        SetComponentEnabled(cuttingController, true);
        SetCleaningObjectsEnabled(false);
    }

    private void HandleCuttingPhaseCompleted()
    {
        Debug.Log("[GAME FLOW] Cutting completado.");
        OnCuttingCompleted?.Invoke();
        SetPhase(GamePhase.Welding);

        SetComponentEnabled(cuttingController, false);
        SetComponentEnabled(weldingManager, true);
        SetCleaningObjectsEnabled(false);
    }

    private void HandleWeldingPhaseCompleted()
    {
        Debug.Log("[GAME FLOW] Soldadura completada.");
        OnWeldingCompleted?.Invoke();
        SetPhase(GamePhase.Painting);

        SetComponentEnabled(weldingManager, false);
        SetComponentEnabled(paintingManager, true);
        SetCleaningObjectsEnabled(false);
    }

    private void HandlePaintingPhaseCompleted()
    {
        Debug.Log("[GAME FLOW] Pintura completada.");
        OnPaintingCompleted?.Invoke();
        SetPhase(GamePhase.Completed);

        SetComponentEnabled(paintingManager, false);
        OnPhaseCompleted?.Invoke();
    }  

    private void SetPhase(GamePhase phase)
    {
        CurrentPhase = phase;
        Debug.Log($"[GAME FLOW] Cambiando a: {phase}");
        UpdateContinueButtonState();
    }

    // Lógica visual e interactiva del botón
    private void UpdateContinueButtonState()
    {
        if (continueButton == null) return;

        // Está activo en todas las fases excepto Placement y Completed
        bool isButtonActive = (CurrentPhase != GamePhase.Placement && CurrentPhase != GamePhase.Completed);
        
        continueButton.interactable = isButtonActive;

        // Bajar transparencia si está desactivado usando un CanvasGroup
        if (continueButtonCanvasGroup != null)
        {
            continueButtonCanvasGroup.alpha = isButtonActive ? 1f : 0.4f;
        }
    }

    private void SetComponentEnabled(Behaviour component, bool isEnabled)
    {
        if (component != null) component.enabled = isEnabled;
    }

    private void SetCleaningObjectsEnabled(bool isEnabled)
    {
        CleaningObject[] cleaningObjects = FindObjectsByType<CleaningObject>(FindObjectsSortMode.None);
        foreach (CleaningObject cleaningObject in cleaningObjects)
            cleaningObject.SetInteractionEnabled(isEnabled);
    }
}