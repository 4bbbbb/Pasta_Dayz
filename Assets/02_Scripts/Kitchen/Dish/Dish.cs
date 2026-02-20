using System.Collections.Generic;
using UnityEngine;

public class Dish : MonoBehaviour, IHasIngredients
{
    private HashSet<int> ingredientSet = new HashSet<int>();

    public void AddIngredient(int id)
    {
        ingredientSet.Add(id);
    }

    public HashSet<int> GetIngredientSet()
    {
        return ingredientSet;
    }
}