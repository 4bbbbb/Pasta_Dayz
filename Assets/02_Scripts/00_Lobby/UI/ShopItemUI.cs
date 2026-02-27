using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    public Text nameText;
    public Text priceText;
    public Text statusText;
    public Button purchaseButton;

    private IngredientData itemData;
    private Shop_Manager shopManager;

    // 데이터를 UI에 연결
    public void SetData(IngredientData data, Shop_Manager manager)
    {
        itemData = data;
        shopManager = manager;

        purchaseButton.onClick.AddListener(OnPurchaseButton);
        RefreshUI();
    }

    // 버튼 클릭 처리
    void OnPurchaseButton()
    {
        shopManager.PurchaseItem(itemData);
    }

    // UI 갱신
    public void RefreshUI()
    {
        nameText.text = itemData.name;
        priceText.text = itemData.unlockCost > 0 ? $"{itemData.unlockCost}G" : "Free";

        if (itemData.isUnlocked)
        {
            statusText.text = "Owned";
            purchaseButton.interactable = false;
        }
        else if (Level_Manager.Instance.currentLevel < itemData.unlockLevel)
        {
            statusText.text = $"Unlock Lv.{itemData.unlockLevel}";
            purchaseButton.interactable = false;
        }
        else
        {
            statusText.text = $"Buy ({itemData.unlockCost}G)";
            purchaseButton.interactable = Gold_Manager.Instance.totalGold >= itemData.unlockCost;
        }
    }
}