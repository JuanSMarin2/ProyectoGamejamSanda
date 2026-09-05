using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    [SerializeField] private CursorManager cursorManager;

    [Header("UI")]
    [SerializeField] private Button continueButton;
    [SerializeField] private CanvasGroup continueButtonCanvasGroup;

    [Header("Phase Panels")]
    [SerializeField] private RectTransform placementPanel;
    [SerializeField] private RectTransform cleaningPanel;
    [SerializeField] private RectTransform cuttingPanel;
    [SerializeField] private RectTransform weldingPanel;
    [SerializeField] private RectTransform paintingPanel;
    [SerializeField] private float panelHiddenOffset = 1200f;
    [SerializeField] private float panelDropDuration = 0.4f;
    [SerializeField] private float panelHoldDuration = 2f;
    [SerializeField] private float panelRiseDuration = 0.35f;
    [SerializeField] private float panelClickCooldown = 0.5f;
    [SerializeField] private AnimationCurve panelDropCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve panelRiseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public GamePhase CurrentPhase { get; private set; }

    public event Action OnPlacementCompleted;
    public event Action OnCleaningCompleted;
    public event Action OnCuttingCompleted;
    public event Action OnWeldingCompleted;
    public event Action OnPaintingCompleted;
    public event Action OnPhaseCompleted;

    private readonly Dictionary<RectTransform, Vector2> panelShownPositions = new();

    private RectTransform activePanel;
    private Coroutine panelRoutine;
    private bool panelSkipRequested;
    private float panelShownTime;

    private void Awake()
    {
        if (pieceSelectionManager == null) pieceSelectionManager = FindFirstObjectByType<PieceSelectionManager>();
        if (cleaningManager == null) cleaningManager = FindFirstObjectByType<CleaningManager>();
        if (cuttingController == null) cuttingController = FindFirstObjectByType<CuttingController>();
        if (weldingManager == null) weldingManager = FindFirstObjectByType<WeldingManager>();
        if (paintingManager == null) paintingManager = FindFirstObjectByType<PaintingManager>();
        if (cursorManager == null) cursorManager = FindFirstObjectByType<CursorManager>();

        if (pieceSelectionManager != null) pieceSelectionManager.OnSelectionPhaseCompleted += HandleSelectionPhaseCompleted;
        if (cleaningManager != null) cleaningManager.PhaseCompleted += HandleCleaningPhaseCompleted;
        if (cuttingController != null) cuttingController.OnCuttingCompleted += HandleCuttingPhaseCompleted;
        if (weldingManager != null) weldingManager.OnWeldingCompleted += HandleWeldingPhaseCompleted;
        if (paintingManager != null) paintingManager.OnPaintingCompleted += HandlePaintingPhaseCompleted;

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        }

        HidePanelInstant(placementPanel);
        HidePanelInstant(cleaningPanel);
        HidePanelInstant(cuttingPanel);
        HidePanelInstant(weldingPanel);
        HidePanelInstant(paintingPanel);

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

    private void Update()
    {
        if (activePanel == null || panelSkipRequested)
            return;

        if (Time.time - panelShownTime < panelClickCooldown)
            return;

        if (Mouse.current == null ||
            !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        if (IsClickOnPanel(activePanel, Mouse.current.position.ReadValue()))
            panelSkipRequested = true;
    }

    private bool IsClickOnPanel(RectTransform panel, Vector2 screenPosition)
    {
        Canvas canvas = panel.GetComponentInParent<Canvas>();

        Camera canvasCamera =
            canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;

        return RectTransformUtility.RectangleContainsScreenPoint(
            panel,
            screenPosition,
            canvasCamera
        );
    }

    private void OnContinueButtonClicked()
    {
        switch (CurrentPhase)
        {
            case GamePhase.Cleaning:
                HandleCleaningPhaseCompleted();
                break;
            case GamePhase.Cutting:
                HandleCuttingPhaseCompleted();
                break;
            case GamePhase.Welding:
                if (weldingManager != null) weldingManager.ConfirmWelding();
                else HandleWeldingPhaseCompleted();
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
        if (cursorManager != null)
            cursorManager.SetPhaseCursor(phase);
        ShowPhasePanel(phase);
    }

    private void ShowPhasePanel(GamePhase phase)
    {
        RectTransform panel = null;

        switch (phase)
        {
            case GamePhase.Placement:
                panel = placementPanel;
                break;
            case GamePhase.Cleaning:
                panel = cleaningPanel;
                break;
            case GamePhase.Cutting:
                panel = cuttingPanel;
                break;
            case GamePhase.Welding:
                panel = weldingPanel;
                break;
            case GamePhase.Painting:
                panel = paintingPanel;
                break;
        }

        ShowPanel(panel);
    }

    private void ShowPanel(RectTransform panel)
    {
        HideActivePanel();

        if (panel == null)
            return;

        if (!panelShownPositions.TryGetValue(panel, out Vector2 shownPosition))
        {
            shownPosition = panel.anchoredPosition;
            panelShownPositions[panel] = shownPosition;
        }

        activePanel = panel;
        panelSkipRequested = false;
        panelShownTime = Time.time;

        panel.gameObject.SetActive(true);
        panel.anchoredPosition = shownPosition + Vector2.up * panelHiddenOffset;

        panelRoutine = StartCoroutine(PanelSequence(panel, shownPosition));
    }

    private void HideActivePanel()
    {
        if (panelRoutine != null)
        {
            StopCoroutine(panelRoutine);
            panelRoutine = null;
        }

        if (activePanel != null)
            activePanel.gameObject.SetActive(false);

        activePanel = null;
        panelSkipRequested = false;
    }

    private void HidePanelInstant(RectTransform panel)
    {
        if (panel != null)
            panel.gameObject.SetActive(false);
    }

    private IEnumerator PanelSequence(RectTransform panel, Vector2 shownPosition)
    {
        Vector2 hiddenPosition = shownPosition + Vector2.up * panelHiddenOffset;

        yield return MovePanel(panel, hiddenPosition, shownPosition, panelDropDuration, panelDropCurve);

        float waitTime = 0f;

        while (waitTime < panelHoldDuration && !panelSkipRequested)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }

        panelSkipRequested = false;

        yield return MovePanel(panel, panel.anchoredPosition, hiddenPosition, panelRiseDuration, panelRiseCurve);

        panel.gameObject.SetActive(false);

        if (activePanel == panel)
        {
            activePanel = null;
            panelRoutine = null;
        }
    }

    private IEnumerator MovePanel(
        RectTransform panel,
        Vector2 from,
        Vector2 to,
        float duration,
        AnimationCurve curve)
    {
        if (duration <= 0f)
        {
            panel.anchoredPosition = to;
            yield break;
        }

        float time = 0f;

        while (time < duration && !panelSkipRequested)
        {
            time += Time.deltaTime;

            float progress = Mathf.Clamp01(time / duration);
            float eased = curve != null ? curve.Evaluate(progress) : progress;

            panel.anchoredPosition = Vector2.LerpUnclamped(from, to, eased);

            yield return null;
        }

        if (!panelSkipRequested)
            panel.anchoredPosition = to;
    }

    private void UpdateContinueButtonState()
    {
        if (continueButton == null) return;

        bool isButtonActive = (CurrentPhase != GamePhase.Placement && CurrentPhase != GamePhase.Completed);

        continueButton.interactable = isButtonActive;

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