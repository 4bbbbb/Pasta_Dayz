using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop_Manager : MonoBehaviour
{
    public static Shop_Manager Instance;

    // 데이터베이스
    public IngredientDatabase ingredientDatabase;

    // 상점 UI를 배치할 부모
    public Transform shopContentParent;

    // 아이템 UI prefab
    public GameObject shopItemPrefab;

    // 상점 아이템 UI 리스트
    //private List<ShopItemUI> shopItemUIs = new List<ShopItemUI>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        PopulateShop();
        UpdateShopUI();
    }

    // 상점 UI 생성
    void PopulateShop()
    {
        foreach (var item in ingredientDatabase.ingredientList)
        {
            GameObject go = Instantiate(shopItemPrefab, shopContentParent);
            //ShopItemUI ui = go.GetComponent<ShopItemUI>();
            //ui.SetData(item, this); // 데이터 전달
            //shopItemUIs.Add(ui);
        }
    }

    // 전체 UI 갱신 (레벨업/골드 변경 시 호출)
    public void UpdateShopUI()
    {
        //foreach (var ui in shopItemUIs)
        {
            //ui.RefreshUI();
        }
    }

    // 구매 처리
    public void PurchaseItem(IngredientData item)
    {
        if (!CanPurchase(item)) return;

        Gold_Manager.Instance.Spend(item.unlockCost); // 골드 차감
        item.isUnlocked = true;

        UpdateShopUI();
    }

    // 구매 가능 조건
    bool CanPurchase(IngredientData item)
    {
        return !item.isUnlocked &&
               Level_Manager.Instance.currentLevel >= item.unlockLevel &&
               Gold_Manager.Instance.totalGold >= item.unlockCost;
    }
}