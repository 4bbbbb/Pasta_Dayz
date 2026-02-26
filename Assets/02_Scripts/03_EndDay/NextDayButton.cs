using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextDayButton : MonoBehaviour
{
    public void OnClickNextDayBtn()
    {
        Gold_Manager.Instance.ResetDailyStats();

        Debug.Log("다음 날 시작!");

        Gold_Manager.Instance.ResetDailyStats();
        DayManager.Instance.ResetForNextDay();

        SceneManager.LoadScene(1);        
        
    }


}
