using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MenuData
{
    public int menuID;
    public string menuName;
    public List<int> IngredientsID;
    public bool isBaked;

    public MenuData(int id, string name, List<int> ingredients, bool oven)
    {
        menuID = id;
        menuName = name;
        IngredientsID = ingredients;
        isBaked = oven;
    }
}