using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopPanelUI : MonoBehaviour
{    
    public Transform contentParent;
    void Start()
    {
        if (Shop_Manager.Instance != null)
        {
            Shop_Manager.Instance.InitializeUI(contentParent);
        }
    }
}