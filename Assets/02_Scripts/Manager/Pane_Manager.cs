using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pane_Manager : MonoBehaviour
{   
    void Start()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        var pane = IngredientDatabase.Instance.ingredientList.Find(i => i.id == 601);

        if (pane != null && pane.isUnlocked)
        {
            Transform paneObj = transform.Find("Pane_601");

            if (paneObj != null)
                paneObj.gameObject.SetActive(true);
        }
    }
}
