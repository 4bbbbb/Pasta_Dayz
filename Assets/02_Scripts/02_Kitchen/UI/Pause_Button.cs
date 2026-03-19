using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_Button : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("일시정지 창")]
    [SerializeField] private GameObject pausePanel;

    void Start()
    {
        ResumeGame(); 
    }

    public void TogglePause()
    {
        if (IsPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }            
    }

    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);

        }
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);

        }
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
        IsPaused = false;
    }
}