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
        if (ordertemplateDB == null)
        {
            Debug.LogError("Order.GenerateOrderMessage : ordertemplateDB가 null");
            return "";
        }

        if (menuData == null)
        {
            Debug.LogError("Order.GenerateOrderMessage : menuData가 null");
            return "";
        }

        string menuTemp = ordertemplateDB.GetRandomTemplate("Menu");
        if (menuTemp == null)
        {
            Debug.LogError("Order.GenerateOrderMessage : Menu 템플릿이 null");
            return "";
        }

        string noodleTemp = ordertemplateDB.GetRandomTemplate("Noodle");
        if (noodleTemp == null)
        {
            Debug.LogError("Order.GenerateOrderMessage : Noodle 템플릿이 null");
            return "";
        }

        string toppingTemp = ordertemplateDB.GetRandomTemplate("Topping");
        if (toppingTemp == null)
        {
            Debug.LogError("Order.GenerateOrderMessage : Topping 템플릿이 null");
            return "";
        }

        string toppingText = string.Join(", ", toppingNames);

        menuTemp = menuTemp.Replace("{menu}", menuData.menuName);
        noodleTemp = noodleTemp.Replace("{noodle}", noodleName);
        toppingTemp = toppingTemp.Replace("{topping}", toppingText);

        return menuTemp + "\n" + noodleTemp + " " + toppingTemp;
    }

    public string GetOrderText()
    {
        if (IngredientDatabase.Instance == null)
        {
            Debug.LogError("IngredientDatabase.Instance가 null입니다.");
            return "";
        }

        var noodle = IngredientDatabase.Instance.GetIngredient(noodleID);
        if (noodle == null)
            return "";

        string noodleName = noodle.name;

        List<string> toppingNames = toppingIDs
            .Select(id => IngredientDatabase.Instance.GetIngredient(id))
            .Where(x => x != null)
            .Select(x => x.name)
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

    public float Price()
    {
        if (IngredientDatabase.Instance == null)
        {
            Debug.LogError("IngredientDatabase.Instance가 null입니다.");
            return 0f;
        }

        float total = 0f;

        foreach (int id in menuData.IngredientsID)
        {
            var ingredient = IngredientDatabase.Instance.GetIngredient(id);
            if (ingredient != null)
                total += ingredient.price;
        }

        var noodle = IngredientDatabase.Instance.GetIngredient(noodleID);
        if (noodle != null)
            total += noodle.price;

        foreach (int id in toppingIDs)
        {
            var ingredient = IngredientDatabase.Instance.GetIngredient(id);
            if (ingredient != null)
                total += ingredient.price;
        }

        return total;
    }

    public float Ingredient_Cost()
    {
        if (IngredientDatabase.Instance == null)
        {
            Debug.LogError("IngredientDatabase.Instance가 null입니다.");
            return 0f;
        }

        float total = 0f;

        foreach (int id in menuData.IngredientsID)
        {
            var ingredient = IngredientDatabase.Instance.GetIngredient(id);
            if (ingredient != null)
                total += ingredient.ingredientCost;
        }

        var noodle = IngredientDatabase.Instance.GetIngredient(noodleID);
        if (noodle != null)
            total += noodle.ingredientCost;

        foreach (int id in toppingIDs)
        {
            var ingredient = IngredientDatabase.Instance.GetIngredient(id);
            if (ingredient != null)
                total += ingredient.ingredientCost;
        }

        return total;
    }
}