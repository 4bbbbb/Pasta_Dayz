using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonUI : MonoBehaviour
{
    public void OnClickNextDayBtn()
    {        
        Debug.Log("다음 날 시작!");

        Gold_Manager.Instance.ResetDailyStats();
        Day_Manager.Instance.ResetForNextDay();

        SceneManager.LoadScene(1);        
    }

    public void OnClickHomeBtn()
    {
               

        Gold_Manager.Instance.ResetDailyStats();
        Day_Manager.Instance.ResetForNextDay();

        SceneManager.LoadScene(0);
    }


}
