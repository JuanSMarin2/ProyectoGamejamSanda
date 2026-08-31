using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CraftingInventoryManager : MonoBehaviour
{
    [Header("Spawn Anchors")]
    [SerializeField] private Transform baseAnchor;
    [SerializeField] private Transform[] largeAnchors = new Transform[2];
    [SerializeField] private Transform[] smallAnchors = new Transform[3];

    [Header("Prefabs")]
    [SerializeField] private PrefabItemSetup prefabSetup;

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

    private PieceObjectData basePiece;
    private PieceObjectData[] largePieces = new PieceObjectData[2];
    private PieceObjectData[] smallPieces = new PieceObjectData[3];

    private readonly List<GameObject> spawnedPieces = new List<GameObject>();

    public IReadOnlyList<GameObject> SpawnedPieces => spawnedPieces;

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

        if (prefabSetup == null)
            prefabSetup = FindFirstObjectByType<PrefabItemSetup>();

        if (resultsPanel != null)
            resultsPanel.SetActive(false);

        SpawnInventoryPieces();
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

    private void SpawnInventoryPieces()
    {
        if (SelectedItemsData.Instance == null ||
            SelectedItemsData.Instance.SelectedItems.Count == 0)
        {
            Debug.LogWarning("[CRAFTING] No hay items seleccionados para craftear.");
            return;
        }

        if (prefabSetup == null)
        {
            Debug.LogWarning("[CRAFTING] No hay PrefabItemSetup en la escena.");
            return;
        }

        IReadOnlyList<ObjectData> sourceItems = SelectedItemsData.Instance.SelectedItems;

        basePiece = SpawnCategory(sourceItems, PieceCategory.Base, new[] { baseAnchor }) is { } spawnedBase
            ? spawnedBase[0]
            : null;

        PieceObjectData[] spawnedLarge = SpawnCategory(sourceItems, PieceCategory.LargeAccessory, largeAnchors);
        PieceObjectData[] spawnedSmall = SpawnCategory(sourceItems, PieceCategory.SmallAccessory, smallAnchors);

        for (int i = 0; i < largePieces.Length && i < spawnedLarge.Length; i++)
            largePieces[i] = spawnedLarge[i];

        for (int i = 0; i < smallPieces.Length && i < spawnedSmall.Length; i++)
            smallPieces[i] = spawnedSmall[i];
    }

    private PieceObjectData[] SpawnCategory(
        IReadOnlyList<ObjectData> sourceItems,
        PieceCategory category,
        Transform[] anchors)
    {
        if (anchors == null)
            return new PieceObjectData[0];

        PieceObjectData[] spawned = new PieceObjectData[anchors.Length];

        int anchorIndex = 0;

        foreach (ObjectData item in sourceItems)
        {
            if (item == null || item.Category != category)
                continue;

            if (anchorIndex >= anchors.Length)
                break;

            Transform anchor = anchors[anchorIndex];

            if (anchor == null)
            {
                Debug.LogWarning($"[CRAFTING] El ancla {anchorIndex} de {category} no está asignada.");
                anchorIndex++;
                continue;
            }

            GameObject prefab = prefabSetup.GetPrefab(category, item.id);

            if (prefab == null)
            {
                anchorIndex++;
                continue;
            }

            GameObject instance = Instantiate(prefab, anchor);
            instance.transform.localPosition = Vector3.zero;

            spawnedPieces.Add(instance);

            PieceObjectData piece = instance.GetComponentInChildren<PieceObjectData>();

            if (piece != null)
            {
                piece.SetData(item);
                spawned[anchorIndex] = piece;
                Debug.Log($"[CRAFTING] Instanciado '{prefab.name}' para '{item.itemName}' (id {item.id}) en '{anchor.name}'.");
            }
            else
            {
                Debug.LogWarning($"[CRAFTING] El prefab '{prefab.name}' no tiene PieceObjectData.");
            }

            anchorIndex++;
        }

        if (anchorIndex == 0)
            Debug.LogWarning($"[CRAFTING] No hay ningún item de categoría {category} en el inventario.");

        return spawned;
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

        if (SelectedItemsData.Instance != null)
            SelectedItemsData.Instance.ConsumeSelectedFromInventory();

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