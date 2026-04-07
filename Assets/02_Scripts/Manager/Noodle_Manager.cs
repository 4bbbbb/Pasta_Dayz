using System.Collections.Generic;
using UnityEngine;

public class Noodle_Manager : MonoBehaviour
{
    [System.Serializable]
    public class NoodleSlot
    {
        public int id;
        public GameObject noodleObj;
        public GameObject emptyObj;
    }

    [SerializeField] private List<NoodleSlot> noodleSlots = new List<NoodleSlot>();

    private void OnEnable()
    {
        RefreshNoodles();
    }

    private void Start()
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
        }
    }
}