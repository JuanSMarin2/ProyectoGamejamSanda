using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Buyer : MonoBehaviour
{
    [Header("Preferences")]
    [SerializeField] private float rustPreference = 50f;
    [SerializeField] private float shinePreference = 50f;
    [SerializeField] private float weightPreference = 50f;


    [Header("Interest")]
    [SerializeField]
    [Range(0f, 1f)]
    private float maxInterestDistance = 0.6f;


    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Vector3 shelfOffset = new Vector3(1f, 0f, 0f);
    [SerializeField] private float minBrowseDuration = 2f;
    [SerializeField] private float maxBrowseDuration = 5f;


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


    private enum BuyerState
    {
        Browsing,
        MovingToQueueSpot,
        WaitingInQueue,
        ReadyToInteract,
        InTransaction,
        Leaving
    }

    private BuyerManager buyerManager;
    private NpcAnimatorController npcAnimator;

    private Transform spawnPoint;
    private ArtworkData targetArtwork;
    private int queueIndex = -1;

    private BuyerState state = BuyerState.Browsing;

    private Transform currentBrowseAnchor;
    private bool browseArrived;
    private float browseTimer;
    private float currentBrowseDuration;

    private Color originalColor;

    private float offer;


    private void Awake()
    {
        npcAnimator = GetComponent<NpcAnimatorController>();

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


    public void Initialize(Transform newSpawnPoint, BuyerManager newBuyerManager)
    {
        spawnPoint = newSpawnPoint;
        buyerManager = newBuyerManager;

        transform.position = spawnPoint.position;

        Decide();
    }


    private void Update()
    {
        switch (state)
        {
            case BuyerState.Browsing:
                UpdateBrowsing();
                break;
            case BuyerState.MovingToQueueSpot:
                UpdateMovingToQueueSpot();
                break;
            case BuyerState.Leaving:
                UpdateLeaving();
                break;
        }
    }


    private void Decide()
    {
        targetArtwork = null;
        queueIndex = -1;

        if (buyerManager == null ||
            ArtworkDisplayData.Instance == null ||
            ArtworkDisplayData.Instance.Artworks.Count == 0)
        {
            StartBrowsing();
            return;
        }


        List<ArtworkData> candidates = new List<ArtworkData>(ArtworkDisplayData.Instance.Artworks);
        candidates.Sort((a, b) => GetArtworkDistance(a).CompareTo(GetArtworkDistance(b)));

        foreach (ArtworkData artwork in candidates)
        {
            if (GetArtworkDistance(artwork) > maxInterestDistance)
                break;

            int newIndex = buyerManager.TryJoinQueue(artwork, this);

            if (newIndex < 0)
                continue;

            targetArtwork = artwork;
            queueIndex = newIndex;
            state = BuyerState.MovingToQueueSpot;

            if (npcAnimator != null)
                npcAnimator.PlayUp();

            return;
        }

        StartBrowsing();
    }


    private void StartBrowsing()
    {
        ReleaseCurrentAnchor();

        if (buyerManager == null)
        {
            StartLeaving();
            return;
        }

        currentBrowseAnchor = buyerManager.RequestBrowseAnchor(this, null);

        if (currentBrowseAnchor == null)
        {
            StartLeaving();
            return;
        }

        browseArrived = false;
        browseTimer = 0f;
        currentBrowseDuration = Random.Range(minBrowseDuration, maxBrowseDuration);
        state = BuyerState.Browsing;

        if (npcAnimator != null)
            npcAnimator.PlayUp();
    }


    private void UpdateBrowsing()
    {
        if (currentBrowseAnchor == null)
        {
            StartLeaving();
            return;
        }

        if (!browseArrived)
        {
            if (MoveTo(currentBrowseAnchor.position))
            {
                browseArrived = true;

                if (npcAnimator != null)
                    npcAnimator.PlayIdle();
            }

            return;
        }

        browseTimer += Time.deltaTime;

        if (browseTimer >= currentBrowseDuration)
            StartLeaving();
    }


    private void ReleaseCurrentAnchor()
    {
        if (currentBrowseAnchor != null && buyerManager != null)
            buyerManager.ReleaseBrowseAnchor(currentBrowseAnchor);

        currentBrowseAnchor = null;
    }


    private void UpdateMovingToQueueSpot()
    {
        if (buyerManager == null || targetArtwork == null || queueIndex < 0)
        {
            StartLeaving();
            return;
        }

        if (!buyerManager.IsArtworkOnDisplay(targetArtwork))
        {
            OnTargetArtworkGone();
            return;
        }

        Vector3? targetPosition = buyerManager.GetQueuePosition(targetArtwork, queueIndex, shelfOffset);

        if (!targetPosition.HasValue)
        {
            OnTargetArtworkGone();
            return;
        }

        if (!MoveTo(targetPosition.Value))
            return;

        if (queueIndex == 0)
        {
            BecomeFront();
        }
        else
        {
            state = BuyerState.WaitingInQueue;

            if (npcAnimator != null)
                npcAnimator.PlayIdle();
        }
    }


    private void BecomeFront()
    {
        state = BuyerState.ReadyToInteract;

        if (npcAnimator != null)
            npcAnimator.PlayIdle();

        if (spriteRenderer != null)
            spriteRenderer.color = interactColor;

        if (interactable != null)
            interactable.SetInteractionEnabled(true);
    }


    public void SetQueueIndex(int newIndex)
    {
        queueIndex = newIndex;

        if (state == BuyerState.ReadyToInteract ||
            state == BuyerState.InTransaction ||
            state == BuyerState.Leaving)
            return;

        state = BuyerState.MovingToQueueSpot;

        if (npcAnimator != null)
            npcAnimator.PlayUp();

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        if (interactable != null)
            interactable.SetInteractionEnabled(false);
    }


    public void OnTargetArtworkGone()
    {
        targetArtwork = null;
        queueIndex = -1;

        if (state == BuyerState.InTransaction || state == BuyerState.Leaving)
            return;

        if (interactable != null)
            interactable.SetInteractionEnabled(false);

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        Decide();
    }


    public void OpenOffer()
    {
        if (state != BuyerState.ReadyToInteract)
            return;

        state = BuyerState.InTransaction;

        if (interactable != null)
            interactable.SetInteractionEnabled(false);

        if (buyerManager != null)
            buyerManager.SetPlayerMovement(false);

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
        if (targetArtwork == null || ArtworkDisplayData.Instance == null)
        {
            FinishTransaction();
            return;
        }

        int artworkIndex = -1;

        IReadOnlyList<ArtworkData> artworks = ArtworkDisplayData.Instance.Artworks;

        for (int i = 0; i < artworks.Count; i++)
        {
            if (artworks[i] == targetArtwork)
            {
                artworkIndex = i;
                break;
            }
        }

        ArtworkData soldArtwork = targetArtwork;

        if (artworkIndex >= 0)
        {
            MoneyData.Instance.AddMoney(
                Mathf.RoundToInt(offer)
            );

            ArtworkDisplayData.Instance.RemoveArtwork(
                artworkIndex
            );
        }

        targetArtwork = null;

        if (buyerManager != null)
            buyerManager.OnArtworkSold(soldArtwork, this);

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
        if (offerPanel != null)
            offerPanel.SetActive(false);

        if (interactable != null)
            interactable.SetInteractionEnabled(false);

        if (buyerManager != null)
        {
            buyerManager.SetPlayerMovement(true);
            buyerManager.LeaveQueue(this);
        }

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        StartLeaving();
    }


    private void StartLeaving()
    {
        ReleaseCurrentAnchor();

        targetArtwork = null;
        queueIndex = -1;
        state = BuyerState.Leaving;

        if (interactable != null)
            interactable.SetInteractionEnabled(false);

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        if (npcAnimator != null)
            npcAnimator.PlayDown();
    }


    private void UpdateLeaving()
    {
        if (spawnPoint == null)
        {
            Destroy(gameObject);
            return;
        }

        if (!MoveTo(spawnPoint.position))
            return;

        if (buyerManager != null)
            buyerManager.BuyerFinished(this);

        Destroy(gameObject);
    }


    private bool MoveTo(Vector3 targetPosition)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            return true;
        }

        return false;
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
}