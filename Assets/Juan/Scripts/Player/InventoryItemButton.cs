using UnityEngine;
using UnityEngine.UI;

public enum InventorySection
{
    Base,
    Small,
    Large
}

public class InventoryItemButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image itemImage;

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
    }

    public void Setup(InventorySection itemSection, int sectionIndex, InventoryPanel ownerPanel)
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

    private void NotifyClick()
    {
        if (panel != null)
            panel.OnSlotClicked(section, localIndex);
    }
}