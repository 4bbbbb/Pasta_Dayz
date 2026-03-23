using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Cooker_GasStove : MonoBehaviour, IInteractable
{
    public GameObject fryingPan;
    private bool isFireOn = false;
    bool isCooking = false;

    [Header("<< 가스 사운드 >>")]
    [SerializeField] private AudioClip fireOnSound;

    public bool CanBeSelected => false;

    void Start()
    {
        fryingPan.SetActive(false);
        isFireOn = false;
    }

    public bool Interact(IInteractable target)
    {
        if (isCooking)
        {
            Debug.Log("이미 후라이팬이 있습니다 !");
            return false;
        }

        if (target == null)
        {
            Debug.Log("후라이팬이 준비 되었습니다 !");
            isCooking = true;
            fryingPan.SetActive(true);
            return false;
        }

        return false;
    }

    public void TurnOn()
    {
        isFireOn = true;

        if (SoundManager.Instance != null && fireOnSound != null)
        {
            SoundManager.Instance.PlaySFX(fireOnSound);
        }
    }

    public void TurnOff()
    {
        isFireOn = false;
    }

    public void DestroyFryingPan()
    {
        isCooking = false;
        fryingPan.SetActive(false);
    }

    public void Cancel()
    {
    }
}