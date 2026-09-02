using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public enum InventorySection
{
    Base,
    Small,
    Large
}


public static class ItemDescriptionBuilder
{
    public static string BuildDescription(ObjectData item)
    {
        if (item == null)
            return string.Empty;


        int elegance = (int)item.Elegance;
        int robustness = (int)item.Robustness;
        int brightness = (int)item.Brightness;


        int maxValue = Mathf.Max(
            elegance,
            Mathf.Max(robustness, brightness)
        );


        if (maxValue <= 0)
            return string.Empty;


        List<string> adjectives = new List<string>();


        if (elegance == maxValue)
            adjectives.Add(GetEleganceName(elegance));

        if (robustness == maxValue)
            adjectives.Add(GetRobustnessName(robustness));

        if (brightness == maxValue)
            adjectives.Add(GetBrightnessName(brightness));


        return JoinAdjectives(adjectives);
    }


    private static string GetEleganceName(int value)
    {
        switch (value)
        {
            case 1:
                return "Un poco Elegante";

            case 2:
                return "Elegante";

            case 3:
                return "Muy Elegante";

            case 4:
                return "Demasiado Elegante";

            case 5:
                return "Majestuoso";

            default:
                return string.Empty;
        }
    }


    private static string GetRobustnessName(int value)
    {
        switch (value)
        {
            case 1:
                return "Un poco Robusto";

            case 2:
                return "Robusto";

            case 3:
                return "Muy Robusto";

            case 4:
                return "Demasiado Robusto";

            case 5:
                return "Indestructible";

            default:
                return string.Empty;
        }
    }


    private static string GetBrightnessName(int value)
    {
        switch (value)
        {
            case 1:
                return "Un poco Brillante";

            case 2:
                return "Brillante";

            case 3:
                return "Muy Brillante";

            case 4:
                return "Demasiado Brillante";

            case 5:
                return "Radiante";

            default:
                return string.Empty;
        }
    }


    private static string JoinAdjectives(List<string> adjectives)
    {
        if (adjectives.Count == 0)
            return string.Empty;


        if (adjectives.Count == 1)
            return adjectives[0];


        if (adjectives.Count == 2)
        {
            return adjectives[0] +
                   " y " +
                   adjectives[1].ToLower();
        }


        return adjectives[0] +
               ", " +
               adjectives[1].ToLower() +
               " y " +
               adjectives[2].ToLower();
    }
}


public class InventoryItemButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text descriptionText;


    private InventorySection section;
    private int localIndex;
    private InventoryPanel panel;


    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();


        if (itemImage == null)
        {
            Image[] images = GetComponentsInChildren<Image>(true);

            foreach (Image image in images)
            {
                if (image.gameObject != gameObject)
                {
                    itemImage = image;
                    break;
                }
            }
        }


        if (descriptionText == null)
            descriptionText = GetComponentInChildren<TMP_Text>(true);


        if (descriptionText != null)
            descriptionText.gameObject.SetActive(false);
    }


    public void Setup(
        InventorySection itemSection,
        int sectionIndex,
        InventoryPanel ownerPanel
    )
    {
        section = itemSection;
        localIndex = sectionIndex;
        panel = ownerPanel;


        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(NotifyClick);
        }
    }


    public void SetSprite(Sprite sprite)
    {
        if (itemImage == null)
            return;


        itemImage.enabled = sprite != null;
        itemImage.sprite = sprite;
    }


    public void SetDescription(string text)
    {
        if (descriptionText == null)
            return;


        bool hasText = !string.IsNullOrEmpty(text);


        descriptionText.gameObject.SetActive(hasText);
        descriptionText.text = hasText ? text : string.Empty;
    }


    private void NotifyClick()
    {
        if (panel != null)
            panel.OnSlotClicked(section, localIndex);
    }
}