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

    [SerializeField] private TMP_Text coinsGainText;

    [SerializeField] private float scaleInDuration = 0.2f;
    [SerializeField] private float scaleOutDuration = 0.2f;
    [SerializeField] private float displayDuration = 3f;


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


        originalScale = feedbackPanel.transform.localScale;

        feedbackPanel.SetActive(false);
    }


    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
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
        int slotCount = Mathf.Min(itemImages.Count, itemNames.Count);

        for (int i = 0; i < slotCount; i++)
        {
            bool hasItem = i < items.Count;

            itemImages[i].gameObject.SetActive(hasItem);
            itemNames[i].gameObject.SetActive(hasItem);

            if (hasItem)
            {
                itemImages[i].sprite = items[i].sprite;
                itemNames[i].text = items[i].itemName;
            }
        }


        if (inventoryFull && itemNames.Count > 0)
        {
            itemNames[0].gameObject.SetActive(true);
            itemNames[0].text = "Inventario lleno";
        }


        if (coinsGainText != null)
        {
            coinsGainText.gameObject.SetActive(true);
            coinsGainText.text = "x" + coins;
        }
    }


    private IEnumerator ShowFeedbackCoroutine()
    {
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