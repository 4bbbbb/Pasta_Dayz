using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoldTextUI : MonoBehaviour
{
    public Text goldText;
    
    void Start()
    {
        if (Gold_Manager.Instance != null && goldText != null)
        {
            Gold_Manager.Instance.SetUIText(goldText);
        }
    }    
}
