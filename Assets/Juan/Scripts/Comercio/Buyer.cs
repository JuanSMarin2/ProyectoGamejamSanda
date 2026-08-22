using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Buyer : MonoBehaviour
{
    [Header("Preferences")]
    [SerializeField] private float rustPreference = 50f;
    [SerializeField] private float shinePreference = 50f;
    [SerializeField] private float weightPreference = 50f;


    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Vector3 shelfOffset = new Vector3(1f, 0f, 0f);


    [Header("Interaction")]
    [SerializeField] private InteractableObject interactable;


    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color interactColor = Color.yellow;


    [Header("Offer")]
    [SerializeField] private GameObject offerPanel;
    [SerializeField] private TMP_Text offerText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button rejectButton;


    [Header("Player")]
    [SerializeField] private PlayerMovement playerMovement;


    private Transform spawnPoint;
    private Transform targetShelf;

    private int targetArtworkIndex = -1;
    private ArtworkData targetArtwork;

    private Color originalColor;

    private bool movingToArtwork;
    private bool waitingForInteraction;
    private bool returning;

    private float offer;


    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;


        if (offerPanel != null)
            offerPanel.SetActive(false);


        if (interactable != null)
            interactable.SetInteractionEnabled(false);


        if (confirmButton != null)
            confirmButton.onClick.AddListener(ConfirmOffer);

        if (rejectButton != null)
            rejectButton.onClick.AddListener(RejectOffer);
    }


    public void Initialize(Transform newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;

        transform.position = spawnPoint.position;

        ChooseArtwork();
    }


    private void Update()
    {
        if (movingToArtwork)
        {
            MoveToArtwork();
        }
        else if (returning)
        {
            MoveToSpawn();
        }
    }


    private void ChooseArtwork()
    {
        if (ArtworkDisplayData.Instance == null)
            return;

        if (ArtworkDisplayData.Instance.Artworks.Count == 0)
            return;


        float bestDistance = float.MaxValue;
        int bestIndex = -1;


        for (int i = 0; i < ArtworkDisplayData.Instance.Artworks.Count; i++)
        {
            ArtworkData artwork = ArtworkDisplayData.Instance.Artworks[i];

            float distance = GetArtworkDistance(artwork);


            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestIndex = i;
            }
        }


        if (bestIndex == -1)
            return;


        targetArtworkIndex = bestIndex;
        targetArtwork = ArtworkDisplayData.Instance.Artworks[bestIndex];


        if (ArtworkDisplayVisual.Instance != null)
        {
            targetShelf = ArtworkDisplayVisual.Instance.GetShelf(bestIndex);
        }


        if (targetShelf == null)
            return;


        transform.position = spawnPoint.position;

        movingToArtwork = true;
    }


    private float GetArtworkDistance(ArtworkData artwork)
    {
        float rustDifference =
            Mathf.Abs(artwork.rust - rustPreference) / 100f;

        float shineDifference =
            Mathf.Abs(artwork.shine - shinePreference) / 100f;

        float weightDifference =
            Mathf.Abs(artwork.weight - weightPreference) / 100f;


        return (
            rustDifference +
            shineDifference +
            weightDifference
        ) / 3f;
    }


    private void MoveToArtwork()
    {
        if (targetShelf == null)
            return;


        Vector3 targetPosition = targetShelf.position + shelfOffset;


        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );


        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;

            movingToArtwork = false;
            waitingForInteraction = true;


            if (spriteRenderer != null)
                spriteRenderer.color = interactColor;


            if (interactable != null)
                interactable.SetInteractionEnabled(true);
        }
    }


    public void OpenOffer()
    {
        if (!waitingForInteraction)
            return;


        waitingForInteraction = false;


        if (interactable != null)
            interactable.SetInteractionEnabled(false);

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(false);


        offer = CalculateOffer();


        if (offerPanel != null)
            offerPanel.SetActive(true);


        if (offerText != null)
        {
            offerText.text =
                "Estoy dispuesto a darte $" +
                Mathf.RoundToInt(offer) +
                " por esta obra.";
        }
    }


    private float CalculateOffer()
    {
        if (targetArtwork == null)
            return 0f;


        float distance = GetArtworkDistance(targetArtwork);

        float compatibility = 1f - distance;

        compatibility = Mathf.Clamp01(compatibility);


        float multiplier = Mathf.Lerp(
            1f,
            2f,
            compatibility
        );


        return targetArtwork.baseValue * multiplier;
    }


    private void ConfirmOffer()
    {
        if (targetArtworkIndex < 0)
            return;


        MoneyData.Instance.AddMoney(
            Mathf.RoundToInt(offer)
        );


        ArtworkDisplayData.Instance.RemoveArtwork(
            targetArtworkIndex
        );


        FinishTransaction();
    }


    public void RejectOffer()
    {
        if (offerPanel != null && !offerPanel.activeSelf)
            return;


        FinishTransaction();
    }


    private void FinishTransaction()
    {
        waitingForInteraction = false;
        movingToArtwork = false;


        if (offerPanel != null)
            offerPanel.SetActive(false);


        if (interactable != null)
            interactable.SetInteractionEnabled(false);


        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);


        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;


        returning = true;
    }


    private void MoveToSpawn()
    {
        if (spawnPoint == null)
            return;


        transform.position = Vector3.MoveTowards(
            transform.position,
            spawnPoint.position,
            moveSpeed * Time.deltaTime
        );


        if (Vector3.Distance(transform.position, spawnPoint.position) < 0.01f)
        {
            transform.position = spawnPoint.position;

            returning = false;

            Destroy(gameObject);
        }
    }
}