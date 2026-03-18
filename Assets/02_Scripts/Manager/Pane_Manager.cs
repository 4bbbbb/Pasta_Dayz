using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pane_Manager : MonoBehaviour
{   
    void Start()
    {        
        gameObject.SetActive(false);       

        var pane = IngredientDatabase.Instance.ingredientList.Find(i => i.id == 601);

        if (pane != null && pane.isUnlocked)
        {
            transform.Find("Pane_601")?.gameObject.SetActive(true);
        }
    }
}
