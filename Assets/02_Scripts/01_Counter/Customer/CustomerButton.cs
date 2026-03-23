using System.Collections;
using UnityEngine;

public class CustomerButton : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField] private AudioClip clickSFX;

    [Header("딜레이")]
    [SerializeField] private float clickDelay = 0.3f;

    private bool isProcessing = false;

    public void OnClickYesBtn()
    {
        if (isProcessing) return;

        isProcessing = true;
        PlayClickSFX();
        StartCoroutine(OnClickYesBtnRoutine());
    }

    public void OnClickAutoButton()
    {
        if (isProcessing) return;

        isProcessing = true;
        PlayClickSFX();
        StartCoroutine(OnClickAutoButtonRoutine());
    }

    private IEnumerator OnClickYesBtnRoutine()
    {
        yield return new WaitForSeconds(clickDelay);

        Order_Manager manager = FindObjectOfType<Order_Manager>();

        if (manager == null)
        {
            Debug.LogError("OrderManager를 찾을 수 없음!");
            isProcessing = false;
            yield break;
        }

        if (manager.dayManager != null && manager.dayManager.isTakingOrder)
        {
            manager.GetPrice();
        }
        else
        {
            Debug.Log("영업 종료! 주문 불가");
            isProcessing = false;
        }
    }

    private IEnumerator OnClickAutoButtonRoutine()
    {
        yield return new WaitForSeconds(clickDelay);

        Order_Manager manager = FindObjectOfType<Order_Manager>();

        if (manager == null)
        {
            Debug.LogError("OrderManager를 찾을 수 없음!");
            isProcessing = false;
            yield break;
        }

        if (manager.dayManager != null && manager.dayManager.isTakingOrder)
        {
            manager.OnClickAutoButton();
        }
        else
        {
            Debug.Log("영업 종료! 자동 주문 불가");
            isProcessing = false;
        }
    }

    void PlayClickSFX()
    {
        if (SoundManager.Instance != null && clickSFX != null)
        {
            SoundManager.Instance.PlaySFX(clickSFX);
        }
    }
}