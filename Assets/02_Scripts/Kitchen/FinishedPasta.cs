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

    [Header("<<치즈 스폰 위치>>")]
    [SerializeField] Transform cheeseSpawnPoint;

    private SpriteRenderer sr;
    public bool isSelected { get; private set; }

    public bool CanBeSelected => true;
    private bool isOnPlate = false;

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

        if (target is Cheese_ParmesanCheese cheese)
        {
            if (!isOnPlate)
            {
                Debug.Log("그릇 위에 올려진 파스타에만 치즈를 추가할 수 있어요!");
                return false;
            }

            // 치즈 프리팹 생성
            Instantiate(
                parmesanCheesePrefab,
                cheeseSpawnPoint.position,
                Quaternion.identity,
                transform
            );

            Destroy(cheese.gameObject); // 치즈 소비
            return true;
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
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Plates_BasicPlate>() != null)
        {
            isOnPlate = true;
            Debug.Log("파스타&그릇 TriggerEnter");
        }
    }

    public void Cancel()
    {
        isSelected = false;
        sr.color = Color.white;
    }
}
