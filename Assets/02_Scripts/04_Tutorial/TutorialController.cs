using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialController : MonoBehaviour
{
    public static TutorialController Instance;

    public enum TutorialStep
    {
        None,

        // Counter Intro
        Counter_Welcome,
        Counter_DayIcon,
        Counter_Satisfaction,
        Counter_Pause,

        // First order
        Counter_FirstCustomerSpawn,
        Counter_FirstOrderExplain,
        Counter_FirstYesExplain,
        Counter_FirstAutoExplain,
        Counter_FirstWaitYesClick,

        // First kitchen
        Kitchen_FirstIntro,
        Kitchen_FirstCookProgress,
        Kitchen_FirstCookDone_ReturnToCounter,

        // Back to counter
        Counter_FirstServeResult,
        Counter_SecondOrderIntro,

        // Second order
        Counter_SecondCustomerSpawn,
        Counter_SecondOrderExplain,
        Counter_SecondWaitYesClick,

        // Second kitchen
        Kitchen_SecondIntro,
        Kitchen_SecondCookProgress,
        Kitchen_SecondCookDone_ReturnToCounter,

        // Finish
        Counter_SecondServeResult,
        Counter_FinishExplain,
        Counter_StartRealDay1,

        Completed
    }

    [SerializeField] private MenuDatabase tutorialMenuDB;
    [SerializeField] private OrderTemplateDatabase tutorialTemplateDB;

    [Header("현재 상태")]
    [SerializeField] private bool isTutorialActive = false;
    [SerializeField] private TutorialStep currentStep = TutorialStep.None;

    [Header("씬 캐시")]
    private CounterTutorialView counterView;
    private KitchenTutorialView kitchenView;

    [Header("플래그")]
    private bool waitingForNextButton = false;
    private bool waitingForYesButton = false;
    private bool waitingForKitchenComplete = false;
    private bool pendingServeSuccess = false;
    private bool hasPendingServeResult = false;

    private const string KEY_TUTORIAL_COMPLETED = "TUTORIAL_COMPLETED";

    public bool IsTutorialActive => isTutorialActive;
    public TutorialStep CurrentStep => currentStep;
    public bool IsCompleted => PlayerPrefs.GetInt(KEY_TUTORIAL_COMPLETED, 0) == 1;

    private bool IsCounterScene =>
        SceneManager.GetActiveScene().name == "01_Counter" || SceneManager.GetActiveScene().buildIndex == 1;

    private bool IsKitchenScene =>
        SceneManager.GetActiveScene().name == "02_Kitchen" || SceneManager.GetActiveScene().buildIndex == 2;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // 로비에서 튜토리얼 보기 선택 시 호출
    public void StartTutorial()
    {
        isTutorialActive = true;
        currentStep = TutorialStep.Counter_Welcome;

        waitingForNextButton = false;
        waitingForYesButton = false;
        waitingForKitchenComplete = false;
        hasPendingServeResult = false;
        pendingServeSuccess = false;

        Debug.Log("[Tutorial] StartTutorial");
    }

    public void NotifyCounterSceneReady()
    {
        if (!isTutorialActive)
            return;

        RunCurrentStep();
    }

    public void RegisterCounterView(CounterTutorialView view)
    {
        counterView = view;
        if (isTutorialActive && IsCounterScene)
        {
            RunCurrentStep();
        }
    }

    public void UnregisterCounterView(CounterTutorialView view)
    {
        if (counterView == view)
            counterView = null;
    }

    public void RegisterKitchenView(KitchenTutorialView view)
    {
        kitchenView = view;
        if (isTutorialActive && IsKitchenScene)
        {
            RunCurrentStep();
        }
    }

    public void UnregisterKitchenView(KitchenTutorialView view)
    {
        if (kitchenView == view)
            kitchenView = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isTutorialActive)
            return;

        StartCoroutine(CoWaitAndRunStep());
    }

    private IEnumerator CoWaitAndRunStep()
    {
        yield return null;
        RunCurrentStep();
    }

    public bool ShouldBlockNormalOrderFlow()
    {
        return isTutorialActive && currentStep != TutorialStep.Completed;
    }

    public bool ShouldBlockAutoCook()
    {
        if (!isTutorialActive) return false;

        // 첫 주문 튜토리얼에서는 자동완성 막기
        if (currentStep == TutorialStep.Counter_FirstAutoExplain ||
            currentStep == TutorialStep.Counter_FirstWaitYesClick)
            return true;

        return false;
    }

    public void OnClickNext()
    {
        if (!isTutorialActive || !waitingForNextButton)
            return;

        waitingForNextButton = false;
        AdvanceStep();
    }

    public void OnClickCounterYes()
    {
        if (!isTutorialActive || !waitingForYesButton)
            return;

        waitingForYesButton = false;

        switch (currentStep)
        {
            case TutorialStep.Counter_FirstWaitYesClick:
                currentStep = TutorialStep.Kitchen_FirstIntro;
                Order_Manager.Instance.GetPrice(); // 기존 흐름 재사용
                break;

            case TutorialStep.Counter_SecondWaitYesClick:
                currentStep = TutorialStep.Kitchen_SecondIntro;
                Order_Manager.Instance.GetPrice(); // 기존 흐름 재사용
                break;
        }
    }

    // 주방에서 요리 완료 후 카운터로 돌아가기 직전에 호출
    public void OnKitchenDishCompleted()
    {
        if (!isTutorialActive)
            return;

        waitingForKitchenComplete = false;

        switch (currentStep)
        {
            case TutorialStep.Kitchen_FirstCookProgress:
                currentStep = TutorialStep.Kitchen_FirstCookDone_ReturnToCounter;
                SceneManager.LoadScene("01_Counter");
                break;

            case TutorialStep.Kitchen_SecondCookProgress:
                currentStep = TutorialStep.Kitchen_SecondCookDone_ReturnToCounter;
                SceneManager.LoadScene("01_Counter");
                break;
        }
    }

    // 손님 제출 연출 끝나고 Order_Manager가 알려줄 콜백
    public void NotifyServeSequenceFinished(bool success)
    {
        if (!isTutorialActive)
            return;

        hasPendingServeResult = true;
        pendingServeSuccess = success;

        switch (currentStep)
        {
            case TutorialStep.Kitchen_FirstCookDone_ReturnToCounter:
                currentStep = TutorialStep.Counter_FirstServeResult;
                break;

            case TutorialStep.Kitchen_SecondCookDone_ReturnToCounter:
                currentStep = TutorialStep.Counter_SecondServeResult;
                break;
        }

        RunCurrentStep();
    }

    private void AdvanceStep()
    {
        switch (currentStep)
        {
            case TutorialStep.Counter_Welcome:
                currentStep = TutorialStep.Counter_DayIcon;
                break;

            case TutorialStep.Counter_DayIcon:
                currentStep = TutorialStep.Counter_Satisfaction;
                break;

            case TutorialStep.Counter_Satisfaction:
                currentStep = TutorialStep.Counter_Pause;
                break;

            case TutorialStep.Counter_Pause:
                currentStep = TutorialStep.Counter_FirstCustomerSpawn;
                break;

            case TutorialStep.Counter_FirstCustomerSpawn:
                currentStep = TutorialStep.Counter_FirstOrderExplain;
                break;

            case TutorialStep.Counter_FirstOrderExplain:
                currentStep = TutorialStep.Counter_FirstYesExplain;
                break;

            case TutorialStep.Counter_FirstYesExplain:
                currentStep = TutorialStep.Counter_FirstAutoExplain;
                break;

            case TutorialStep.Counter_FirstAutoExplain:
                currentStep = TutorialStep.Counter_FirstWaitYesClick;
                break;

            case TutorialStep.Counter_FirstServeResult:
                currentStep = TutorialStep.Counter_SecondOrderIntro;
                break;

            case TutorialStep.Counter_SecondOrderIntro:
                currentStep = TutorialStep.Counter_SecondCustomerSpawn;
                break;

            case TutorialStep.Counter_SecondCustomerSpawn:
                currentStep = TutorialStep.Counter_SecondOrderExplain;
                break;

            case TutorialStep.Counter_SecondOrderExplain:
                currentStep = TutorialStep.Counter_SecondWaitYesClick;
                break;

            case TutorialStep.Counter_SecondServeResult:
                currentStep = TutorialStep.Counter_FinishExplain;
                break;

            case TutorialStep.Counter_FinishExplain:
                currentStep = TutorialStep.Counter_StartRealDay1;
                break;

            case TutorialStep.Counter_StartRealDay1:
                FinishTutorialAndStartRealDay1();
                return;
        }

        RunCurrentStep();
    }

    private void RunCurrentStep()
    {
        if (!isTutorialActive)
            return;

        if (IsCounterScene)
        {
            RunCounterStep();
        }
        else if (IsKitchenScene)
        {
            RunKitchenStep();
        }
    }

    private void RunCounterStep()
    {
        if (counterView == null)
            return;

        counterView.ResetView();

        switch (currentStep)
        {
            case TutorialStep.Counter_Welcome:
                waitingForNextButton = true;
                counterView.ShowMessage(
                    $"어서오세요 {GetPlayerName()} 사장님!\n오늘은 첫 영업 전에 기본 진행 방법을 알려드릴게요.",
                    true
                );
                break;

            case TutorialStep.Counter_DayIcon:
                waitingForNextButton = true;
                counterView.ShowDayInfo(
                    "이 아이콘은 하루를 의미해요.\n시간이 다 지나면 하루 영업이 종료되고 정산을 합니다.\n종료되기 전에만 주문을 받으면 조리 중 하루가 끝나도 끝까지 조리가 가능하니 걱정마세요."
                );
                break;

            case TutorialStep.Counter_Satisfaction:
                waitingForNextButton = true;
                counterView.ShowSatisfactionInfo(
                    "이 아이콘은 손님의 만족도를 의미해요.\n만족도에 따라 받을 수 있는 팁이 달라지니 손님이 실망하지 않게 최대한 빠르게 만들어봐요."
                );
                break;

            case TutorialStep.Counter_Pause:
                waitingForNextButton = true;
                counterView.ShowPauseInfo(
                    "이 아이콘을 누르면 게임을 잠시 멈출 수 있어요.\n" +
                    "1) 여기선 게임 내 설정을 변경할 수 있어요.\n" +
                    "2) 레시피가 헷갈리면 레시피북을 확인해보세요.\n" +
                    "3) 홈 버튼으로 종료할 수 있지만 저장되지 않아요.\n" +
                    "4) 화면 아무데나 누르면 이어서 진행할 수 있어요."
                );
                break;

            case TutorialStep.Counter_FirstCustomerSpawn:
                SpawnFirstTutorialCustomer();
                waitingForNextButton = true;
                counterView.ShowMessage("이제 첫 손님을 받아볼게요.", true);
                break;

            case TutorialStep.Counter_FirstOrderExplain:
                waitingForNextButton = true;
                counterView.ShowMessage("손님이 알리오 올리오, 스파게티면, 토마토 토핑 추가를 주문했어요.", true);
                Order_ManagerBridge_ShowDecisionButtons(true, true, false, false);
                break;

            case TutorialStep.Counter_FirstYesExplain:
                waitingForNextButton = true;
                counterView.ShowMessage("네 버튼을 누르면 주방으로 넘어가서 조리를 시작해요.", true);
                Order_ManagerBridge_ShowDecisionButtons(true, true, false, false);
                break;

            case TutorialStep.Counter_FirstAutoExplain:
                waitingForNextButton = true;
                counterView.ShowMessage("자동완성 버튼은 $5가 소모돼요.\n복잡한 메뉴가 들어오면 사용해보는 걸 추천해요.\n이번 튜토리얼 첫 주문은 직접 만들어볼게요.", true);
                Order_ManagerBridge_ShowDecisionButtons(true, true, false, false);
                break;

            case TutorialStep.Counter_FirstWaitYesClick:
                waitingForYesButton = true;
                counterView.ShowMessage("네를 누르고 파스타를 만들러 가볼까요?", false);
                Order_ManagerBridge_ShowDecisionButtons(true, true, true, false);
                break;

            case TutorialStep.Counter_FirstServeResult:
                waitingForNextButton = true;
                counterView.ShowMessage(
                    pendingServeSuccess
                        ? "손님에게 만든 파스타를 전달했어요.\n손님이 만족해 하시면서 팁을 주시네요. 팁을 많이 받을 수 있도록 노력해봐요."
                        : "이번에는 손님이 만족하지 못했어요.\n그래도 괜찮아요. 다음 주문에서 더 잘해보면 돼요.",
                    true
                );
                break;

            case TutorialStep.Counter_SecondOrderIntro:
                waitingForNextButton = true;
                counterView.ShowMessage("이번에는 토마토 파스타를 만들어볼까요?", true);
                break;

            case TutorialStep.Counter_SecondCustomerSpawn:
                SpawnSecondTutorialCustomer();
                waitingForNextButton = true;
                counterView.ShowMessage("두 번째 손님이 들어왔어요.", true);
                break;

            case TutorialStep.Counter_SecondOrderExplain:
                waitingForNextButton = true;
                counterView.ShowMessage("손님이 토마토 파스타, 스파게티면, 토마토 토핑, 마늘 토핑을 주문했어요.", true);
                Order_ManagerBridge_ShowDecisionButtons(true, true, false, false);
                break;

            case TutorialStep.Counter_SecondWaitYesClick:
                waitingForYesButton = true;
                counterView.ShowMessage("이번에도 네를 눌러 주방으로 가서 조리해볼게요.", false);
                Order_ManagerBridge_ShowDecisionButtons(true, true, true, false);
                break;

            case TutorialStep.Counter_SecondServeResult:
                waitingForNextButton = true;
                counterView.ShowMessage("좋아요. 이런 식으로 주문을 받고, 조리하고, 제출하면 돼요.", true);
                break;

            case TutorialStep.Counter_FinishExplain:
                waitingForNextButton = true;
                counterView.ShowMessage("이제 실제 게임을 시작해보아요.", true);
                break;

            case TutorialStep.Counter_StartRealDay1:
                waitingForNextButton = true;
                counterView.ShowMessage("다음 버튼을 누르면 1일차 실제 게임이 시작됩니다.", true);
                break;
        }
    }

    private void RunKitchenStep()
    {
        if (kitchenView == null)
            return;

        kitchenView.ResetView();

        switch (currentStep)
        {
            case TutorialStep.Kitchen_FirstIntro:
                waitingForNextButton = false;
                waitingForKitchenComplete = true;
                currentStep = TutorialStep.Kitchen_FirstCookProgress;
                kitchenView.StartFirstKitchenTutorial();
                break;

            case TutorialStep.Kitchen_FirstCookProgress:
                kitchenView.ResumeFirstKitchenTutorial();
                break;

            case TutorialStep.Kitchen_SecondIntro:
                waitingForNextButton = false;
                waitingForKitchenComplete = true;
                currentStep = TutorialStep.Kitchen_SecondCookProgress;
                kitchenView.StartSecondKitchenTutorial();
                break;

            case TutorialStep.Kitchen_SecondCookProgress:
                kitchenView.ResumeSecondKitchenTutorial();
                break;
        }
    }

    private void SpawnFirstTutorialCustomer()
    {
        Order firstOrder = CreateFirstTutorialOrder();
        Order_ManagerBridge_SpawnTutorialCustomer(
            firstOrder,
            0,
            "알리오 올리오\n스파게티면, 토마토 토핑 추가"
        );
    }

    private void SpawnSecondTutorialCustomer()
    {
        Order secondOrder = CreateSecondTutorialOrder();
        Order_ManagerBridge_SpawnTutorialCustomer(
            secondOrder,
            1,
            "토마토 파스타\n스파게티면, 토마토 토핑, 마늘 토핑 추가"
        );
    }

    private Order CreateFirstTutorialOrder()
    {
        MenuData aglioMenu = tutorialMenuDB.GetMenuByID(201);
        if (aglioMenu == null)
        {
            Debug.LogError("튜토리얼용 알리오 올리오 메뉴를 찾지 못함");
            return null;
        }

        int spaghettiID = 101;
        List<int> toppings = new List<int> { 301 };

        return new Order(aglioMenu, spaghettiID, toppings, tutorialTemplateDB);
    }

    private Order CreateSecondTutorialOrder()
    {
        MenuData tomatoMenu = tutorialMenuDB.GetMenuByID(202);
        if (tomatoMenu == null)
        {
            Debug.LogError("튜토리얼용 토마토 파스타 메뉴를 찾지 못함");
            return null;
        }

        int spaghettiID = 101;
        List<int> toppings = new List<int> { 301, 302 };

        return new Order(tomatoMenu, spaghettiID, toppings, tutorialTemplateDB);
    }

    private string GetPlayerName()
    {
        if (Game_Manager.Instance != null && !string.IsNullOrWhiteSpace(Game_Manager.Instance.currentNickname))
            return Game_Manager.Instance.currentNickname;

        return "Player";
    }

    private void FinishTutorialAndStartRealDay1()
    {
        isTutorialActive = false;
        currentStep = TutorialStep.Completed;

        PlayerPrefs.SetInt(KEY_TUTORIAL_COMPLETED, 1);
        PlayerPrefs.Save();

        hasPendingServeResult = false;
        pendingServeSuccess = false;
        waitingForNextButton = false;
        waitingForYesButton = false;
        waitingForKitchenComplete = false;

        if (counterView != null)
            counterView.HideAll();

        if (kitchenView != null)
            kitchenView.HideAll();

        if (Order_Manager.Instance != null)
            Order_Manager.Instance.ResetForTutorialToRealGame();

        // 여기에 Day1 시작 코드 추가
    }

    #region Bridge Methods
    // 여기 아래는 Order_Manager에 최소 메서드 추가해두면 깔끔해진다.

    private void Order_ManagerBridge_ShowDecisionButtons(bool showYes, bool showAuto, bool enableYes, bool enableAuto)
    {
        if (Order_Manager.Instance == null) return;
        Order_Manager.Instance.ShowTutorialDecisionButtons(showYes, showAuto, enableYes, enableAuto);
    }

    private void Order_ManagerBridge_SpawnTutorialCustomer(Order order, int spriteIndex, string forcedMessage)
    {
        if (Order_Manager.Instance == null) return;
        Order_Manager.Instance.SpawnTutorialCustomer(order, spriteIndex, forcedMessage);
    }
    #endregion
}