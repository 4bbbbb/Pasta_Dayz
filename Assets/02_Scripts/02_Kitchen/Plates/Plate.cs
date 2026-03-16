using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;
using static Topping;

public class Plate : MonoBehaviour, IInteractable
{
    [Header("<<기본 그릇 스프라이트>>")]
    [SerializeField] private Sprite original501Sprite;
    [SerializeField] private Sprite selected501Sprite;

    [Header("<<오븐 그릇 스프라이트>>")]
    [SerializeField] private Sprite original502Sprite;
    [SerializeField] private Sprite selected502Sprite;

    private SpriteRenderer sr;
    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;

    public PlateType plateType;
    public enum PlateType
    {
        BasicPlate,
        OvenPlate,
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        isSelected = false;

        if (plateType == PlateType.BasicPlate)
        {
            sr.sprite = original501Sprite;
        }

        if (plateType == PlateType.OvenPlate)
        {
            sr.sprite = original502Sprite;
        }
    }
    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            Select();
            return true;
        }
        return false;
    }

    void Select()
    {
        isSelected = true;

        if (plateType == PlateType.BasicPlate)
        {
            sr.sprite = selected501Sprite;
        }

        if (plateType == PlateType.OvenPlate)
        {
            sr.sprite = selected502Sprite;
        }
    }
    public void Cancel()
    {
        isSelected = false;

        if (plateType == PlateType.BasicPlate)
        {
            sr.sprite = original501Sprite;
        }

        if (plateType == PlateType.OvenPlate)
        {
            sr.sprite = original502Sprite;
        }
    }
}
