using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CraftSceneChanger : MonoBehaviour
{
    [SerializeField] private GameObject notEnoughItemsPanel;
    [SerializeField] private float panelDuration = 2f;

    private Coroutine panelCoroutine;

    private void Awake()
    {
        if (notEnoughItemsPanel != null)
            notEnoughItemsPanel.SetActive(false);
    }

    public void CheckInventoryAndWarp(string sceneName)
    {
        if (InventoryData.Instance != null && InventoryData.Instance.IsFull())
        {
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

        panelCoroutine = StartCoroutine(ShowPanelRoutine());
    }

    private IEnumerator ShowPanelRoutine()
    {
        notEnoughItemsPanel.SetActive(true);

        yield return new WaitForSeconds(panelDuration);

        notEnoughItemsPanel.SetActive(false);

        panelCoroutine = null;
    }
}
