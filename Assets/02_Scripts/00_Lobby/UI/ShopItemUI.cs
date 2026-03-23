using UnityEngine;
using UnityEngine.UI;
using static IngredientData;

public class ShopItemUI : MonoBehaviour
{
    public Image iconImage;
    public Image ownedImage;
    public Image lockedImage;
    public Text nameText;
    public Text priceText;
    public Text statusText;
    public Button purchaseButton;

    [Header("SFX")]
    [SerializeField] private AudioClip clickSFX;

    private IngredientData itemData;
    private Shop_Manager shopManager;
    private Gold_Manager goldManager;

    public void SetData(IngredientData data, Shop_Manager manager)
    {
        itemData = data;
        shopManager = manager;

        iconImage.sprite = IngredientDatabase.Instance.GetIcon(itemData.id);
        purchaseButton.onClick.RemoveAllListeners();
        purchaseButton.onClick.AddListener(OnPurchaseButton);
        ItemUI();
    }

    void OnPurchaseButton()
    {
        PlayClickSFX();
        shopManager.PurchaseItem(itemData);
    }

    void PlayClickSFX()
    {
        if (SoundManager.Instance != null && clickSFX != null)
        {
            SoundManager.Instance.PlaySFX(clickSFX);
        }
    }

    public void ItemUI()
    {
        nameText.text = itemData.name;
        priceText.text = itemData.unlockCost > 0 ? $"$ {itemData.unlockCost}" : "Free";

        if (itemData.isUnlocked)
        {
            statusText.text = "";
            ownedImage.gameObject.SetActive(true);
            lockedImage.gameObject.SetActive(false);
            purchaseButton.gameObject.SetActive(false);
        }
        else if (Level_Manager.Instance.currentLevel < itemData.unlockLevel)
        {
            statusText.text = $"Lv.{itemData.unlockLevel}에서 잠금 해제";
            purchaseButton.gameObject.SetActive(false);
            ownedImage.gameObject.SetActive(false);
            lockedImage.gameObject.SetActive(true);
        }
        else
        {
            statusText.text = "";
            ownedImage.gameObject.SetActive(false);
            lockedImage.gameObject.SetActive(false);
            purchaseButton.gameObject.SetActive(true);
            purchaseButton.interactable =
                Gold_Manager.Instance.totalGold >= itemData.unlockCost;
        }
    }

    public CategoryType GetCategory()
    {
        return itemData.categoryType;
    }
}