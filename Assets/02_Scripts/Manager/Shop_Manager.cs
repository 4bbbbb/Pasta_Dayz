using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static IngredientData;

public class Shop_Manager : MonoBehaviour
{
    public static Shop_Manager Instance;

    // 상점 UI를 배치할 부모
    public Transform shopContentParent;

    // 아이템 UI prefab
    public GameObject shopItemPrefab;

    // 상점 아이템 UI 리스트
    private List<ShopItemUI> shopItemUIs = new List<ShopItemUI>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        shopItemUIs.Clear();

        foreach (Transform child in shopContentParent)
        {
            Destroy(child.gameObject);
        }

        PopulateShop();
        UpdateShopUI();
        FilterByCategory(CategoryType.Noodle);
    }

    // 상점 UI 생성
    void PopulateShop()
    {
        if (IngredientDatabase.Instance == null)
        {
            return;
        }

        foreach (var item in IngredientDatabase.Instance.ingredientList)
        {
            if (item.id == 204)
            {
                continue;
            }

            GameObject go = Instantiate(shopItemPrefab, shopContentParent);
            ShopItemUI ui = go.GetComponent<ShopItemUI>();
            ui.SetData(item, this);
            shopItemUIs.Add(ui);
        }
    }

    // 전체 UI 갱신
    public void UpdateShopUI()
    {
        foreach (var ui in shopItemUIs)
        {
            ui.ItemUI();
        }
    }

    public void PurchaseItem(IngredientData item)
    {
        if (!CanPurchase(item))
        {
            return;
        }

        Gold_Manager.Instance.SpendShop(item.unlockCost);

        IngredientDatabase.Instance.UpdateUnlockState(item.id, true);

        if (item.id == 203)
        {
            IngredientDatabase.Instance.UpdateUnlockState(204, true);
        }

        UpdateShopUI();

        if (Game_Manager.Instance != null)
        {
            Game_Manager.Instance.SaveGame();
        }

        if (ToppingManager.Instance != null)
        {
            ToppingManager.Instance.RefreshToppingUI();
        }

        if (Game_Manager.Instance != null)
        {
            Game_Manager.Instance.RefreshAllIngredientObjects();
        }
    }

    bool CanPurchase(IngredientData item)
    {
        return !item.isUnlocked &&
               Level_Manager.Instance.currentLevel >= item.unlockLevel &&
               Gold_Manager.Instance.totalGold >= item.unlockCost;
    }

    public void InitializeUI(Transform parent)
    {
        shopContentParent = parent;

        shopItemUIs.Clear();

        foreach (Transform child in shopContentParent)
        {
            Destroy(child.gameObject);
        }

        PopulateShop();
        UpdateShopUI();
        FilterByCategory(CategoryType.Noodle);
    }

    public void FilterByCategory(CategoryType type)
    {
        foreach (var ui in shopItemUIs)
        {
            bool isMatch = ui.GetCategory() == type;
            ui.gameObject.SetActive(isMatch);
        }
    }

    public void OnClickNoodle()
    {
        FilterByCategory(CategoryType.Noodle);
    }

    public void OnClickSauce()
    {
        FilterByCategory(CategoryType.Sauce);
    }

    public void OnClickTopping()
    {
        FilterByCategory(CategoryType.Topping);
    }

    public void OnClickCheese()
    {
        FilterByCategory(CategoryType.Cheese);
    }

    public void OnClickPlate()
    {
        FilterByCategory(CategoryType.Plate);
    }

    public void OnClickPane()
    {
        FilterByCategory(CategoryType.Pane);
    }
}