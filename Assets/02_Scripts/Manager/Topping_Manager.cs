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

        // 언락된 토핑 리스트를 가져오고
        // 토핑 그룹의 자식 게임오브젝트 Topping들에 적용

        // 2. 구매한 토핑 리스트 가져오기
        var list = IngredientDatabase.Instance.ingredientList
            .Where(t => t.isUnlocked && t.categoryType == IngredientData.CategoryType.Topping).ToList();

        // 3️. 토핑 개수에 따라 그룹 활성화
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

        // 4️. 그룹 안의 토핑 전부 끄기
        foreach (Transform child in activeGroup.transform)
        {
            child.gameObject.SetActive(false);
        }

        // 5. Unlocked된 토핑만 켜기
        var toppings = activeGroup.GetComponentsInChildren<Topping>(true);

        for (int i = 0; i < list.Count; i++)
        {
            toppings[i].gameObject.SetActive(true);
            IngredientDatabase.IngredientIconData iconData = IngredientDatabase.Instance.GetIngredientIconData(list[i].id);
            toppings[i].Initialize(iconData);
        }
    }
}