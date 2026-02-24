using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomerButton : MonoBehaviour
{
    public void OnClickYesBtn()
    {
        OrderManager manager = FindObjectOfType<OrderManager>();
        if (manager != null)
        {
            manager.GoToKitchen();
        }
        else
        {
            Debug.LogError("OrderManager를 찾을 수 없음!");
        }
    }
}
