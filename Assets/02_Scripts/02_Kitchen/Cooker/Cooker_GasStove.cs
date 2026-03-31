using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Cooker_GasStove : MonoBehaviour, IInteractable
{
    public GameObject fryingPan;
    bool isCooking = false;

    [Header("<< 가스 사운드 >>")]
    [SerializeField] private AudioClip fireOnSound;

    public bool CanBeSelected => false;

    void Awake()
    {
        fryingPan.SetActive(false);
        isCooking = false;
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
            if (!CanClickTutorialGasStove())
                return false;

            Debug.Log("후라이팬이 준비 되었습니다 !");

            isCooking = true;
            fryingPan.SetActive(true);

            Cooker_FryingPan pan = fryingPan.GetComponent<Cooker_FryingPan>();
            if (pan != null)
            {
                pan.PrepareForReuse();
            }

            if (IsFirstKitchenTutorialActive())
            {
                TutorialController.Instance?.TryConsumeKitchenAction(
                    TutorialController.KitchenPracticeTarget.ClickGasStove
                );
            }

            return false;
        }

        return false;
    }

    public void TurnOn()
    {
        if (SoundManager.Instance != null && fireOnSound != null)
        {
            SoundManager.Instance.PlaySFX(fireOnSound);
        }
    }

    public void TurnOff()
    {
    }

    public void DestroyFryingPan()
    {
        isCooking = false;
        fryingPan.SetActive(false);
    }

    public void Cancel()
    {
    }

    private bool IsFirstKitchenTutorialActive()
    {
        return TutorialController.Instance != null
            && TutorialController.Instance.IsTutorialActive
            && TutorialController.Instance.CurrentStep == TutorialController.TutorialStep.Kitchen_FirstCookProgress;
    }

    //private bool CanClickTutorialGasStove()
    //{
    //    if (!IsFirstKitchenTutorialActive())
    //        return true;

    //    if (TutorialController.Instance == null)
    //        return true;

    //    return TutorialController.Instance.IsKitchenActionAllowed(
    //        TutorialController.KitchenPracticeTarget.ClickGasStove
    //    );
    //}
    private bool CanClickTutorialGasStove()
    {
        return true;
    }
}
