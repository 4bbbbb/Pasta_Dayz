using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Cooker_FryingPan : MonoBehaviour, IInteractable
{
    [SerializeField] private Cooker_GasStove gasStove;
    [SerializeField] private Transform toppingSpawnPoint;
    [SerializeField] private Transform noodleSpawnPoint;

    private SpriteRenderer sr;

    private bool hasOil = false;
    private bool isCooking = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public bool Interact(IInteractable target)
    {
        if (isCooking) return false;


        if (target is Topping topping)
        {
            if (topping.isOliveOil)
            {
                hasOil = true;
                gasStove.TurnOn();
            }

            Instantiate(
                topping.toppingPrefab,
                toppingSpawnPoint.position,
                Quaternion.identity,
                toppingSpawnPoint
            );

            return true;
        }

        if (target is Noodles_CookedNoodle cookedNoodle)
        {
            if (!hasOil) return false;

            Instantiate(cookedNoodle.cookedNoodlePrefab, noodleSpawnPoint.position, Quaternion.identity, transform);

            Destroy(cookedNoodle.gameObject);

            StartCoroutine(CookRoutine());
            return true;
        }

        return false;
    }

    IEnumerator CookRoutine()
    {
        isCooking = true;
        sr.color = Color.red;

        yield return new WaitForSeconds(4f);

        isCooking = false;
        sr.color = Color.white;
        gasStove.TurnOff();
    }

    public void Cancel()
    {

    }

}
    
