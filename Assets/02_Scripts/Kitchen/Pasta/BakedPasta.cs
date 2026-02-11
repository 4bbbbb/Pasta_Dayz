using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;
using static Sauces;

public class BakedPasta : MonoBehaviour, IInteractable
{
    [Header("<<파슬리 프리팹>>")]
    [SerializeField] private GameObject parsleyPrefab;

    [Header("<<파슬리 스폰 위치>>")]
    [SerializeField] Transform parsleySpawnPoint; 

    private SpriteRenderer sr;
    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            Debug.Log("잘 구워진 파스타 선택!");
            Select();
            return true;
        }

        if (target is Topping_Parsley parsley)
        {
            Debug.Log("파슬리를 뿌렸어요");
            Instantiate(
                parsleyPrefab,
                parsleySpawnPoint.position,
                Quaternion.identity,
                parsleySpawnPoint
                );
            return true;
        }

        return false;
    }

    void Select()
    {
        isSelected = true;
        sr.color = Color.red;
    }

    public void Cancel()
    {
        isSelected = false;
        sr.color = Color.white;
    }
}
