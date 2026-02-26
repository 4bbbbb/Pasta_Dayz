using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    public DayManager dayManager;
    public Image clockImage;    

    void Update()
    {
        if (DayManager.Instance == null) return;
        if (!DayManager.Instance.isDayActive) return;

        float ratio = DayManager.Instance.GetRemainingTime() / DayManager.Instance.dayDuration;

        clockImage.fillAmount = ratio;
    }
}
