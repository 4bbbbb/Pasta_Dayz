using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plate_Manager : MonoBehaviour
{    
    void Start()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        foreach (var item in IngredientDatabase.Instance.ingredientList)
        {
            if (item.isUnlocked && item.categoryType == IngredientData.CategoryType.Plate)
            {
                GameObject plateObj = transform.Find($"Plate_{item.id}")?.gameObject;

                if (plateObj != null)
                    plateObj.SetActive(true);
            }
        }
    }   
}
