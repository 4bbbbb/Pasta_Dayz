using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PastaBox : MonoBehaviour, IHasIngredients
{
    private HashSet<int> finalingredientSet = new HashSet<int>();

    public bool IsBaked { get; private set; }

    public void SetIngredients(HashSet<int> set)
    {
        finalingredientSet = new HashSet<int>(set);
    }

    public HashSet<int> GetIngredientSet()
    {
        return new HashSet<int>(finalingredientSet);
    }

    public void SetBaked(bool baked)
    {
        IsBaked = baked;
    }
}