using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Cooker_PlateTable : MonoBehaviour, IInteractable
{
    [Header("<<그릇 프리팹>>")]
    [SerializeField] private GameObject basicPlatePrefab;
    [SerializeField] private GameObject ovenPlatePrefab;

    [Header("<<그릇 프리팹>>")]
    [SerializeField] private GameObject bakedPastaPrefab;

    [Header("<<스폰위치>>")]
    [SerializeField] private Transform plateSpawnPoint;

    public bool CanBeSelected => false;

    private bool HasPlateOnTable()
    {
        if (plateSpawnPoint == null)
            return false;

        Plates_BasicPlate basicPlate = plateSpawnPoint.GetComponentInChildren<Plates_BasicPlate>(true);
        if (basicPlate != null && !basicPlate.isBeingTrashed)
            return true;

        Plates_OvenPlate ovenPlate = plateSpawnPoint.GetComponentInChildren<Plates_OvenPlate>(true);
        if (ovenPlate != null && !ovenPlate.isBeingTrashed)
            return true;

        return false;
    }

    public bool Interact(IInteractable target)
    {
        if (HasPlateOnTable())
        {
            Debug.Log("이미 그릇이 있습니다!");
            return false;
        }

        if (target == null)
        {
            Debug.Log("그릇을 선택해주세요 !!");
            return true;
        }

        if (target is Plate plate)
        {
            if (!CanAcceptTutorialPlate(plate))
                return false;

            GameObject platePrefab = plate.plateType switch
            {
                Plate.PlateType.BasicPlate => basicPlatePrefab,
                Plate.PlateType.OvenPlate => ovenPlatePrefab,
                _ => null
            };

            if (platePrefab == null)
                return false;

            Debug.Log("그릇이 준비되었어요 !");
            Instantiate(
                platePrefab,
                plateSpawnPoint.position,
                Quaternion.identity,
                plateSpawnPoint
            );

            if (IsFirstKitchenTutorialActive())
            {
                TutorialController.Instance?.TryConsumeKitchenAction(
                    TutorialController.KitchenPracticeTarget.DragPlateToTable
                );
            }

            return true;
        }

        if (target is BakedPasta bakedPasta)
        {
            bakedPasta.transform.SetParent(plateSpawnPoint);
            bakedPasta.transform.position = plateSpawnPoint.position;

            bakedPasta.AddIngredient(502);
            bakedPasta.SetState(BakedPasta.BakedState.Plated);

            return true;
        }

        return false;
    }

    public void ClearPlateTable()
    {
        if (plateSpawnPoint == null)
            return;

        for (int i = plateSpawnPoint.childCount - 1; i >= 0; i--)
            Destroy(plateSpawnPoint.GetChild(i).gameObject);
    }

    private bool IsFirstKitchenTutorialActive()
    {
        return TutorialController.Instance != null
            && TutorialController.Instance.IsTutorialActive
            && TutorialController.Instance.CurrentStep == TutorialController.TutorialStep.Kitchen_FirstCookProgress;
    }

    private bool CanAcceptTutorialPlate(Plate plate)
    {
        if (!IsFirstKitchenTutorialActive())
            return true;

        if (TutorialController.Instance == null)
            return true;

        if (!TutorialController.Instance.IsKitchenActionAllowed(
                TutorialController.KitchenPracticeTarget.DragPlateToTable))
            return false;

        return plate != null && plate.plateType == Plate.PlateType.BasicPlate;
    }

    public void Cancel()
    {
    }
}
