using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Order : IHasIngredients
{
    public MenuData menuData;
    public int noodleID;
    public List<int> toppingIDs;

    private OrderTemplateDatabase ordertemplateDB;

    public bool IsBaked => menuData != null && menuData.isBaked;

    public Order(MenuData menu, int noodle, List<int> toppings, OrderTemplateDatabase db)
    {
        menuData = menu;
        noodleID = noodle;
        toppingIDs = toppings;
        ordertemplateDB = db;
    }

    public string GenerateOrderMessage(string noodleName, List<string> toppingNames)
    {
        string menuTemp = ordertemplateDB.GetRandomTemplate("Menu");
        string noodleTemp = ordertemplateDB.GetRandomTemplate("Noodle");
        string toppingTemp = ordertemplateDB.GetRandomTemplate("Topping");

        string toppingText = string.Join(", ", toppingNames);

        menuTemp = menuTemp.Replace("{menu}", menuData.menuName);
        noodleTemp = noodleTemp.Replace("{noodle}", noodleName);
        toppingTemp = toppingTemp.Replace("{topping}", toppingText);

        return menuTemp + "\n" + noodleTemp + " " + toppingTemp;
    }

    public string GetOrderText(IngredientDatabase ingredientDB)
    {
        string noodleName = ingredientDB.GetIngredient(noodleID).name;

        List<string> toppingNames = toppingIDs
            .Select(id => ingredientDB.GetIngredient(id).name)
            .ToList();

        return GenerateOrderMessage(noodleName, toppingNames);
    }

    public HashSet<int> GetIngredientSet()
    {
        HashSet<int> result = new HashSet<int>();

        foreach (int id in menuData.IngredientsID)
            result.Add(id);

        result.Add(noodleID);

        foreach (int id in toppingIDs)
            result.Add(id);

        return result;
    }

    public float Price(IngredientDatabase ingredientDB)
    {
        float total = 0f;

        foreach (int id in menuData.IngredientsID)
            total += ingredientDB.GetIngredient(id).price;

        total += ingredientDB.GetIngredient(noodleID).price;

        foreach (int id in toppingIDs)
            total += ingredientDB.GetIngredient(id).price;

        return total;
    }

    public float Ingredient_Cost(IngredientDatabase ingredientDB)
    {
        float total = 0f;

        foreach (int id in menuData.IngredientsID)
            total += ingredientDB.GetIngredient(id).ingredientCost;

        total += ingredientDB.GetIngredient(noodleID).ingredientCost;

        foreach (int id in toppingIDs)
            total += ingredientDB.GetIngredient(id).ingredientCost;

        return total;
    }
}