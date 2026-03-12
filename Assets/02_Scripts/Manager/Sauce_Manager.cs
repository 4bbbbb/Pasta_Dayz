using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sauce_Manager : MonoBehaviour
{    
    void Start()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        foreach (var item in IngredientDatabase.Instance.ingredientList)
        {
            if (item.isUnlocked && item.categoryType == IngredientData.CategoryType.Sauce)
            {
                GameObject sauceObj = transform.Find($"Sauce_{item.id}")?.gameObject;

                if (sauceObj != null)
                    sauceObj.SetActive(true);
            }
        }
    }
}
