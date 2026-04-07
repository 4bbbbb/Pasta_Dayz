using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RecipeBook_UI : MonoBehaviour
{
    public static RecipeBook_UI Instance;

    [Header("DB")]
    [SerializeField] private MenuDatabase menuDatabase;

    [Header("오른쪽 패널")]
    [SerializeField] private Image dishImage;
    [SerializeField] private TMP_Text dishNameText;

    [Header("재료 슬롯 이미지")]
    [SerializeField] private Image oilImage;
    [SerializeField] private Image sauceImage;
    [SerializeField] private Image cheeseImage;
    [SerializeField] private Image plateImage;
    [SerializeField] private Image paneImage;
    [SerializeField] private Image parsleyImage;

    [Header("오일")]
    [SerializeField] private Sprite oilSprite;

    [Header("소스")]
    [SerializeField] private Sprite tomatoSauceSprite;
    [SerializeField] private Sprite creamSauceSprite;
    [SerializeField] private Sprite roseSauceSprite;
    [SerializeField] private Sprite vongoleSauceSprite;
    [SerializeField] private Sprite sauceEmptySprite;

    [Header("치즈")]
    [SerializeField] private Sprite parmesanSprite;
    [SerializeField] private Sprite mozzarellaSprite;
    [SerializeField] private Sprite cheeseEmptySprite;

    [Header("그릇")]
    [SerializeField] private Sprite normalPlateSprite;
    [SerializeField] private Sprite ovenPlateSprite;
    [SerializeField] private Sprite plateEmptySprite;

    [Header("빠네")]
    [SerializeField] private Sprite paneSprite;
    [SerializeField] private Sprite breadEmptySprite;

    [Header("파슬리")]
    [SerializeField] private Sprite parsleySprite;
    [SerializeField] private Sprite parsleyEmptySprite;

    [Header("완성 음식 이미지")]
    [SerializeField] private List<MenuSpriteData> menuSpriteList = new List<MenuSpriteData>();

    private Dictionary<int, Sprite> menuSpriteDict = new Dictionary<int, Sprite>();

    [System.Serializable]
    public class MenuSpriteData
    {
        public int menuID;
        public Sprite sprite;
    }

    private void Awake()
    {
        Instance = this;

        foreach (var data in menuSpriteList)
        {
            if (!menuSpriteDict.ContainsKey(data.menuID))
                menuSpriteDict.Add(data.menuID, data.sprite);
        }
    }

    private void Start()
    {
        ShowRecipeByID(1);
    }

    public void ShowRecipeByID(int menuID)
    {
        MenuData menu = menuDatabase.GetMenuByID(menuID);
        if (menu == null)
        {
            Debug.LogWarning($"Menu ID {menuID} 못 찾음");
            return;
        }

        UpdateDish(menu);
        UpdateIngredients(menu);
    }

    private void UpdateDish(MenuData menu)
    {
        dishNameText.text = menu.menuName;

        if (menuSpriteDict.TryGetValue(menu.menuID, out Sprite sprite))
            dishImage.sprite = sprite;
    }

    private void UpdateIngredients(MenuData menu)
    {
        oilImage.sprite = oilSprite;
        sauceImage.sprite = sauceEmptySprite;
        cheeseImage.sprite = cheeseEmptySprite;
        plateImage.sprite = plateEmptySprite;
        paneImage.sprite = breadEmptySprite;
        parsleyImage.sprite = parsleyEmptySprite;

        foreach (int id in menu.IngredientsID)
        {
            switch (id)
            {
                case 201:
                    oilImage.sprite = oilSprite;
                    break;

                case 202:
                    sauceImage.sprite = tomatoSauceSprite;
                    break;
                case 203:
                    sauceImage.sprite = creamSauceSprite;
                    break;
                case 204:
                    sauceImage.sprite = roseSauceSprite;
                    break;
                case 205:
                    sauceImage.sprite = vongoleSauceSprite;
                    break;

                case 401:
                    cheeseImage.sprite = parmesanSprite;
                    break;
                case 402:
                    cheeseImage.sprite = mozzarellaSprite;
                    break;

                case 501:
                    plateImage.sprite = normalPlateSprite;
                    break;
                case 502:
                    plateImage.sprite = ovenPlateSprite;
                    break;

                case 601:
                    paneImage.sprite = paneSprite;
                    break;

                case 701:
                    parsleyImage.sprite = parsleySprite;
                    break;
            }
        }
    }
}