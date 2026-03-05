using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static IngredientData;

public class IngredientDatabase : MonoBehaviour
{
    public List<IngredientData> ingredientList = new List<IngredientData>();
    public List<IngredientIconData> iconDataList = new List<IngredientIconData>();

    Dictionary<int, IngredientData> ingredientDict;
    Dictionary<int, IngredientIconData> iconDict;

    [System.Serializable]
    public class IngredientIconData
    {
        public int id;
        public Sprite icon;
        public GameObject ingredientPrefab;
    }

    void Awake()
    {
        LoadIngredientData();

        ingredientDict = ingredientList.ToDictionary(i => i.id);
        iconDict = iconDataList.ToDictionary(i => i.id);
    }

    void LoadIngredientData()
    {
        var data = CSVReader.Read("Data/IngredientsData");

        foreach (var row in data)
        {
            int id = int.Parse(row["ID"].ToString());
            //Trim() 코드로 공백오류 보완
            string category = row["Category"].ToString().Trim();
            string name = row["Name"].ToString().Trim();
            float price = float.TryParse(row["Price"]?.ToString(), out var p) ? p : 0f;
            float cost = float.TryParse(row["Ingredient_Cost"]?.ToString(), out var c) ? c : 0f;
            int level = int.TryParse(row["Unlock_Level"]?.ToString(), out var l) ? l : 0;
            float unlockCost = float.TryParse(row["Unlock_Cost"]?.ToString(), out var u) ? u : 0f;
            bool isUnlocked = row["isUnlocked"]?.ToString().Trim() == "1";

            CategoryType categoryType;

            if (!System.Enum.TryParse(category, out categoryType))
            {
                throw new System.Exception($"Invalid Category in CSV: {category}");
            }

            var ingredient = new IngredientData(id,category,name,price,cost,level,unlockCost,isUnlocked);

            ingredient.categoryType = categoryType;

            ingredientList.Add(ingredient);
        }        
    }

    public IngredientData GetIngredient(int id) => ingredientDict.TryGetValue(id, out var data) ? data : null;
    public Sprite GetIcon(int id) => iconDict.TryGetValue(id, out var data) ? data.icon : null;
    public GameObject GetPrefab(int id) => iconDict.TryGetValue(id, out var data) ? data.ingredientPrefab : null;

    public int GetRandomNoodle()
    {
        var noodles = ingredientList
           .Where(i => i.categoryType == CategoryType.Noodle && i.isUnlocked)
           .ToList();

        return noodles.Count == 0 ? -1 : noodles[Random.Range(0, noodles.Count)].id;
    }
       
    public List<int> GetRandomToppings()
    {
        var toppings = ingredientList
            .Where(i => i.categoryType == CategoryType.Topping && i.isUnlocked)
            .Select(i => i.id)
            .OrderBy(x => Random.value)
            .Take(Random.Range(1, 3))
            .ToList();

        return toppings;
    }

    public void UpdateUnlockState(int id, bool state)
    {
        if (ingredientDict.TryGetValue(id, out var data))
        {
            data.isUnlocked = state;
        }
    }  
}
