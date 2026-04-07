using UnityEngine;

public class Pane_Manager : MonoBehaviour
{
    private void OnEnable()
    {
        RefreshPane();
    }

    private void Start()
    {
        RefreshPane();
    }

    public void RefreshPane()
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

        var pane = IngredientDatabase.Instance.ingredientList.Find(i => i.id == 601);

        if (pane != null && pane.isUnlocked)
        {
            Transform paneObj = transform.Find("Pane_601");

            if (paneObj != null)
                paneObj.gameObject.SetActive(true);
        }
    }
}