using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_Button : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("일시정지 창")]
    [SerializeField] private GameObject pausePanel;

    [Header("클릭 복귀 딜레이")]
    [SerializeField] private float resumeClickDelay = 0.15f;

    [Header("SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSFX;

    private float pauseOpenedTime;

    void Start()
    {
        ResumeGame();
    }

    void Update()
    {
        if (!IsPaused) return;

        // 일시정지 버튼 누른 직후 같은 클릭으로 바로 resume 되는 것 방지
        if (Time.unscaledTime - pauseOpenedTime < resumeClickDelay) return;

        if (Input.GetMouseButtonDown(0))
        {
            ResumeGame();
            return;
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            ResumeGame();
            return;
        }
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
        pauseOpenedTime = Time.unscaledTime;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        PlayClickSFX();

        Time.timeScale = 0f;
    }

    void PlayClickSFX()
    {
        if (audioSource != null && clickSFX != null)
        {
            audioSource.PlayOneShot(clickSFX);
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