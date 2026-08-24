using TMPro;
using UnityEngine;

public class CraftingStatusUI : MonoBehaviour
{
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private GameObject shapeGO;
    [SerializeField] private GameObject colorGO;

    private GamePhase lastPhase = (GamePhase)(-1);

    private void Awake()
    {
        if (gameFlowManager == null)
            gameFlowManager = FindFirstObjectByType<GameFlowManager>();
    }

    private void Start()
    {
        ApplyPhase(gameFlowManager != null ? gameFlowManager.CurrentPhase : GamePhase.Placement);
    }

    private void Update()
    {
        if (gameFlowManager == null)
            return;

        if (gameFlowManager.CurrentPhase != lastPhase)
            ApplyPhase(gameFlowManager.CurrentPhase);
    }

    private void ApplyPhase(GamePhase phase)
    {
        lastPhase = phase;

        if (statusText != null)
            statusText.text = GetStatusText(phase);

        if (shapeGO != null)
            shapeGO.SetActive(phase == GamePhase.Cutting);

        if (colorGO != null)
            colorGO.SetActive(phase == GamePhase.Painting);
    }

    private string GetStatusText(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.Placement:
                return "Selecciona los objetos que vas a utilizar para tu obra";
            case GamePhase.Cleaning:
                return "Limpia los objetos y quítales todo el óxido";
            case GamePhase.Cutting:
                return "Escoge la forma que quieres darle a tu base y corta las piezas con el mouse";
            case GamePhase.Welding:
                return "Coloca y suelda las piezas de tu obra";
            case GamePhase.Painting:
                return "Escoge los colores y pinta tus piezas";
            case GamePhase.Completed:
                return "¡Obra terminada!";
            default:
                return string.Empty;
        }
    }
}