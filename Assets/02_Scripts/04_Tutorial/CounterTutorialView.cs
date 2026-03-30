using TMPro;
using UnityEngine;

public class CounterTutorialView : MonoBehaviour
{
    [Header("공통")]
    [SerializeField] private GameObject TutorialPanel;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private GameObject nextButton;

    [Header("설명용 UI")]
    [SerializeField] private GameObject dayHighlight;
    [SerializeField] private GameObject satisfactionRoot;
    [SerializeField] private GameObject pauseHighlight;
    [SerializeField] private GameObject pausePanelPreview;

    void Awake()
    {
        if (TutorialController.Instance != null)
            TutorialController.Instance.RegisterCounterView(this);
    }

    void OnDestroy()
    {
        if (TutorialController.Instance != null)
            TutorialController.Instance.UnregisterCounterView(this);
    }

    public void ResetView()
    {
        HideAllIndicators();

        if (TutorialPanel != null) TutorialPanel.SetActive(true);
        if (nextButton != null) nextButton.SetActive(false);
    }

    public void HideAll()
    {
        if (TutorialPanel != null) TutorialPanel.SetActive(false);
        HideAllIndicators();
    }

    public void HideAllIndicators()
    {
        if (dayHighlight != null) dayHighlight.SetActive(false);
        if (satisfactionRoot != null) satisfactionRoot.SetActive(false);
        if (pauseHighlight != null) pauseHighlight.SetActive(false);
        if (pausePanelPreview != null) pausePanelPreview.SetActive(false);
    }

    public void ShowMessage(string msg, bool showNext)
    {
        if (TutorialPanel != null) TutorialPanel.SetActive(true);
        if (messageText != null) messageText.text = msg;
        if (nextButton != null) nextButton.SetActive(showNext);
    }

    public void ShowDayInfo(string msg)
    {
        ShowMessage(msg, true);
        if (dayHighlight != null) dayHighlight.SetActive(true);
    }

    public void ShowSatisfactionInfo(string msg)
    {
        ShowMessage(msg, true);
        if (satisfactionRoot != null) satisfactionRoot.SetActive(true);
    }

    public void ShowPauseInfo(string msg)
    {
        ShowMessage(msg, true);
        if (pauseHighlight != null) pauseHighlight.SetActive(true);
        if (pausePanelPreview != null) pausePanelPreview.SetActive(true);
    }

    public void OnClickNextButton()
    {
        if (TutorialController.Instance != null)
            TutorialController.Instance.OnClickNext();
    }
}