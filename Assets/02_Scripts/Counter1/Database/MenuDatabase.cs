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
        var csv = Resources.Load<TextAsset>("Data/MenuData");
        var lines = csv.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var values = lines[i].Split(',');

            int id = int.Parse(values[0]);
            string name = values[1];
            var ingredientList = values[2].Replace("\"", "").Split(',').Select(int.Parse).ToList();
            bool isBaked = false;
            if (values.Length > 3 && int.TryParse(values[3].Trim(), out int bakedInt))
            {
                isBaked = bakedInt == 1;
            }

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