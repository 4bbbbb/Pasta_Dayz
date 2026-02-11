using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class FinishedPasta : MonoBehaviour, IInteractable
{
    [Header("<<후라이팬>>")]
    [SerializeField] private Cooker_FryingPan fryingPan;

    [Header("<<치즈 프리팹>>")]
    [SerializeField] private GameObject parmesanCheesePrefab;
    [SerializeField] private GameObject mozzarellaCheesePrefab;

    [Header("<<치즈 스폰 위치>>")]
    [SerializeField] Transform cheeseSpawnPoint;

    private SpriteRenderer sr;
    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;

    private bool isOnPlate = false;
    private bool hasCheese = false;

    private Cheese.CheeseType? addedCheeseType = null;

    void Start()
    {
        fryingPan = FindObjectOfType<Cooker_FryingPan>();
        sr = GetComponent<SpriteRenderer>();
    }

    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            Debug.Log("완성된 파스타 선택!");
            Select();
            return true;
        }

        if (target is Cheese cheese)
        {
            if (!isOnPlate)
            {
                Debug.Log("그릇 위에 올려진 파스타에만 치즈를 추가할 수 있어요!");
                return false;
            }

            if (hasCheese)
            {
                Debug.Log("이미 치즈가 추가되어 있어요!");
                return false;
            }

            GameObject cheesePrefab = cheese.cheeseType switch
            {
                Cheese.CheeseType.Parmesan => parmesanCheesePrefab,
                Cheese.CheeseType.Mozzarella => mozzarellaCheesePrefab,
                _ => null
            };

            if (cheesePrefab == null)
            {
                return false;
            }

            Instantiate(
                cheesePrefab,
                cheeseSpawnPoint.position,
                Quaternion.identity,
                cheeseSpawnPoint
            );

            hasCheese = true;
            addedCheeseType = cheese.cheeseType;

            return true;
        }

        if (isOnPlate && target is Plates_BasicPlate)
        {
            return false;
        }

        if (isOnPlate && target is Plates_OvenPlate)
        {
            return false;
        }

        return false;
    }
    void Select()
    {
        isSelected = true;
        sr.color = Color.red;
    }
    public void OnMovedToPlate()
    {
        fryingPan.gameObject.SetActive(false);
        isOnPlate = true;
        Debug.Log("완성된 파스타를 그릇에 담았어요 !!");
    }
    public bool IsOnOvenPlate()
    {
        return isOnPlate && GetComponentInParent<Plates_OvenPlate>() != null;
    }

    public bool HasMozzarella()
    {
        return addedCheeseType == Cheese.CheeseType.Mozzarella;
    }

    public void Cancel()
    {
        isSelected = false;
        sr.color = Color.white;
    }
}
