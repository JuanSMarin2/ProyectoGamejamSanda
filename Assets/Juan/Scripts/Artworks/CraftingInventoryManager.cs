using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CraftingInventoryManager : MonoBehaviour
{
    [Header("Crafting Pieces")]
    [SerializeField] private PieceObjectData basePiece;
    [SerializeField] private PieceObjectData[] largePieces = new PieceObjectData[2];
    [SerializeField] private PieceObjectData[] smallPieces = new PieceObjectData[3];

    [Header("Flow")]
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private CraftingManager craftingManager;
    [SerializeField] private string placedTag = "ItemDraggablePlaced";

    [Header("Results")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TMP_Text eleganceText;
    [SerializeField] private TMP_Text robustnessText;
    [SerializeField] private TMP_Text brightnessText;
    [SerializeField] private Image artworkImage;
    [SerializeField] private float resultsPanelDuration = 3f;
    [SerializeField] private string shopSceneName = "Tienda";

    private bool flowFinished;

    private float lastElegancePercent;
    private float lastRobustnessPercent;
    private float lastBrightnessPercent;

    private void Awake()
    {
        if (gameFlowManager == null)
            gameFlowManager = FindFirstObjectByType<GameFlowManager>();

        if (craftingManager == null)
            craftingManager = FindFirstObjectByType<CraftingManager>();

        if (resultsPanel != null)
            resultsPanel.SetActive(false);
    }

    private void Start()
    {
        AssignInventoryPieces();
    }

    private void OnEnable()
    {
        if (gameFlowManager != null)
            gameFlowManager.OnPhaseCompleted += HandleFlowCompleted;
    }

    private void OnDisable()
    {
        if (gameFlowManager != null)
            gameFlowManager.OnPhaseCompleted -= HandleFlowCompleted;
    }

    public void AssignInventoryPieces()
    {
        if (InventoryData.Instance == null)
        {
            Debug.LogWarning("[CRAFTING] InventoryData.Instance es null.");
            return;
        }

        Debug.Log($"[CRAFTING] Items en inventario: {InventoryData.Instance.Items.Count}");

        AssignCategory(PieceCategory.Base, new[] { basePiece });
        AssignCategory(PieceCategory.LargeAccessory, largePieces);
        AssignCategory(PieceCategory.SmallAccessory, smallPieces);
    }

    private void AssignCategory(PieceCategory category, PieceObjectData[] slots)
    {
        if (slots == null)
            return;

        int slotIndex = 0;

        foreach (ObjectData item in InventoryData.Instance.Items)
        {
            if (item == null || item.Category != category)
                continue;

            if (slotIndex >= slots.Length)
                break;

            if (slots[slotIndex] != null)
            {
                slots[slotIndex].SetData(item);
                Debug.Log($"[CRAFTING] '{slots[slotIndex].name}' recibió el SO '{item.itemName}' (id {item.id}).");
            }

            slotIndex++;
        }

        if (slotIndex == 0)
            Debug.LogWarning($"[CRAFTING] No hay ningún item de categoría {category} en el inventario.");
    }

    private void HandleFlowCompleted()
    {
        if (flowFinished)
            return;

        flowFinished = true;

        TagPiecesAsPlaced();
        ComputeAndStoreArtworkStats();

        if (craftingManager != null)
            craftingManager.FinishArtwork();
        else
            Debug.LogWarning("[CRAFTING] No hay CraftingManager asignado.");

        StartCoroutine(ShowResultsAndLoadShop());
    }

    private void TagPiecesAsPlaced()
    {
        TagPiece(basePiece);

        foreach (PieceObjectData piece in largePieces)
            TagPiece(piece);

        foreach (PieceObjectData piece in smallPieces)
            TagPiece(piece);
    }

    private void TagPiece(PieceObjectData piece)
    {
        if (piece != null)
            piece.gameObject.tag = placedTag;
    }

    private void ComputeAndStoreArtworkStats()
    {
        if (CurrentArtworkData.Instance == null)
        {
            Debug.LogWarning("[CRAFTING] CurrentArtworkData.Instance es null, no se guardan stats.");
            return;
        }

        float totalElegance = 0f;
        float totalRobustness = 0f;
        float totalBrightness = 0f;
        int pieceCount = 0;

        AccumulatePiece(basePiece, ref totalElegance, ref totalRobustness, ref totalBrightness, ref pieceCount);

        foreach (PieceObjectData piece in largePieces)
            AccumulatePiece(piece, ref totalElegance, ref totalRobustness, ref totalBrightness, ref pieceCount);

        foreach (PieceObjectData piece in smallPieces)
            AccumulatePiece(piece, ref totalElegance, ref totalRobustness, ref totalBrightness, ref pieceCount);

        if (pieceCount == 0)
        {
            Debug.LogWarning("[CRAFTING] No hay piezas con datos para calcular estadísticas.");
            return;
        }

        float avgElegance = totalElegance / pieceCount;
        float avgRobustness = totalRobustness / pieceCount;
        float avgBrightness = totalBrightness / pieceCount;

        lastElegancePercent = avgElegance / 3f * 100f;
        lastRobustnessPercent = avgRobustness / 3f * 100f;
        lastBrightnessPercent = avgBrightness / 3f * 100f;

        ArtworkData artwork = CurrentArtworkData.Instance.Artwork;

        artwork.rust = lastElegancePercent;
        artwork.weight = lastRobustnessPercent;
        artwork.shine = lastBrightnessPercent;
        artwork.baseValue = Mathf.RoundToInt((avgElegance + avgRobustness + avgBrightness) * 15f);

        Debug.Log($"[CRAFTING] Stats finales -> Elegancia: {lastElegancePercent:F1}% | Robustez: {lastRobustnessPercent:F1}% | Brillo: {lastBrightnessPercent:F1}% | Valor: ${artwork.baseValue}");
    }

    private void AccumulatePiece(
        PieceObjectData piece,
        ref float totalElegance,
        ref float totalRobustness,
        ref float totalBrightness,
        ref int pieceCount)
    {
        if (piece == null || piece.Data == null)
            return;

        totalElegance += (int)piece.Data.Elegance;
        totalRobustness += (int)piece.Data.Robustness;
        totalBrightness += (int)piece.Data.Brightness;
        pieceCount++;
    }

    private IEnumerator ShowResultsAndLoadShop()
    {
        UpdateResultsTexts();

        if (resultsPanel != null)
            resultsPanel.SetActive(true);

        yield return null;
        yield return null;

        UpdateResultsImage();

        yield return new WaitForSeconds(resultsPanelDuration);

        if (resultsPanel != null)
            resultsPanel.SetActive(false);

        SceneManager.LoadScene(shopSceneName);
    }

    private void UpdateResultsTexts()
    {
        if (eleganceText != null)
            eleganceText.text = $"Elegancia: {Mathf.RoundToInt(lastElegancePercent)}%";

        if (robustnessText != null)
            robustnessText.text = $"Robustez: {Mathf.RoundToInt(lastRobustnessPercent)}%";

        if (brightnessText != null)
            brightnessText.text = $"Brillo: {Mathf.RoundToInt(lastBrightnessPercent)}%";
    }

    private void UpdateResultsImage()
    {
        if (artworkImage == null)
            return;

        if (ArtworkDisplayData.Instance == null ||
            ArtworkDisplayData.Instance.Artworks.Count == 0)
        {
            Debug.LogWarning("[CRAFTING] No hay obras en la tienda para mostrar en el panel.");
            return;
        }

        ArtworkData lastArtwork =
            ArtworkDisplayData.Instance.Artworks[ArtworkDisplayData.Instance.Artworks.Count - 1];

        if (lastArtwork.artworkSprite == null)
        {
            Debug.LogWarning("[CRAFTING] La última obra no tiene sprite.");
            return;
        }

        artworkImage.sprite = lastArtwork.artworkSprite;
        artworkImage.preserveAspect = true;
    }
}