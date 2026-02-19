using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Order
{
    public MenuData menuData;
    public int noodleID;
    public List<int> toppingIDs;

    OrderTemplateDatabase ordertemplateDB;

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
}
