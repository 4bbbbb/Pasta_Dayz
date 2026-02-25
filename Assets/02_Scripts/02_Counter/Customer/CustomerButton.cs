using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomerButton : MonoBehaviour
{
    public void OnClickYesBtn()
    {
        OrderManager manager = FindObjectOfType<OrderManager>();

        if (manager == null)
        {
            Debug.LogError("OrderManager를 찾을 수 없음!");
            return;
        }

        if (manager.dayManager.isTakingOrder)
        {
            // Price 계산과 Gold 누적, 씬 전환까지 OrderManager.GetPrice()가 처리
            manager.GetPrice();
        }
        else
        {
            Debug.Log("영업 종료! 주문 불가");
        }
    }
}
