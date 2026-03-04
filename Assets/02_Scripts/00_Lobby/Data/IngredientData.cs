using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IngredientData
{
    public int id;
    public string category;
    public CategoryType categoryType;
    public string name;
    public float price;
    public float ingredientCost;
    public int unlockLevel;
    public float unlockCost;
    public bool isUnlocked;
    public Sprite icon;

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

    public enum CategoryType
    {
        Noodle,
        Sauce,
        Topping,
        Cheese,
        Plate,
        Pane,
        Parsley
    }
}
