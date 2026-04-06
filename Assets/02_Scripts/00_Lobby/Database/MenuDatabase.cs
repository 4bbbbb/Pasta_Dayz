using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuDatabase : MonoBehaviour
{
    public static MenuDatabase Instance;

    public List<MenuData> menuList = new List<MenuData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadMenuData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadMenuData()
    {
        menuList.Clear();

        var data = CSVReader.Read("Data/MenuData");

        foreach (var row in data)
        {
            int id = int.Parse(row["Menu_ID"].ToString());
            string name = row["Menu"].ToString().Trim();

            string ingredientRaw = row["IngredientsID"].ToString().Replace("\"", "");
            List<int> ingredientList = ingredientRaw
                .Split(',')
                .Select(x => int.Parse(x.Trim()))
                .ToList();

            bool isBaked = row["isBaked"]?.ToString().Trim() == "1";

            menuList.Add(new MenuData(id, name, ingredientList, isBaked));
        }
    }

    public MenuData GetMenuByID(int id)
    {
        return menuList.Find(m => m.menuID == id);
    }

    public bool IsMenuUnlocked(int menuID, IngredientDatabase ingredientDB)
    {
        MenuData menu = GetMenuByID(menuID);
        if (menu == null) return false;
        if (ingredientDB == null) return false;

        return menu.IngredientsID.All(id =>
        {
            var ingredient = ingredientDB.GetIngredient(id);
            return ingredient != null && ingredient.isUnlocked;
        });
    }
}