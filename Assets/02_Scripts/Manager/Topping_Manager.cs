using System.Linq;
using UnityEngine;

public class ToppingManager : MonoBehaviour
{       
    public GameObject sixToppingGroup;
    public GameObject tenToppingGroup;
    public GameObject thirteenToppingGroup;

    void Start()
    {
        // 1️. 그룹 초기화: 모두 비활성화
        sixToppingGroup.SetActive(false);
        tenToppingGroup.SetActive(false);
        thirteenToppingGroup.SetActive(false);

        // 2️. 구매한 토핑 개수 계산
        int unlockedToppingCount = IngredientDatabase.Instance.ingredientList
            .Count(x => x.isUnlocked && x.categoryType == IngredientData.CategoryType.Topping);

        // 3️. 토핑 개수에 따라 그룹 활성화
        GameObject activeGroup;

        if (unlockedToppingCount <= 6)
        {
            activeGroup = sixToppingGroup;
        }
        else if (unlockedToppingCount <= 10)
        {
            activeGroup = tenToppingGroup;
        }
        else
        {
            activeGroup = thirteenToppingGroup;
        }
        activeGroup.SetActive(true);

        // 4️. 그룹 안의 토핑 전부 끄기
        foreach (Transform child in activeGroup.transform)
        {
            child.gameObject.SetActive(false);
        }

        // 5. Unlocked된 토핑만 켜기
        foreach (var item in IngredientDatabase.Instance.ingredientList)
        {
            if (item.isUnlocked && item.categoryType == IngredientData.CategoryType.Topping)
            {
                GameObject toppingObj = activeGroup.transform.Find($"Topping_{item.id}")?.gameObject;

                if (toppingObj != null)
                    toppingObj.SetActive(true);
            }
        }
    }
}