using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IngredientData
{
    public int id;
    public string category;      // Noodle, Sauce, Topping, Cheese µî
    public string name;
    public float price;
    public float ingredientCost;
    public int unlockLevel;
    public float unlockCost;
    public bool isUnlocked;

    public IngredientData(
        int id,
        string category,
        string name,
        float price,
        float ingredientCost,
        int unlockLevel,
        float unlockCost,
        bool isUnlocked = false)
    {
        this.id = id;
        this.category = category;
        this.name = name;
        this.price = price;
        this.ingredientCost = ingredientCost;
        this.unlockLevel = unlockLevel;
        this.unlockCost = unlockCost;
        this.isUnlocked = isUnlocked;
    }
}
