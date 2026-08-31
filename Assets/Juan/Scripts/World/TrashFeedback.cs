using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class TrashFeedback : MonoBehaviour
{
    public static TrashFeedback Instance { get; private set; }


    [SerializeField] private GameObject feedbackPanel;

    [SerializeField] private List<Image> itemImages = new();
    [SerializeField] private List<TMP_Text> itemNames = new();

    [Header("Item Spaces")]
    [SerializeField] private GameObject itemSpace1;
    [SerializeField] private GameObject itemSpace2;
    [SerializeField] private GameObject itemSpace3;

    [Header("Coin")]
    [SerializeField] private GameObject coinSpace;
    [SerializeField] private TMP_Text coinsGainText;

    [SerializeField] private float scaleInDuration = 0.2f;
    [SerializeField] private float scaleOutDuration = 0.2f;
    [SerializeField] private float displayDuration = 3f;


    private readonly List<GameObject> itemSpaces = new();

    private Vector3 originalScale;
    private Coroutine feedbackCoroutine;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;


        if (feedbackPanel == null)
        {
            Debug.LogError("[TRASH FEEDBACK] No hay un panel de feedback asignado.");
            return;
        }


        originalScale = feedbackPanel.transform.localScale;

        RegisterSpace(itemSpace1);
        RegisterSpace(itemSpace2);
        RegisterSpace(itemSpace3);

        if (coinSpace != null)
            coinSpace.SetActive(false);

        feedbackPanel.SetActive(false);
    }


    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }


    private void RegisterSpace(GameObject space)
    {
        if (space == null)
            return;

        itemSpaces.Add(space);
        space.SetActive(false);
    }


    public void ShowFeedback(List<ObjectData> items, int coins, bool inventoryFull)
    {
        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);


        UpdateFeedback(items, coins, inventoryFull);

        feedbackCoroutine = StartCoroutine(ShowFeedbackCoroutine());
    }


    private void UpdateFeedback(List<ObjectData> items, int coins, bool inventoryFull)
    {
        int slotCount = itemSpaces.Count;
        slotCount = Mathf.Min(slotCount, itemImages.Count);
        slotCount = Mathf.Min(slotCount, itemNames.Count);

        for (int i = 0; i < slotCount; i++)
        {
            bool hasItem = items != null && i < items.Count && items[i] != null;

            itemSpaces[i].SetActive(hasItem);

            if (!hasItem)
                continue;

            if (itemImages[i] != null)
                itemImages[i].sprite = items[i].sprite;

            if (itemNames[i] != null)
                itemNames[i].text = items[i].itemName;
        }


        if (inventoryFull && itemSpaces.Count > 0 && itemNames.Count > 0)
        {
            itemSpaces[0].SetActive(true);

            if (itemNames[0] != null)
                itemNames[0].text = "Inventario lleno";
        }


        if (coinSpace != null)
        {
            coinSpace.SetActive(coins > 0);

            if (coins > 0 && coinsGainText != null)
                coinsGainText.text = "x" + coins;
        }
    }


    private IEnumerator ShowFeedbackCoroutine()
    {
        if (feedbackPanel == null)
            yield break;

        feedbackPanel.SetActive(true);


        yield return StartCoroutine(ScaleIn());


        yield return new WaitForSeconds(displayDuration);


        yield return StartCoroutine(ScaleOut());


        feedbackPanel.SetActive(false);

        feedbackCoroutine = null;
    }


    private IEnumerator ScaleIn()
    {
        float time = 0f;

        Vector3 startScale = originalScale * 0.01f;

        feedbackPanel.transform.localScale = startScale;


        while (time < scaleInDuration)
        {
            time += Time.deltaTime;

            float progress = time / scaleInDuration;
            progress = Mathf.SmoothStep(0f, 1f, progress);


            feedbackPanel.transform.localScale =
                Vector3.Lerp(startScale, originalScale, progress);


            yield return null;
        }


        feedbackPanel.transform.localScale = originalScale;
    }


    private IEnumerator ScaleOut()
    {
        float time = 0f;

        Vector3 startScale = feedbackPanel.transform.localScale;
        Vector3 endScale = originalScale * 0.01f;


        while (time < scaleOutDuration)
        {
            time += Time.deltaTime;

            float progress = time / scaleOutDuration;
            progress = Mathf.SmoothStep(0f, 1f, progress);


            feedbackPanel.transform.localScale =
                Vector3.Lerp(startScale, endScale, progress);


            yield return null;
        }


        feedbackPanel.transform.localScale = endScale;
    }
}