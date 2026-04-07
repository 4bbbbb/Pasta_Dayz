using UnityEngine;

public class Plate_Manager : MonoBehaviour
{
    private void OnEnable()
    {
        RefreshPlates();
    }

    private void Start()
    {
        RefreshPlates();
    }

    public void RefreshPlates()
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
            if (item.isUnlocked && item.categoryType == IngredientData.CategoryType.Plate)
            {
                GameObject plateObj = transform.Find($"Plate_{item.id}")?.gameObject;

                if (plateObj != null)
                    plateObj.SetActive(true);
            }
        }
    }
}