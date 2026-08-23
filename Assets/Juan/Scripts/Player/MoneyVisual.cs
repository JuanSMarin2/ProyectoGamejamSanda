using TMPro;
using UnityEngine;


public class MoneyVisual : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;


    private void Update()
    {
        if (MoneyData.Instance == null)
            return;

        moneyText.text = "" + MoneyData.Instance.Money;
    }
}