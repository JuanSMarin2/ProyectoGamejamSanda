using TMPro;
using UnityEngine;


public class MoneyVisual : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;


    private void OnEnable()
    {
        if (MoneyData.Instance != null)
            MoneyData.Instance.OnMoneyChanged += UpdateVisual;

        UpdateVisual();
    }


    private void OnDisable()
    {
        if (MoneyData.Instance != null)
            MoneyData.Instance.OnMoneyChanged -= UpdateVisual;
    }


    private void UpdateVisual()
    {
        if (MoneyData.Instance == null || moneyText == null)
            return;

        moneyText.text = MoneyData.Instance.Money.ToString();
    }
}