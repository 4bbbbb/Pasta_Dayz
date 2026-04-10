using System.Collections.Generic;
using UnityEngine;

public class NoodleManager : MonoBehaviour
{
    [System.Serializable]
    public class NoodleSlot
    {
        public int id;                  // 예: 102
        public GameObject noodleObj;    // 구매 후 보일 오브젝트
        public GameObject emptyObj;     // 구매 전 보일 빈 오브젝트
    }

    [SerializeField] private List<NoodleSlot> noodleSlots = new List<NoodleSlot>();

    void Start()
    {
        RefreshNoodles();
    }

    public void RefreshNoodles()
    {
        if (IngredientDatabase.Instance == null)
        {
            Debug.LogError("IngredientDatabase.Instance가 없습니다.");
            return;
        }

        foreach (var slot in noodleSlots)
        {
            bool unlocked = false;

            foreach (var item in IngredientDatabase.Instance.ingredientList)
            {
                if (item.id == slot.id && item.categoryType == IngredientData.CategoryType.Noodle)
                {
                    unlocked = item.isUnlocked;
                    break;
                }
            }

            if (slot.noodleObj != null)
                slot.noodleObj.SetActive(unlocked);

            if (slot.emptyObj != null)
                slot.emptyObj.SetActive(!unlocked);

            Debug.Log($"Noodle ID {slot.id} / unlocked = {unlocked}");
        }
    }
}