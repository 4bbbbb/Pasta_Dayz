using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PastaBox : MonoBehaviour, IHasIngredients
{
    private HashSet<int> finalingredientSet;

    // 이 메서드를 사용해 finalSet을 DeliveryBox에 넣어준다.
    public void SetIngredients(HashSet<int> set)
    {
        finalingredientSet = new HashSet<int>(set);
    }

    // GetIngredientSet을 통해 finalSet을 가져올 수 있다.
    public HashSet<int> GetIngredientSet()
    {
        return finalingredientSet;
    }

}
