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

    private IngredientData itemData;
    private Shop_Manager shopManager;  
    private Gold_Manager goldManager;

    // 데이터를 UI에 연결
    public void SetData(IngredientData data, Shop_Manager manager)
    {
        itemData = data;
        shopManager = manager;
        iconImage.sprite = itemData.icon;

        purchaseButton.onClick.RemoveAllListeners();
        purchaseButton.onClick.AddListener(OnPurchaseButton);
        ItemUI();
    }

    // 버튼 클릭 처리
    void OnPurchaseButton()
    {
        shopManager.PurchaseItem(itemData);
    }

    // UI 갱신
    public void ItemUI()
    {
        nameText.text = itemData.name;
        priceText.text = itemData.unlockCost > 0 ? $"$ {itemData.unlockCost}" : "Free";

        // 1. 보유중
        if (itemData.isUnlocked)
        {
            statusText.text = "";
            ownedImage.gameObject.SetActive(true);
            purchaseButton.gameObject.SetActive(false); 
        }
        // 2️. 레벨 부족
        else if (Level_Manager.Instance.currentLevel < itemData.unlockLevel)
        {
            statusText.text = $"Lv.{itemData.unlockLevel}에서 잠금 해제";
            purchaseButton.gameObject.SetActive(false);
            lockedImage.gameObject.SetActive(true);
        }
        // 3️. 구매 가능
        else
        {
            statusText.text = ""; 
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