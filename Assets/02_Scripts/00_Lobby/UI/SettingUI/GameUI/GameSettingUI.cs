using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSettingUI : MonoBehaviour
{
    [Header("Reset ¹öÆ°")]
    [SerializeField] private RectTransform resetButton;

    [Header("SFX")]
    [SerializeField] private AudioClip buttonClickSFX;

    [Header("Á©¸® È¿°ú")]
    [SerializeField] private float pressX = 0.96f;
    [SerializeField] private float pressY = 0.94f;

    [Header("µô·¹ÀÌ")]
    [SerializeField] private float actionDelay = 0.3f;

    private Vector3 originalScale;
    private bool isProcessing = false;

    private void Awake()
    {
        if (resetButton != null)
            originalScale = resetButton.localScale;
    }

    public void OnClickResetGameButton()
    {
        if (isProcessing)
            return;

        StartCoroutine(ResetGameRoutine());
    }

    private IEnumerator ResetGameRoutine()
    {
        isProcessing = true;

        PlayButtonFeedback();

        yield return new WaitForSeconds(actionDelay);

        if (Game_Manager.Instance != null)
        {
            Game_Manager.Instance.ResetAllProgress();
        }

        SceneManager.LoadScene(0);
    }

    private void PlayButtonFeedback()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(buttonClickSFX);
        }

        if (resetButton == null)
            return;

        resetButton.DOKill();
        resetButton.localScale = originalScale;

        Sequence seq = DOTween.Sequence();
        seq.Append(
            resetButton.DOScale(
                new Vector3(originalScale.x * pressX, originalScale.y * pressY, originalScale.z),
                0.1f
            ).SetEase(Ease.OutCubic)
        );
        seq.Append(
            resetButton.DOScale(originalScale, 0.14f).SetEase(Ease.OutQuad)
        );
    }

    private void OnDestroy()
    {
        if (resetButton != null)
            resetButton.DOKill();
    }
}