using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    public Day_Manager dayManager;
    public Image clockImage;    

    void Update()
    {
        if (Day_Manager.Instance == null) return;
        if (!Day_Manager.Instance.isDayActive) return;

        float ratio = Day_Manager.Instance.GetRemainingTime() / Day_Manager.Instance.dayDuration;

        clockImage.fillAmount = ratio;
    }
}
