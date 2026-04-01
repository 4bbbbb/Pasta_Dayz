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

        Counter_Welcome,
        Counter_DayIcon,
        Counter_DayIcon2,
        Counter_Satisfaction,
        Counter_Pause,
        Counter_Setting,
        Counter_Book,
        Counter_Home,
        Counter_Resume,

        Counter_FirstCustomerSpawn,
        Counter_FirstOrderExplain,
        Counter_FirstYesExplain,
        Counter_FirstAutoExplain,
        Counter_FirstAutoExplain2,
        Counter_FirstWaitYesClick,

        Kitchen_FirstIntro,
        Kitchen_FirstCookProgress,
        Kitchen_FirstCookDone_ReturnToCounter,

        Counter_FirstServeResult,
        Counter_FirstServeReaction,
        Counter_FirstServeMoneyExplain,
        Counter_FirstServeCheer,
        Counter_FirstServeCheer2,

        Completed
    }

    public enum CounterPracticeTarget
    {
        None,
        Pause,
        Setting,
        Book,
        Home,
        Resume
    }

    public enum KitchenPracticeTarget
    {
        None,
        DragSpaghettiToCooker,
        ClickGasStove,
        DragOilToPan,
        DragGarlicToPan,
        DragCookedNoodleToPan,
        DragPlateToTable,
        DragPastaToPlate,
        DragParmesanToPlate,
        DragParsleyToPlate,
        DragPlateToPassTable
    }

    private enum FirstKitchenGuideStep
    {
        None,
        Intro,
        DragNoodle,
        NoodleBoilingInfo,
        ClickGasStove,
        DragOil,
        OilInfo,
        DragGarlic,
        GarlicInfo,
        DragCookedNoodle,
        CookStartInfo,
        FinishedInfo,
        DragPlate,
        PlateSpawnedInfo,
        DragPastaToPlate,
        PlatedInfo,
        DragParmesan,
        DragParsley,
        DragPassTable,
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
    private bool waitingForKitchenPractice = false;

    [Header("카운터")]
    private bool waitingForCounterPractice = false;
    private CounterPracticeTarget expectedCounterPractice = CounterPracticeTarget.None;

    [Header("키친")]
    private KitchenPracticeTarget expectedKitchenPractice = KitchenPracticeTarget.None;
    private FirstKitchenGuideStep firstKitchenGuideStep = FirstKitchenGuideStep.None;

    private const string KEY_TUTORIAL_COMPLETED = "TUTORIAL_COMPLETED";
    private const string KEY_SHOULD_PLAY_TUTORIAL = "SHOULD_PLAY_TUTORIAL";

    public bool IsTutorialActive => isTutorialActive;
    public TutorialStep CurrentStep => currentStep;
    public bool IsCompleted => PlayerPrefs.GetInt(KEY_TUTORIAL_COMPLETED, 0) == 1;

    private bool IsCounterScene =>
        SceneManager.GetActiveScene().name == "01_Counter" || SceneManager.GetActiveScene().buildIndex == 1;

    private bool IsKitchenScene =>
        SceneManager.GetActiveScene().name == "02_Kitchen" || SceneManager.GetActiveScene().buildIndex == 2;

    private void Awake()
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

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void StartTutorial()
    {
        isTutorialActive = true;
        currentStep = TutorialStep.Counter_Welcome;

        waitingForNextButton = false;
        waitingForYesButton = false;
        waitingForKitchenComplete = false;
        hasPendingServeResult = false;
        pendingServeSuccess = false;
        ResetCounterPracticeState();
        ResetKitchenPracticeState();
        firstKitchenGuideStep = FirstKitchenGuideStep.None;

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
            RunCurrentStep();
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
            RunCurrentStep();
    }

    public void UnregisterKitchenView(KitchenTutorialView view)
    {
        if (kitchenView == view)
            kitchenView = null;
    }

    private void TryAutoStartTutorial()
    {
        if (isTutorialActive) return;
        if (IsCompleted) return;
        if (PlayerPrefs.GetInt(KEY_SHOULD_PLAY_TUTORIAL, 0) != 1) return;
        if (!IsCounterScene && !IsKitchenScene) return;

        StartTutorial();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryAutoStartTutorial();

        if (!isTutorialActive)
            return;

        bool shouldHideTutorialCustomerOnCounterEnter =
            IsCounterScene &&
            (currentStep == TutorialStep.Counter_Welcome ||
             currentStep == TutorialStep.Counter_DayIcon ||
             currentStep == TutorialStep.Counter_Satisfaction ||
             currentStep == TutorialStep.Counter_Pause);

        if (shouldHideTutorialCustomerOnCounterEnter)
            Order_ManagerBridge_PrepareTutorialCustomerUI();

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

        if (currentStep == TutorialStep.Counter_FirstAutoExplain ||
            currentStep == TutorialStep.Counter_FirstAutoExplain2 ||
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
                Order_Manager.Instance.GetPrice();
                break;
        }
    }

    public bool IsKitchenActionAllowed(KitchenPracticeTarget action)
    {
        if (!isTutorialActive) return true;
        if (!IsKitchenScene) return true;
        if (currentStep != TutorialStep.Kitchen_FirstCookProgress) return true;

        return waitingForKitchenPractice && expectedKitchenPractice == action;
    }

    public void OnTutorialPausePressed()
    {
        HandleCounterPractice(CounterPracticeTarget.Pause);
    }

    public void OnTutorialSettingPressed()
    {
        HandleCounterPractice(CounterPracticeTarget.Setting);
    }

    public void OnTutorialBookPressed()
    {
        HandleCounterPractice(CounterPracticeTarget.Book);
    }

    public void OnTutorialHomePressed()
    {
        HandleCounterPractice(CounterPracticeTarget.Home);
    }

    public void OnTutorialResumePressed()
    {
        HandleCounterPractice(CounterPracticeTarget.Resume);
    }

    private void HandleCounterPractice(CounterPracticeTarget input)
    {
        if (!isTutorialActive || !IsCounterScene || !waitingForCounterPractice)
            return;

        if (input != expectedCounterPractice)
            return;

        ResetCounterPracticeState();

        switch (input)
        {
            case CounterPracticeTarget.Pause:
                waitingForNextButton = true;
                if (counterView != null)
                    counterView.KeepPausePanelOpenThenShowNext("게임 내 시간도 멈춰지니 걱정하지 마세요.");
                break;

            case CounterPracticeTarget.Setting:
                waitingForNextButton = true;
                if (counterView != null)
                    counterView.PlaySettingPreviewThenShowNext("여기서 게임 내 세팅들을 설정할 수 있어요.");
                break;

            case CounterPracticeTarget.Book:
                waitingForNextButton = true;
                if (counterView != null)
                    counterView.PlayBookPreviewThenShowNext("레시피가 헷갈린다면 한번씩 확인해보세요.");
                break;

            case CounterPracticeTarget.Home:
                waitingForNextButton = true;
                if (counterView != null)
                    counterView.PlayHomePreviewThenShowNext("게임을 종료할 수 있지만 저장은 되지 않으니 신중하게 눌러주세요.");
                break;

            case CounterPracticeTarget.Resume:
                if (counterView != null)
                    counterView.ClosePersistentPausePanel();

                waitingForNextButton = false;
                AdvanceStep();
                break;
        }
    }

    public void OnKitchenDishCompleted()
    {
        if (!isTutorialActive)
            return;

        waitingForKitchenComplete = false;
        ResetKitchenPracticeState();

        switch (currentStep)
        {
            case TutorialStep.Kitchen_FirstCookProgress:
                currentStep = TutorialStep.Kitchen_FirstCookDone_ReturnToCounter;
                SceneManager.LoadScene("01_Counter");
                break;
        }
    }

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
                currentStep = TutorialStep.Counter_DayIcon2;
                break;

            case TutorialStep.Counter_DayIcon2:
                currentStep = TutorialStep.Counter_Satisfaction;
                break;

            case TutorialStep.Counter_Satisfaction:
                currentStep = TutorialStep.Counter_Pause;
                break;

            case TutorialStep.Counter_Pause:
                currentStep = TutorialStep.Counter_Setting;
                break;

            case TutorialStep.Counter_Setting:
                currentStep = TutorialStep.Counter_Book;
                break;

            case TutorialStep.Counter_Book:
                currentStep = TutorialStep.Counter_Home;
                break;

            case TutorialStep.Counter_Home:
                currentStep = TutorialStep.Counter_Resume;
                break;

            case TutorialStep.Counter_Resume:
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
                currentStep = TutorialStep.Counter_FirstAutoExplain2;
                break;

            case TutorialStep.Counter_FirstAutoExplain2:
                currentStep = TutorialStep.Counter_FirstWaitYesClick;
                break;

            case TutorialStep.Counter_FirstServeResult:
                currentStep = TutorialStep.Counter_FirstServeReaction;
                break;

            case TutorialStep.Counter_FirstServeReaction:
                currentStep = TutorialStep.Counter_FirstServeMoneyExplain;
                break;

            case TutorialStep.Counter_FirstServeMoneyExplain:
                currentStep = TutorialStep.Counter_FirstServeCheer;
                break;

            case TutorialStep.Counter_FirstServeCheer:
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
            RunCounterStep();
        else if (IsKitchenScene)
            RunKitchenStep();
    }

    private void RunCounterStep()
    {
        if (counterView == null)
            return;

        waitingForNextButton = false;
        waitingForYesButton = false;
        ResetCounterPracticeState();

        if (currentStep == TutorialStep.Kitchen_FirstCookDone_ReturnToCounter)
        {
            counterView.HideAll();
            return;
        }

        counterView.ResetView();

        switch (currentStep)
        {
            case TutorialStep.Counter_Welcome:
                waitingForNextButton = true;
                counterView.ShowMessage($"어서오세요 {GetPlayerName()} 사장님!\n오늘은 첫 영업 전에 기본 진행 방법을 알려드릴게요.", true);
                break;

            case TutorialStep.Counter_DayIcon:
                waitingForNextButton = true;
                counterView.ShowDayInfo("이 아이콘은 하루를 의미해요.\n시간이 다 지나면 하루 영업이 종료되고 정산을 합니다.");
                break;

            case TutorialStep.Counter_DayIcon2:
                waitingForNextButton = true;
                counterView.ShowDayInfo("종료되기 전에만 주문을 받으면 조리 중 하루가 끝나도 끝까지 조리가 가능하니\n걱정마세요!");
                break;

            case TutorialStep.Counter_Satisfaction:
                waitingForNextButton = true;
                counterView.ShowSatisfactionInfo("이 아이콘은 손님의 만족도를 의미해요.\n만족도에 따라 받을 수 있는 팁이 달라지니 손님이 실망하지 않게 최대한 빠르게 만들어주세요.");
                break;

            case TutorialStep.Counter_Pause:
                waitingForCounterPractice = true;
                expectedCounterPractice = CounterPracticeTarget.Pause;
                counterView.ShowPauseInfo("이 버튼을 누르면 게임을 멈출 수 있어요.\n눌러서 멈춰볼까요?", false);
                break;

            case TutorialStep.Counter_Setting:
                waitingForCounterPractice = true;
                expectedCounterPractice = CounterPracticeTarget.Setting;
                counterView.ShowSettingInfo("이건 설정 버튼이에요. 눌러볼까요?", false);
                break;

            case TutorialStep.Counter_Book:
                waitingForCounterPractice = true;
                expectedCounterPractice = CounterPracticeTarget.Book;
                counterView.ShowBookInfo("메뉴북 버튼을 누르면 현재 만들 수 있는 메뉴들을 볼 수 있어요.", false);
                break;

            case TutorialStep.Counter_Home:
                waitingForCounterPractice = true;
                expectedCounterPractice = CounterPracticeTarget.Home;
                counterView.ShowHomeInfo("이번엔 홈 버튼을 직접 눌러보세요.", false);
                break;

            case TutorialStep.Counter_Resume:
                waitingForCounterPractice = true;
                expectedCounterPractice = CounterPracticeTarget.Resume;
                counterView.ShowResumeInfo("게임을 다시 진행하고 싶다면 화면을 아무데나 클릭해보세요.", false);
                break;

            case TutorialStep.Counter_FirstCustomerSpawn:
                SpawnFirstTutorialCustomer();            
                waitingForNextButton = true;
                counterView.ShowMessage("이제 첫 손님을 받아볼게요.", true);
                break;

            case TutorialStep.Counter_FirstOrderExplain:
                waitingForNextButton = true;
                counterView.ShowMessage("손님이 알리오 올리오에 스파게티면, 마늘 토핑 추가를 주문했어요.", true);
                break;

            case TutorialStep.Counter_FirstYesExplain:
                waitingForNextButton = true;
                counterView.ShowMessage("네 버튼을 누르면 주방으로 넘어가서 조리를 시작해요.", true);
                Order_ManagerBridge_ShowDecisionButtons(true, true, false, false);
                break;

            case TutorialStep.Counter_FirstAutoExplain:
                waitingForNextButton = true;
                counterView.ShowMessage("자동완성 버튼은 $5가 소모돼요.\n복잡한 메뉴가 들어오면 사용해보는 걸 추천해요.", true);
                Order_ManagerBridge_ShowDecisionButtons(true, true, false, false);
                break;

            case TutorialStep.Counter_FirstAutoExplain2:
                waitingForNextButton = true;
                counterView.ShowMessage("이번 튜토리얼 첫 주문은 직접 만들어볼게요.", true);
                Order_ManagerBridge_ShowDecisionButtons(true, true, false, false);
                if (counterView != null)
                    counterView.SetInputBlocker(false);
                break;

            case TutorialStep.Counter_FirstWaitYesClick:
                waitingForYesButton = true;             
                counterView.ShowMessage("그럼 주문을 받아볼까요? '네' 버튼을 눌러주세요.", false);
                Order_ManagerBridge_ShowDecisionButtons(true, true, true, false);
                break;

            case TutorialStep.Counter_FirstServeResult:
                waitingForNextButton = true;
                counterView.ShowMessage("손님에게 만든 파스타를 전달했어요.", true);
                break;

            case TutorialStep.Counter_FirstServeReaction:
                waitingForNextButton = true;
                counterView.ShowMessage("손님이 파스타가 마음에 들었나봐요.", true);
                break;

            case TutorialStep.Counter_FirstServeMoneyExplain:
                waitingForNextButton = true;
                counterView.ShowMessage("이런식으로 파스타를 만들다보면 더 많은 돈을 벌어서 재료를 늘릴 수 있어요.", true);
                break;

            case TutorialStep.Counter_FirstServeCheer:
                waitingForNextButton = true;
                counterView.ShowMessage($"이제 실제로 장사를 시작해볼까요?\n{GetPlayerName()} 사장님 화이팅!", true);
                break;            
        }
    }


    private void RunKitchenStep()
    {
        if (kitchenView == null)
            return;

        switch (currentStep)
        {
            case TutorialStep.Kitchen_FirstIntro:
                waitingForNextButton = false;
                waitingForKitchenComplete = true;
                currentStep = TutorialStep.Kitchen_FirstCookProgress;
                StartFirstKitchenGuide();
                break;

            case TutorialStep.Kitchen_FirstCookProgress:
                ResumeFirstKitchenGuide();
                break;
        }
    }

    public bool TryConsumeKitchenAction(KitchenPracticeTarget action)
    {
        if (!IsKitchenActionAllowed(action))
            return false;

        waitingForKitchenPractice = false;
        expectedKitchenPractice = KitchenPracticeTarget.None;

        switch (action)
        {
            case KitchenPracticeTarget.DragSpaghettiToCooker:
                firstKitchenGuideStep = FirstKitchenGuideStep.NoodleBoilingInfo;
                break;

            case KitchenPracticeTarget.ClickGasStove:
                firstKitchenGuideStep = FirstKitchenGuideStep.DragOil;
                break;

            case KitchenPracticeTarget.DragOilToPan:
                firstKitchenGuideStep = FirstKitchenGuideStep.OilInfo;
                break;

            case KitchenPracticeTarget.DragGarlicToPan:
                firstKitchenGuideStep = FirstKitchenGuideStep.GarlicInfo;
                break;

            case KitchenPracticeTarget.DragCookedNoodleToPan:
                firstKitchenGuideStep = FirstKitchenGuideStep.CookStartInfo;
                break;

            case KitchenPracticeTarget.DragPlateToTable:
                firstKitchenGuideStep = FirstKitchenGuideStep.PlateSpawnedInfo;
                break;

            case KitchenPracticeTarget.DragPastaToPlate:
                firstKitchenGuideStep = FirstKitchenGuideStep.PlatedInfo;
                break;

            case KitchenPracticeTarget.DragParmesanToPlate:
                firstKitchenGuideStep = FirstKitchenGuideStep.DragParsley;
                break;

            case KitchenPracticeTarget.DragParsleyToPlate:
                firstKitchenGuideStep = FirstKitchenGuideStep.DragPassTable;
                break;

            case KitchenPracticeTarget.DragPlateToPassTable:
                firstKitchenGuideStep = FirstKitchenGuideStep.Completed;
                OnKitchenDishCompleted();
                return true;
        }

        RunFirstKitchenGuideStep();
        return true;
    }

    public void OnClickKitchenNext()
    {
        if (!isTutorialActive || !IsKitchenScene)
            return;

        switch (firstKitchenGuideStep)
        {
            case FirstKitchenGuideStep.DragNoodle:
                BeginKitchenPractice(KitchenPracticeTarget.DragSpaghettiToCooker);
                return;

            case FirstKitchenGuideStep.ClickGasStove:
                BeginKitchenPractice(KitchenPracticeTarget.ClickGasStove);
                return;

            case FirstKitchenGuideStep.DragOil:
                BeginKitchenPractice(KitchenPracticeTarget.DragOilToPan);
                return;

            case FirstKitchenGuideStep.DragGarlic:
                BeginKitchenPractice(KitchenPracticeTarget.DragGarlicToPan);
                return;

            case FirstKitchenGuideStep.DragCookedNoodle:
                BeginKitchenPractice(KitchenPracticeTarget.DragCookedNoodleToPan);
                return;

            case FirstKitchenGuideStep.DragPlate:
                BeginKitchenPractice(KitchenPracticeTarget.DragPlateToTable);
                return;

            case FirstKitchenGuideStep.DragPastaToPlate:
                BeginKitchenPractice(KitchenPracticeTarget.DragPastaToPlate);
                return;

            case FirstKitchenGuideStep.DragParmesan:
                BeginKitchenPractice(KitchenPracticeTarget.DragParmesanToPlate);
                return;

            case FirstKitchenGuideStep.DragParsley:
                BeginKitchenPractice(KitchenPracticeTarget.DragParsleyToPlate);
                return;

            case FirstKitchenGuideStep.DragPassTable:
                BeginKitchenPractice(KitchenPracticeTarget.DragPlateToPassTable);
                return;
        }

        switch (firstKitchenGuideStep)
        {
            case FirstKitchenGuideStep.Intro:
                firstKitchenGuideStep = FirstKitchenGuideStep.DragNoodle;
                break;

            case FirstKitchenGuideStep.NoodleBoilingInfo:
                firstKitchenGuideStep = FirstKitchenGuideStep.ClickGasStove;
                break;

            case FirstKitchenGuideStep.OilInfo:
                firstKitchenGuideStep = FirstKitchenGuideStep.DragGarlic;
                break;

            case FirstKitchenGuideStep.GarlicInfo:
                firstKitchenGuideStep = FirstKitchenGuideStep.DragCookedNoodle;
                break;

            case FirstKitchenGuideStep.CookStartInfo:
                firstKitchenGuideStep = FirstKitchenGuideStep.FinishedInfo;
                break;

            case FirstKitchenGuideStep.FinishedInfo:
                firstKitchenGuideStep = FirstKitchenGuideStep.DragPlate;
                break;

            case FirstKitchenGuideStep.PlateSpawnedInfo:
                firstKitchenGuideStep = FirstKitchenGuideStep.DragPastaToPlate;
                break;

            case FirstKitchenGuideStep.PlatedInfo:
                firstKitchenGuideStep = FirstKitchenGuideStep.DragParmesan;
                break;
        }

        RunFirstKitchenGuideStep();
    }

    private void BeginKitchenPractice(KitchenPracticeTarget target)
    {
        waitingForKitchenPractice = true;
        expectedKitchenPractice = target;

        if (kitchenView != null)
            kitchenView.HideMessagePanelOnly();
    }

    private void StartFirstKitchenGuide()
    {
        firstKitchenGuideStep = FirstKitchenGuideStep.Intro;
        ResetKitchenPracticeState();
        RunFirstKitchenGuideStep();
    }

    private void ResumeFirstKitchenGuide()
    {
        RunFirstKitchenGuideStep();
    }

    private void RunFirstKitchenGuideStep()
    {
        if (kitchenView == null)
            return;

        ResetKitchenPracticeState();

        switch (firstKitchenGuideStep)
        {
            case FirstKitchenGuideStep.Intro:
                kitchenView.ShowStep(
                    "짠! 여기가 바로 주방이에요.\n아직은 텅 비어있지만 나중에는 재료로 가득 채울 수 있어요.",
                    KitchenTutorialView.KitchenHighlight.None,
                    true
                );
                break;

            case FirstKitchenGuideStep.DragNoodle:
                kitchenView.ShowStep(
                    "이건 파스타 면을 삶는 면탕기에요.\n스파게티면을 면탕기로 드래그 해볼까요?",
                    KitchenTutorialView.KitchenHighlight.PastaCooker,
                    true,
                    true
                );
                break;

            case FirstKitchenGuideStep.NoodleBoilingInfo:
                kitchenView.ShowStep(
                    "면이 보글보글 삶아지고 있어요.\n그동안 다른 걸 준비해 볼까요?",
                    KitchenTutorialView.KitchenHighlight.None,
                    true,
                    true
                );
                break;

            case FirstKitchenGuideStep.ClickGasStove:
                kitchenView.ShowStep(
                    "가스레인지를 클릭해보세요.",
                    KitchenTutorialView.KitchenHighlight.GasStove,
                    true,
                    true
                );
                break;

            case FirstKitchenGuideStep.DragOil:
                kitchenView.ShowStep(
                    "후라이팬이 생겼네요. 바로 위에 있는 올리브오일을 후라이팬으로 드래그 해보세요.",
                    KitchenTutorialView.KitchenHighlight.None,
                    true,
                    true
                );
                break;

            case FirstKitchenGuideStep.OilInfo:
                kitchenView.ShowStep(
                    "올리브 오일을 부으면 가스레인지에 불이 켜진답니다.",
                    KitchenTutorialView.KitchenHighlight.None,
                    true,
                    true
                );
                break;

            case FirstKitchenGuideStep.DragGarlic:
                kitchenView.ShowStep(
                    "이제 토핑을 넣어볼게요. 손님이 마늘을 넣어달라고 했으니 마늘을 후라이팬으로 드래그 해볼까요?",
                    KitchenTutorialView.KitchenHighlight.None,
                    true,
                    true
                );
                break;

            case FirstKitchenGuideStep.GarlicInfo:
                kitchenView.ShowStep(
                    "마늘을 넣었어요.\n토핑을 빼먹거나 주문하지 않은 토핑을 넣을 경우 손님이 실망하니까 주문을 꼭 기억하세요!",
                    KitchenTutorialView.KitchenHighlight.None,
                    true,
                    true
                );
                break;

            case FirstKitchenGuideStep.DragCookedNoodle:
                kitchenView.ShowStep(
                    "이제 면이 다 익은 거 같아요.\n익은 면을 후라이팬으로 드래그 해보세요.",
                    KitchenTutorialView.KitchenHighlight.None,
                    true,
                    true
                );
                break;

            case FirstKitchenGuideStep.CookStartInfo:
                kitchenView.ShowStep(
                    "익은 면을 넣으면 파스타 조리를 시작합니다.\n오일, 소스, 토핑은 순서가 상관 없지만 면은 꼭 마지막에 넣어야해요!",
                    KitchenTutorialView.KitchenHighlight.None,
                    true,
                    true
                );
                break;

            case FirstKitchenGuideStep.FinishedInfo:
                kitchenView.ShowStep(
                    "자 이제 파스타가 완성되었네요. 파스타를 옮길 접시를 꺼내볼까요?",
                    KitchenTutorialView.KitchenHighlight.None,
                    true,
                    true
                );
                break;

            case FirstKitchenGuideStep.DragPlate:
                kitchenView.ShowStep(
                    "접시를 테이블로 드래그 해보세요.",
                    KitchenTutorialView.KitchenHighlight.PlateTable,
                    true,
                    true
                );
                break;

            case FirstKitchenGuideStep.PlateSpawnedInfo:
                kitchenView.ShowStep(
                    "이제 완성된 파스타를 접시로 옮겨야겠죠?",
                    KitchenTutorialView.KitchenHighlight.None,
                    true,
                    true
                );
                break;

            case FirstKitchenGuideStep.DragPastaToPlate:
                kitchenView.ShowStep(
                    "완성된 파스타를 접시로 드래그 해보세요.",
                    KitchenTutorialView.KitchenHighlight.None,
                    true,
                    true
                );
                break;

            case FirstKitchenGuideStep.PlatedInfo:
                kitchenView.ShowStep(
                    "짜잔 접시에 파스타를 예쁘게 담았어요.\n이제 마지막 단계에요.",
                    KitchenTutorialView.KitchenHighlight.None,
                    true,
                    true
                );
                break;

            case FirstKitchenGuideStep.DragParmesan:
                kitchenView.ShowStep(
                    "먼저 치즈를 그릇에 뿌려주세요.",
                    KitchenTutorialView.KitchenHighlight.Parmesan,
                    true,
                    true
                );
                break;

            case FirstKitchenGuideStep.DragParsley:
                kitchenView.ShowStep(
                    "다음으로 파슬리를 뿌려주세요.",
                    KitchenTutorialView.KitchenHighlight.Parsley,
                    true,
                    true
                );
                break;

            case FirstKitchenGuideStep.DragPassTable:
                kitchenView.ShowStep(
                    "이제 완성된 파스타를 기다리고 있는 손님에게 나가볼까요?\n패스테이블로 드래그해봐요.",
                    KitchenTutorialView.KitchenHighlight.PassTable,
                    true,
                    true
                );
                break;
        }
    }

    private void ResetCounterPracticeState()
    {
        waitingForCounterPractice = false;
        expectedCounterPractice = CounterPracticeTarget.None;
    }

    private void ResetKitchenPracticeState()
    {
        waitingForKitchenPractice = false;
        expectedKitchenPractice = KitchenPracticeTarget.None;
    }

    private void SpawnFirstTutorialCustomer()
    {
        Order firstOrder = CreateFirstTutorialOrder();
        Order_ManagerBridge_SpawnTutorialCustomer(firstOrder, 0, "알리오 올리오 하나 해주세요!\n면은 스파게티면이 좋겠어요. 마늘 추가할래요.");
    }

    private Order CreateFirstTutorialOrder()
    {
        if (tutorialMenuDB == null)
        {
            Debug.LogError("tutorialMenuDB가 연결되지 않았음");
            return null;
        }

        MenuData aglioMenu = tutorialMenuDB.GetMenuByID(1);

        if (aglioMenu == null)
        {
            Debug.LogError("튜토리얼용 알리오 올리오 메뉴(ID 1)를 찾지 못함");
            return null;
        }

        int spaghettiID = 101;
        List<int> toppings = new List<int> { 302 };

        return new Order(aglioMenu, spaghettiID, toppings, tutorialTemplateDB);
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
        ResetCounterPracticeState();
        ResetKitchenPracticeState();
        firstKitchenGuideStep = FirstKitchenGuideStep.None;

        if (counterView != null)
            counterView.HideAll();

        if (kitchenView != null)
            kitchenView.HideAll();

        if (Gold_Manager.Instance != null)
        {
            Gold_Manager.Instance.totalGold = 0f;
            Gold_Manager.Instance.ResetDailyStats();
            Gold_Manager.Instance.SetUIText(Gold_Manager.Instance.goldText);
        }

        if (Order_Manager.Instance != null)
        {
            Order_Manager.Instance.PrepareTutorialCustomerUI();
            Order_Manager.Instance.ResetForTutorialToRealGame();
        }

        PlayerPrefs.SetInt(KEY_SHOULD_PLAY_TUTORIAL, 0);
        PlayerPrefs.SetInt(KEY_TUTORIAL_COMPLETED, 1);
        PlayerPrefs.Save();
    }

    #region Bridge Methods
    private void Order_ManagerBridge_PrepareTutorialCustomerUI()
    {
        if (Order_Manager.Instance == null) return;
        Order_Manager.Instance.PrepareTutorialCustomerUI();
    }

    private void Order_ManagerBridge_ShowDecisionButtons(bool showYes, bool showAuto, bool enableYes, bool enableAuto)
    {
        if (Order_Manager.Instance == null) return;
        Order_Manager.Instance.ShowTutorialDecisionButtons(showYes, showAuto, enableYes, enableAuto);
        if (counterView != null)
            counterView.SetInputBlocker(false);

    }

    private void Order_ManagerBridge_SpawnTutorialCustomer(Order order, int spriteIndex, string forcedMessage)
    {
        if (Order_Manager.Instance == null) return;
        Order_Manager.Instance.SpawnTutorialCustomer(order, spriteIndex, forcedMessage);
    }
    #endregion
}
