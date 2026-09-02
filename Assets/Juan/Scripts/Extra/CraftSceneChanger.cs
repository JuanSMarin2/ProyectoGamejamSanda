using System.Collections;
using UnityEngine;

public class CraftSceneChanger : MonoBehaviour
{
    [SerializeField] private GameObject notEnoughItemsPanel;
    [SerializeField] private InventoryPanel inventoryPanel;
    [SerializeField] private float panelDuration = 2f;

    private Coroutine panelCoroutine;

    private void Awake()
    {
        if (notEnoughItemsPanel != null)
            notEnoughItemsPanel.SetActive(false);

        if (inventoryPanel == null)
            inventoryPanel = FindFirstObjectByType<InventoryPanel>();
    }

    public void CheckInventoryAndWarp(string sceneName)
    {
        if (CanSelectRequiredItems())
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.EnterSelectionMode(sceneName);
                return;
            }

            Debug.LogError("[CRAFT CHANGER] No hay InventoryPanel en la escena.");
            return;
        }
        //En caso de no tener los objetos.
        AudioManager.instance.PlayOneShot(FMODEvents.instance.error, transform.position);
        ShowNotEnoughItemsPanel();
    }

    private bool CanSelectRequiredItems()
    {
        if (InventoryData.Instance == null)
            return false;

        bool hasBase =
            InventoryData.Instance.CountByCategory(PieceCategory.Base) >=
            SelectedItemsData.RequiredBase;

        bool hasLarge =
            InventoryData.Instance.CountByCategory(PieceCategory.LargeAccessory) >=
            SelectedItemsData.RequiredLarge;

        bool hasSmall =
            InventoryData.Instance.CountByCategory(PieceCategory.SmallAccessory) >=
            SelectedItemsData.RequiredSmall;

        return hasBase && hasLarge && hasSmall;
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