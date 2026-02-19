using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IngredientDatabase : MonoBehaviour
{
    public List<IngredientData> ingredientList = new List<IngredientData>();

    void Awake()
    {
        LoadIngredientData();
    }

    void LoadIngredientData()
    {
        var data = CSVReader.Read("Data/IngredientsData");

        foreach (var row in data)
        {
            int id = int.Parse(row["ID"].ToString());
            string category = row["Category"].ToString();
            string name = row["Name"].ToString();
            float price = float.TryParse(row["Price"]?.ToString(), out var p) ? p : 0f;
            float cost = float.TryParse(row["Ingredient_Cost"]?.ToString(), out var c) ? c : 0f;
            int level = int.TryParse(row["Unlock_Level"]?.ToString(), out var l) ? l : 0;
            float unlockCost = float.TryParse(row["Unlock_Cost"]?.ToString(), out var u) ? u : 0f;
            bool isUnlocked = row["isUnlocked"]?.ToString() == "1";

            ingredientList.Add(new IngredientData(id, category, name, price, cost, level, unlockCost, isUnlocked));
        }
    }


    // 🔎 ID로 찾기
    public IngredientData GetIngredientByID(int id)
    {
        return ingredientList.Find(i => i.id == id);
    }

    // 🎲 랜덤 면
    public int GetRandomNoodle()
    {
        List<IngredientData> noodles = ingredientList
           .Where(i => i.category == "Noodle" && i.isUnlocked)
           .ToList();

        if (noodles.Count == 0) return -1;

        int rand = Random.Range(0, noodles.Count);
        return noodles[rand].id;
    }

    // 🎲 랜덤 토핑 여러 개
    public List<int> GetRandomToppings()
    {
        List<int> availableToppings = ingredientList
            .Where(i => i.category == "Topping" && i.isUnlocked)
            .Select(i => i.id)
            .ToList();

        List<int> result = new List<int>();

        int count = Random.Range(1, 3); // 1~2개

        if (availableToppings.Count == 0 || count == 0)
            return result;

        // 리스트 섞기
        for (int i = 0; i < availableToppings.Count; i++)
        {
            int rand = Random.Range(i, availableToppings.Count);
            int temp = availableToppings[i];
            availableToppings[i] = availableToppings[rand];
            availableToppings[rand] = temp;
        }

        // 선택
        for (int i = 0; i < count; i++)
        {
            result.Add(availableToppings[i]);
        }

        return result;
    }
}
