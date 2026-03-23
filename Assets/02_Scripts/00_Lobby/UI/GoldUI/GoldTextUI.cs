using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GoldTextUI : MonoBehaviour
{
    public TextMeshProUGUI goldText;

    void Start()
    {
        if (Gold_Manager.Instance != null && goldText != null)
        {
            Gold_Manager.Instance.SetUIText(goldText);
        }
    }    
}
