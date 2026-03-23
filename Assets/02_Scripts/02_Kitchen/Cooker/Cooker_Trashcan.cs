using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Cooker_Trashcan : MonoBehaviour, IInteractable
{
    public bool isSelected => false;
    public bool CanBeSelected => false;

    public bool Interact(IInteractable target)
    {
        if (target is FinishedPasta pasta)
        {
            TrashFinishedPasta(pasta);
            return true;
        }

        return false;
    }

    private void TrashFinishedPasta(FinishedPasta pasta)
    {
        if (pasta == null || pasta.isBeingTrashed)
            return;

        float totalCost = CalculateIngredientCost(pasta.GetIngredientSet());

        if (Gold_Manager.Instance != null && totalCost > 0f)
        {
            Gold_Manager.Instance.SpendBusinessCost(totalCost);
        }

        Debug.Log($"완성 파스타를 버렸습니다. 재료비 {totalCost} 차감");

        pasta.OnTrashed();
    }

    private float CalculateIngredientCost(HashSet<int> ingredientIDs)
    {
        if (ingredientIDs == null || IngredientDatabase.Instance == null)
            return 0f;

        float total = 0f;

        foreach (int id in ingredientIDs)
        {
            IngredientData data = IngredientDatabase.Instance.GetIngredient(id);

            if (data != null)
            {
                total += data.ingredientCost;
            }
        }

        return total;
    }

    public void Cancel()
    {
        
    }
}