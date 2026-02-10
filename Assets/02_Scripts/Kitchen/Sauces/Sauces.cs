using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Sauces : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;
    public bool isSelected { get; private set; }
    public GameObject saucePrefab;
    public SauceType sauceType;

    public bool CanBeSelected => true;

    public enum SauceType
    {
        None,
        Tomato,
        Cream,
        Rose,    // 토마토 + 크림 섞인 결과물
                 // 나중에 더 추가 가능
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        saucePrefab = gameObject;
    }

    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            Debug.Log("소스 선택!");
            Select();
            return true;
        }

        return false;        
    }

    void Select()
    {
        isSelected = true;
        sr.color = Color.yellow;
    }

    public void Cancel()
    {
        isSelected = false;
        sr.color = Color.white;
    }

}
