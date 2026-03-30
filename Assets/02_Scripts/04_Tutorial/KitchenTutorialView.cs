using TMPro;
using UnityEngine;

public class KitchenTutorialView : MonoBehaviour
{
    [Header("공통")]
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text messageText;

    [Header("강조 오브젝트")]
    [SerializeField] private GameObject noodleHighlight;
    [SerializeField] private GameObject sauceHighlight;
    [SerializeField] private GameObject toppingHighlight;
    [SerializeField] private GameObject cookHighlight;
    [SerializeField] private GameObject plateHighlight;
    [SerializeField] private GameObject passHighlight;

    void Awake()
    {
        if (TutorialController.Instance != null)
            TutorialController.Instance.RegisterKitchenView(this);
    }

    void OnDestroy()
    {
        if (TutorialController.Instance != null)
            TutorialController.Instance.UnregisterKitchenView(this);
    }

    public void ResetView()
    {
        if (root != null) root.SetActive(true);
        HideAllHighlights();
    }

    public void HideAll()
    {
        if (root != null) root.SetActive(false);
        HideAllHighlights();
    }

    private void HideAllHighlights()
    {
        if (noodleHighlight != null) noodleHighlight.SetActive(false);
        if (sauceHighlight != null) sauceHighlight.SetActive(false);
        if (toppingHighlight != null) toppingHighlight.SetActive(false);
        if (cookHighlight != null) cookHighlight.SetActive(false);
        if (plateHighlight != null) plateHighlight.SetActive(false);
        if (passHighlight != null) passHighlight.SetActive(false);
    }

    public void StartFirstKitchenTutorial()
    {
        ResetView();
        if (messageText != null)
            messageText.text = "첫 번째 주문을 만들어볼게요. 튜토리얼 순서대로 진행해보세요.";

        // 여기서 첫 주문용 단계 시작
        // 예: 면만 선택 가능, 소스/토핑은 잠금 등
    }

    public void ResumeFirstKitchenTutorial()
    {
        // 씬 다시 바인딩됐을 때 이어서 표시할 내용
    }

    public void StartSecondKitchenTutorial()
    {
        ResetView();
        if (messageText != null)
            messageText.text = "이번에는 두 번째 주문을 직접 만들어볼게요.";

        // 여기서 두 번째 주문용 단계 시작
    }

    public void ResumeSecondKitchenTutorial()
    {
        // 필요하면 이어서 표시
    }

    // 주방에서 마지막 완성 후 카운터로 돌아갈 때 호출
    public void NotifyDishCompleted()
    {
        if (TutorialController.Instance != null)
            TutorialController.Instance.OnKitchenDishCompleted();
    }
}