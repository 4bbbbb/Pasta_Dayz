using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Lobby_UI_Manager : MonoBehaviour
{
    [Header("시작 버튼")]
    [SerializeField] private RectTransform startButton;

    [Header("열기 버튼")]
    [SerializeField] private RectTransform settingOpenButton;
    [SerializeField] private RectTransform shopOpenButton;
    [SerializeField] private RectTransform profileOpenButton;

    [Header("닫기 버튼")]
    [SerializeField] private RectTransform settingCloseButton;
    [SerializeField] private RectTransform shopCloseButton;
    [SerializeField] private RectTransform profileCloseButton;

    [Header("버튼 젤리")]
    [SerializeField] private float pressX = 0.96f;
    [SerializeField] private float pressY = 0.94f;

    [Header("패널 등장/퇴장")]
    [SerializeField] private float buttonAnimDelay = 0.3f;
    [SerializeField] private float panelStartScale = 0.98f;
    [SerializeField] private float panelFadeDuration = 0.4f;
    [SerializeField] private float panelCloseDuration = 0.3f;

    [Header("Setting")]
    [SerializeField] private CanvasGroup settingCanvasGroup;
    [SerializeField] private RectTransform settingPanel;

    [Header("Shop")]
    [SerializeField] private CanvasGroup shopCanvasGroup;
    [SerializeField] private RectTransform shopPanel;

    [Header("Profile")]
    [SerializeField] private CanvasGroup profileCanvasGroup;
    [SerializeField] private RectTransform profilePanel;

    [Header("닉네임 표시")]
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private string defaultNickname = "Player";

    [Header("닉네임 변경 패널")]
    [SerializeField] private CanvasGroup nicknameChangeCanvasGroup;
    [SerializeField] private RectTransform nicknameChangePanel;
    [SerializeField] private TMP_InputField nicknameInputField;
    [SerializeField] private GameObject nicknameCancelButtonObject;

    [Header("튜토리얼 선택 패널")]
    [SerializeField] private CanvasGroup tutorialChoiceCanvasGroup;
    [SerializeField] private RectTransform tutorialChoicePanel;

    [Header("사운드")]
    [SerializeField] private AudioClip buttonClickSFX;

    private Dictionary<RectTransform, Vector3> originalScales = new Dictionary<RectTransform, Vector3>();

    private bool isForcedNicknameFlow = false;
    private bool isStartingGame = false;

    private const string KEY_FIRST_START_FLOW_DONE = "FIRST_START_FLOW_DONE";
    private const string KEY_SHOULD_PLAY_TUTORIAL = "SHOULD_PLAY_TUTORIAL";

    void Awake()
    {
        CacheButtonScale(startButton);
        CacheButtonScale(settingOpenButton);
        CacheButtonScale(shopOpenButton);
        CacheButtonScale(profileOpenButton);

        CacheButtonScale(settingCloseButton);
        CacheButtonScale(shopCloseButton);
        CacheButtonScale(profileCloseButton);

        InitPanel(settingCanvasGroup, settingPanel);
        InitPanel(shopCanvasGroup, shopPanel);
        InitPanel(profileCanvasGroup, profilePanel);
        InitPanel(nicknameChangeCanvasGroup, nicknameChangePanel);
        InitPanel(tutorialChoiceCanvasGroup, tutorialChoicePanel);

        InitNickname();
    }

    void Start()
    {
        if (Game_Manager.Instance != null && nicknameText != null)
        {
            nicknameText.text = Game_Manager.Instance.currentNickname;
        }

        SyncNicknameUI();
    }

    void OnEnable()
    {
        SyncNicknameUI();
    }

    public void SyncNicknameUI()
    {
        if (nicknameText == null) return;
        if (Game_Manager.Instance == null) return;

        nicknameText.text = Game_Manager.Instance.currentNickname;
    }

    void InitNickname()
    {
        if (nicknameText != null && string.IsNullOrWhiteSpace(nicknameText.text))
        {
            nicknameText.text = defaultNickname;
        }

        if (nicknameInputField != null)
        {
            nicknameInputField.text = "";
            nicknameInputField.characterLimit = 8;
        }
    }

    void CacheButtonScale(RectTransform button)
    {
        if (button == null) return;

        if (!originalScales.ContainsKey(button))
            originalScales.Add(button, button.localScale);
    }

    void InitPanel(CanvasGroup cg, RectTransform panel)
    {
        if (cg == null || panel == null)
            return;

        cg.gameObject.SetActive(false);
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        panel.localScale = Vector3.one * panelStartScale;
    }

    public void OnClickButtonSFXOnly()
    {
        PlayButtonClickSFXOnly();
    }

    void PlayButtonClickSFXOnly()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(buttonClickSFX);
        }
    }

    void PlayButtonJelly(RectTransform target)
    {
        if (target == null)
            return;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(buttonClickSFX);
        }

        if (!originalScales.TryGetValue(target, out Vector3 originalScale))
            originalScale = target.localScale;

        target.DOKill();
        target.localScale = originalScale;

        Sequence seq = DOTween.Sequence();

        seq.Append(
            target.DOScale(
                new Vector3(originalScale.x * pressX, originalScale.y * pressY, originalScale.z),
                0.1f
            ).SetEase(Ease.OutCubic)
        );

        seq.Append(
            target.DOScale(originalScale, 0.14f).SetEase(Ease.OutQuad)
        );
    }

    bool HasCompletedFirstStartFlow()
    {
        return PlayerPrefs.GetInt(KEY_FIRST_START_FLOW_DONE, 0) == 1;
    }

    void SetFirstStartFlowDone(bool shouldPlayTutorial)
    {
        PlayerPrefs.SetInt(KEY_FIRST_START_FLOW_DONE, 1);
        PlayerPrefs.SetInt(KEY_SHOULD_PLAY_TUTORIAL, shouldPlayTutorial ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool ShouldPlayTutorial()
    {
        return PlayerPrefs.GetInt(KEY_SHOULD_PLAY_TUTORIAL, 0) == 1;
    }

    public void OnClickStartButton()
    {
        bool firstStartDone = PlayerPrefs.GetInt("FIRST_START_FLOW_DONE", 0) == 1;

        if (!firstStartDone)
        {
            OpenNicknameChangePanelForced();
            return;
        }

        StartCoroutine(LoadSceneAfterDelay());
    }

    private IEnumerator HandleFirstStartFlow()
    {
        isStartingGame = true;

        PlayButtonJelly(startButton);
        yield return new WaitForSeconds(0.3f);

        OpenNicknameChangePanelForced();

        isStartingGame = false;
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        isStartingGame = true;

        PlayButtonJelly(startButton);
        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene(1);
    }

    private IEnumerator LoadSceneAfterChoiceDelay()
    {
        isStartingGame = true;
        yield return new WaitForSeconds(0.15f);
        SceneManager.LoadScene(1);
    }

    public void OpenSettingWithDelay()
    {
        PlayButtonJelly(settingOpenButton);
        StartOpen(settingCanvasGroup, settingPanel);
    }

    public void CloseSettingWithDelay()
    {
        PlayButtonJelly(settingCloseButton);
        StartClose(settingCanvasGroup, settingPanel);
    }

    public void OpenShopWithDelay()
    {
        PlayButtonJelly(shopOpenButton);
        StartOpen(shopCanvasGroup, shopPanel);
    }

    public void CloseShopWithDelay()
    {
        PlayButtonJelly(shopCloseButton);
        StartClose(shopCanvasGroup, shopPanel);
    }

    public void OpenProfileWithDelay()
    {
        PlayButtonJelly(profileOpenButton);
        StartOpen(profileCanvasGroup, profilePanel);
    }

    public void CloseProfileWithDelay()
    {
        PlayButtonJelly(profileCloseButton);
        StartClose(profileCanvasGroup, profilePanel);
    }

    public void OpenNicknameChangePanel()
    {
        OpenNicknameChangePanelInternal(false);
    }

    private void OpenNicknameChangePanelForced()
    {
        OpenNicknameChangePanelInternal(true);
    }

    private void OpenNicknameChangePanelInternal(bool forced)
    {
        PlayButtonClickSFXOnly();

        isForcedNicknameFlow = forced;

        if (nicknameCancelButtonObject != null)
        {
            nicknameCancelButtonObject.SetActive(!forced);
        }

        if (nicknameInputField != null)
        {
            if (Game_Manager.Instance != null)
                nicknameInputField.text = Game_Manager.Instance.currentNickname;
            else if (nicknameText != null)
                nicknameInputField.text = nicknameText.text;

            nicknameInputField.ActivateInputField();
            nicknameInputField.Select();
        }

        StartOpen(nicknameChangeCanvasGroup, nicknameChangePanel);
    }

    public void ConfirmNicknameChange()
    {
        PlayButtonClickSFXOnly();

        if (nicknameText == null || nicknameInputField == null)
            return;

        string newNickname = nicknameInputField.text.Trim();

        if (string.IsNullOrEmpty(newNickname))
            return;

        if (newNickname.Length > 8)
            return;

        nicknameText.text = newNickname;

        if (Game_Manager.Instance != null)
        {
            Game_Manager.Instance.SetNickname(newNickname);
            Game_Manager.Instance.SaveGame();
        }

        SyncNicknameUI();

        bool shouldOpenTutorialChoice = isForcedNicknameFlow && !HasCompletedFirstStartFlow();

        StartClose(nicknameChangeCanvasGroup, nicknameChangePanel);

        if (shouldOpenTutorialChoice)
        {
            isForcedNicknameFlow = false;
            StartCoroutine(OpenTutorialChoiceAfterNicknameClose());
        }
        else
        {
            isForcedNicknameFlow = false;
            if (nicknameCancelButtonObject != null)
                nicknameCancelButtonObject.SetActive(true);
        }
    }

    private IEnumerator OpenTutorialChoiceAfterNicknameClose()
    {
        yield return new WaitForSeconds(buttonAnimDelay + panelCloseDuration + 0.05f);

        if (nicknameCancelButtonObject != null)
            nicknameCancelButtonObject.SetActive(true);

        StartOpen(tutorialChoiceCanvasGroup, tutorialChoicePanel);
    }

    public void CancelNicknameChange()
    {
        PlayButtonClickSFXOnly();

        // 처음 강제 닉네임 입력 흐름에서는 취소 막기
        if (isForcedNicknameFlow)
            return;

        if (nicknameInputField != null && nicknameText != null)
        {
            nicknameInputField.text = nicknameText.text;
        }

        StartClose(nicknameChangeCanvasGroup, nicknameChangePanel);
    }

    public void OnClickViewTutorialButton()
    {
        if (TutorialController.Instance != null)
            TutorialController.Instance.StartTutorial();

        PlayerPrefs.SetInt("FIRST_START_FLOW_DONE", 1);
        PlayerPrefs.SetInt("SHOULD_PLAY_TUTORIAL", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene(1);
    }

    public void OnClickSkipTutorialButton()
    {
        PlayerPrefs.SetInt("FIRST_START_FLOW_DONE", 1);
        PlayerPrefs.SetInt("SHOULD_PLAY_TUTORIAL", 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene(1);
    }

    private void CompleteFirstStartAndLoad(bool shouldPlayTutorial)
    {
        SetFirstStartFlowDone(shouldPlayTutorial);

        if (Game_Manager.Instance != null)
        {
            Game_Manager.Instance.SaveGame();
        }

        StartClose(tutorialChoiceCanvasGroup, tutorialChoicePanel);
        StartCoroutine(LoadSceneAfterChoiceDelay());
    }

    void StartOpen(CanvasGroup cg, RectTransform panel)
    {
        if (cg == null || panel == null) return;

        DOVirtual.DelayedCall(buttonAnimDelay, () =>
        {
            if (cg == null || panel == null) return;
            if (cg.gameObject == null || panel.gameObject == null) return;

            cg.DOKill();
            panel.DOKill();

            cg.gameObject.SetActive(true);
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
            panel.localScale = Vector3.one * panelStartScale;

            cg.DOFade(1f, panelFadeDuration).SetLink(cg.gameObject, LinkBehaviour.KillOnDestroy);
            panel.DOScale(Vector3.one, panelFadeDuration)
                .SetLink(panel.gameObject, LinkBehaviour.KillOnDestroy)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    if (cg == null || panel == null) return;
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                });
        }).SetLink(gameObject, LinkBehaviour.KillOnDestroy);
    }

    void StartClose(CanvasGroup cg, RectTransform panel)
    {
        if (cg == null || panel == null) return;

        DOVirtual.DelayedCall(buttonAnimDelay, () =>
        {
            if (cg == null || panel == null) return;
            if (cg.gameObject == null || panel.gameObject == null) return;

            cg.DOKill();
            panel.DOKill();

            cg.interactable = false;
            cg.blocksRaycasts = false;

            cg.DOFade(0f, panelCloseDuration).SetLink(cg.gameObject, LinkBehaviour.KillOnDestroy);
            panel.DOScale(Vector3.one * panelStartScale, panelCloseDuration)
                .SetLink(panel.gameObject, LinkBehaviour.KillOnDestroy)
                .SetEase(Ease.InCubic)
                .OnComplete(() =>
                {
                    if (cg == null) return;
                    cg.gameObject.SetActive(false);
                });
        }).SetLink(gameObject, LinkBehaviour.KillOnDestroy);
    }

    void OnDestroy()
    {
        DOTween.Kill(gameObject);
    }
}