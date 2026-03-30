using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSettingUI : MonoBehaviour
{
    public void OnClickResetGameButton()
    {
        if (Game_Manager.Instance != null)
        {
            Game_Manager.Instance.ResetAllProgress();
        }

        SceneManager.LoadScene(0);
    }
}