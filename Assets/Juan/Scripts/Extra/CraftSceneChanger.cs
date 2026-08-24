using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CraftSceneChanger : MonoBehaviour
{
    [SerializeField] private GameObject notEnoughItemsPanel;
    [SerializeField] private GameObject fullShelfs;
    [SerializeField] private float panelDuration = 2f;

    private Coroutine panelCoroutine;


    private void Awake()
    {
        if (notEnoughItemsPanel != null)
            notEnoughItemsPanel.SetActive(false);

        if (fullShelfs != null)
            fullShelfs.SetActive(false);
    }


    public void CheckInventoryAndWarp(string sceneName)
    {
        if (InventoryData.Instance != null && InventoryData.Instance.IsFull())
        {
            if (ArtworkDisplayData.Instance != null && ArtworkDisplayData.Instance.Artworks.Count > 3)
            {
                ShowFullShelfs();
                return;
            }

            SceneManager.LoadScene(sceneName);
            return;
        }

        ShowNotEnoughItemsPanel();
    }


    private void ShowNotEnoughItemsPanel()
    {
        if (notEnoughItemsPanel == null)
            return;

        if (panelCoroutine != null)
            StopCoroutine(panelCoroutine);

        panelCoroutine = StartCoroutine(ShowPanelRoutine(notEnoughItemsPanel));
    }


    private void ShowFullShelfs()
    {
        if (fullShelfs == null)
            return;

        if (panelCoroutine != null)
            StopCoroutine(panelCoroutine);

        panelCoroutine = StartCoroutine(ShowPanelRoutine(fullShelfs));
    }


    private IEnumerator ShowPanelRoutine(GameObject panel)
    {
        panel.SetActive(true);

        yield return new WaitForSeconds(panelDuration);

        panel.SetActive(false);

        panelCoroutine = null;
    }
}