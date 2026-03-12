using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoodleManager : MonoBehaviour
{
    void Start()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        foreach (var item in IngredientDatabase.Instance.ingredientList)
        {
            if (item.isUnlocked && item.categoryType == IngredientData.CategoryType.Noodle)
            {
                GameObject noodleObj = transform.Find($"Noodle_{item.id}")?.gameObject;

                if (noodleObj != null)
                    noodleObj.SetActive(true);
            }
        }
    }    
}
