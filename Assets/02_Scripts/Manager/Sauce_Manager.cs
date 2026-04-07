using UnityEngine;

public class Sauce_Manager : MonoBehaviour
{
    private void OnEnable()
    {
        RefreshSauces();
    }

    private void Start()
    {
        RefreshSauces();
    }

    public void RefreshSauces()
    {
        if (IngredientDatabase.Instance == null)
        {
            Debug.LogError("IngredientDatabase.Instance가 없습니다.");
            return;
        }

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