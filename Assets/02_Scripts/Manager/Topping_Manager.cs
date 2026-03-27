﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ToppingManager : MonoBehaviour
{
    public GameObject sixToppingGroup;
    public GameObject tenToppingGroup;
    public GameObject thirteenToppingGroup;

    void Start()
    {
        sixToppingGroup.SetActive(false);
        tenToppingGroup.SetActive(false);
        thirteenToppingGroup.SetActive(false);
        
        List<IngredientData> list = IngredientDatabase.Instance.ingredientList
            .Where(t => t.isUnlocked && t.categoryType == IngredientData.CategoryType.Topping).ToList();

        list.AddRange(IngredientDatabase.Instance.ingredientList.Where(t => t.isUnlocked && t.id == 402));

        GameObject activeGroup;

        if (list.Count <= 6)
        {
            activeGroup = sixToppingGroup;
        }
        else if (list.Count <= 10)
        {
            activeGroup = tenToppingGroup;
        }
        else
        {
            activeGroup = thirteenToppingGroup;
        }
        activeGroup.SetActive(true);

        foreach (Transform child in activeGroup.transform)
        {
            child.gameObject.SetActive(false);
        }

        Topping[] toppings = activeGroup.GetComponentsInChildren<Topping>(true);

        for (int i = 0; i < list.Count; i++)
        {
            toppings[i].gameObject.SetActive(true);
            IngredientDatabase.IngredientIconData iconData = IngredientDatabase.Instance.GetIngredientIconData(list[i].id);
            toppings[i].Initialize(iconData);
        }
    }
}