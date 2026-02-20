using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuDatabase : MonoBehaviour
{
    public List<MenuData> menuList = new List<MenuData>();

    void Awake()
    {
        LoadMenuData();
    }

    void LoadMenuData()
    {
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

    public MenuData GetRandomMenu()
    {
        return menuList[Random.Range(0, menuList.Count)];
    }
}