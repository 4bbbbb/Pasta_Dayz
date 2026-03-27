using System.Collections.Generic;
using UnityEngine;
using static Cooker_Oven;
using static IInteractableScript;

public class Cooker_Trashcan : MonoBehaviour, IInteractable
{
    public bool isSelected => false;
    public bool CanBeSelected => false;

    private Animator anim;

    [Header("애니메이션 트리거 이름")]
    [SerializeField] private string trashTriggerName = "Trash";

    private bool isPlayingAnim = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public bool Interact(IInteractable target)
    {
        if (target is Noodles_CookedNoodle cookedNoodle)
        {
            TrashCookedNoodle(cookedNoodle);
            return true;
        }

        if (target is Cooker_FryingPan fryingPan)
        {
            TrashFryingPan(fryingPan);
            return true;
        }

        if (target is FinishedPasta pasta)
        {
            TrashFinishedPasta(pasta);
            return true;
        }

        if (target is Burned burned)
        {
            TrashBurnedFood(burned);
            return true;
        }

        if (target is Plate_BakedPane bakedPane)
        {
            TrashBakedPane(bakedPane);
            return true;
        }

        if (target is Plates_BasicPlate basicPlate)
        {
            TrashBasicPlate(basicPlate);
            return true;
        }

        if (target is Plates_OvenPlate ovenPlate)
        {
            TrashOvenPlate(ovenPlate);
            return true;
        }

        if (target is BakedPasta bakedPasta)
        {
            TrashBakedPasta(bakedPasta);
            return true;
        }

        return false;
    }

    private void PlayTrashAnimation()
    {
        if (anim == null) return;
        if (isPlayingAnim) return;

        anim.SetTrigger(trashTriggerName);
        StartCoroutine(WaitForAnimation());
    }

    private System.Collections.IEnumerator WaitForAnimation()
    {
        isPlayingAnim = true;
        yield return new WaitForSeconds(0.31f);
        isPlayingAnim = false;
    }

    private void TrashCookedNoodle(Noodles_CookedNoodle cookedNoodle)
    {
        if (cookedNoodle == null || cookedNoodle.isBeingTrashed)
            return;

        ClearKitchenSelection(cookedNoodle);
        PlayTrashAnimation();

        float totalCost = cookedNoodle.GetCost();

        if (Gold_Manager.Instance != null && totalCost > 0f)
            Gold_Manager.Instance.SpendBusinessCost(totalCost);

        Debug.Log($"삶은 면 버림. 재료비 {totalCost} 차감");

        cookedNoodle.OnTrashed();
        cookedNoodle.PlayTrashEffect(transform);
    }

    private void TrashFryingPan(Cooker_FryingPan fryingPan)
    {
        if (fryingPan == null || fryingPan.isBeingTrashed)
            return;

        ClearKitchenSelection(fryingPan);
        PlayTrashAnimation();

        float totalCost = fryingPan.GetPanContentCost();

        if (Gold_Manager.Instance != null && totalCost > 0f)
            Gold_Manager.Instance.SpendBusinessCost(totalCost);

        Debug.Log($"팬 통째로 버림. 재료비 {totalCost} 차감");

        fryingPan.OnTrashed();
        fryingPan.PlayTrashEffect(transform);
    }

    private void TrashFinishedPasta(FinishedPasta pasta)
    {
        if (pasta == null || pasta.isBeingTrashed)
            return;

        ClearKitchenSelection(pasta);
        PlayTrashAnimation();

        float totalCost = CalculateIngredientCost(pasta.GetIngredientSet());

        if (Gold_Manager.Instance != null && totalCost > 0f)
            Gold_Manager.Instance.SpendBusinessCost(totalCost);

        Debug.Log($"완성 파스타를 버렸습니다. 재료비 {totalCost} 차감");

        pasta.OnTrashed();
        pasta.PlayTrashEffect(transform);
    }

    private void TrashBurnedFood(Burned burned)
    {
        if (burned == null || burned.isBeingTrashed)
            return;

        ClearKitchenSelection(burned);
        PlayTrashAnimation();

        float totalCost = burned.GetCost();

        if (Gold_Manager.Instance != null && totalCost > 0f)
            Gold_Manager.Instance.SpendBusinessCost(totalCost);

        Debug.Log($"탄 음식 버림. 재료비 {totalCost} 차감");

        burned.OnTrashed();
        burned.PlayTrashEffect(transform);
    }

    private void TrashBakedPane(Plate_BakedPane bakedPane)
    {
        if (bakedPane == null || bakedPane.isBeingTrashed)
            return;

        ClearKitchenSelection(bakedPane);
        PlayTrashAnimation();

        float totalCost = bakedPane.GetCost();

        if (Gold_Manager.Instance != null && totalCost > 0f)
            Gold_Manager.Instance.SpendBusinessCost(totalCost);

        Debug.Log($"구워진 빠네 버림. 재료비 {totalCost} 차감");

        bakedPane.OnTrashed();
        bakedPane.PlayTrashEffect(transform);
    }

    private void TrashBasicPlate(Plates_BasicPlate basicPlate)
    {
        if (basicPlate == null || basicPlate.isBeingTrashed)
            return;

        ClearKitchenSelection(basicPlate);
        PlayTrashAnimation();

        float totalCost = basicPlate.GetCost();

        if (Gold_Manager.Instance != null && totalCost > 0f)
            Gold_Manager.Instance.SpendBusinessCost(totalCost);

        Debug.Log($"기본 접시 버림. 재료비 {totalCost} 차감");

        basicPlate.OnTrashed();
        basicPlate.PlayTrashEffect(transform);
    }

    private void TrashOvenPlate(Plates_OvenPlate ovenPlate)
    {
        if (ovenPlate == null || ovenPlate.isBeingTrashed)
            return;

        ClearKitchenSelection(ovenPlate);
        PlayTrashAnimation();

        ovenPlate.OnTrashed();
        ovenPlate.PlayTrashEffect(transform);
    }

    private void TrashBakedPasta(BakedPasta bakedPasta)
    {
        if (bakedPasta == null || bakedPasta.isBeingTrashed)
            return;

        ClearKitchenSelection(bakedPasta);
        PlayTrashAnimation();

        float totalCost = bakedPasta.GetCost();

        if (Gold_Manager.Instance != null && totalCost > 0f)
            Gold_Manager.Instance.SpendBusinessCost(totalCost);

        Debug.Log($"구워진 파스타 버림. 재료비 {totalCost} 차감");

        bakedPasta.OnTrashed();
        bakedPasta.PlayTrashEffect(transform);
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
                total += data.ingredientCost;
        }

        return total;
    }

    private void ClearKitchenSelection(IInteractable target)
    {
        Kitchen_Manager km = FindObjectOfType<Kitchen_Manager>();
        if (km != null)
            km.ClearSelectionForTrashed(target);
    }

    public void Cancel()
    {
    }
}