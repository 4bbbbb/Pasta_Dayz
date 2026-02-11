using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Cooker_PlateTable : MonoBehaviour, IInteractable
{
    [Header("<<그릇 프리팹>>")]
    [SerializeField] private GameObject basicPlatePrefab;
    [SerializeField] private GameObject ovenPlatePrefab;
    // 빠네 

    [Header("<<스폰위치>>")]
    [SerializeField] private Transform plateSpawnPoint;

    public bool CanBeSelected => false;

    private Collider tableCollider;

    void Start()
    {
        tableCollider = GetComponent<Collider>();
    }

    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            Debug.Log("그릇을 선택해주세요 !!");   
            return true;
        }        

        if (target is Plate plate)
        {
            GameObject platePrefab = plate.plateType switch
            {
                Plate.PlateType.BasicPlate => basicPlatePrefab,
                Plate.PlateType.OvenPlate => ovenPlatePrefab,
                _ => null
            };

            Debug.Log("그릇이 준비되었어요 !");
            tableCollider.enabled = false;
            Instantiate(
                platePrefab,
                plateSpawnPoint.position,
                Quaternion.identity,
                plateSpawnPoint
            );
            return true;
        }

        return false;
    }

    public void Cancel()
    {

    }
}
