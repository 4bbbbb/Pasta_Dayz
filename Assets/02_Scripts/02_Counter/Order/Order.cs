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

    public Order(MenuData menu, int noodle, List<int> toppings, OrderTemplateDatabase db)
    {
        menuData = menu;
        noodleID = noodle;
        toppingIDs = toppings;
        ordertemplateDB = db;
    }

    public string GenerateOrderMessage(string noodleName, List<string> toppingNames)
    {
        // 1️. 템플릿 랜덤 가져오기
        string menuTemp = ordertemplateDB.GetRandomTemplate("Menu");
        string noodleTemp = ordertemplateDB.GetRandomTemplate("Noodle");
        string toppingTemp = ordertemplateDB.GetRandomTemplate("Topping");

        // 2️. 토핑 문자열 합치기
        string toppingText = string.Join(", ", toppingNames);

        // 3️. 치환
        menuTemp = menuTemp.Replace("{menu}", menuData.menuName);
        noodleTemp = noodleTemp.Replace("{noodle}", noodleName);
        toppingTemp = toppingTemp.Replace("{topping}", toppingText);

        // 4️. 주문하기
        return menuTemp + "\n" + noodleTemp+ " " + toppingTemp;
    }

    public string GetOrderText(IngredientDatabase ingredientDB)
    {
        string noodleName = ingredientDB.GetIngredientByID(noodleID).name;

        List<string> toppingNames = toppingIDs
            .Select(id => ingredientDB.GetIngredientByID(id).name)
            .ToList();

        return GenerateOrderMessage(noodleName, toppingNames);
    }

    public HashSet<int> GetIngredientSet()
    {
        HashSet<int> result = new HashSet<int>();

        // 1. 메뉴 기본 재료
        foreach (int id in menuData.IngredientsID)
            result.Add(id);

        // 2. 면
        result.Add(noodleID);

        // 3. 토핑
        foreach (int id in toppingIDs)
            result.Add(id);

        return result;
    }

    public float Price(IngredientDatabase ingredientDB)
    {
        float total = 0f;

        // 메뉴 기본 재료 가격
        foreach (int id in menuData.IngredientsID)
            total += ingredientDB.GetIngredientByID(id).price;

        // 면 가격
        total += ingredientDB.GetIngredientByID(noodleID).price;

        // 토핑 가격
        foreach (int id in toppingIDs)
            total += ingredientDB.GetIngredientByID(id).price;

        return total;
    }

    public float Ingredient_Cost(IngredientDatabase ingredientDB)
    {
        float total = 0f;

        // 메뉴 기본 재료 비용
        foreach (int id in menuData.IngredientsID)
            total += ingredientDB.GetIngredientByID(id).ingredientCost;

        // 면 비용
        total += ingredientDB.GetIngredientByID(noodleID).ingredientCost;

        // 토핑 비용
        foreach (int id in toppingIDs)
            total += ingredientDB.GetIngredientByID(id).ingredientCost;

        return total;
    }
}
