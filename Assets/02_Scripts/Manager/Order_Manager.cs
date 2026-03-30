using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Order_Manager : MonoBehaviour
{
    public static Order_Manager Instance;

    [SerializeField] public OrderGenerator generator;
    [SerializeField] public Day_Manager dayManager;
    [SerializeField] public IngredientDatabase ingredientDB;
    [SerializeField] public ServeMessageDatabase serveMessageDB;

    [Header("UI")]
    public GameObject customerUIPrefab;

    private Transform customerUIParent;   // Panel_Customer
    private Transform serveBoxParent;     // Image_Table

    [Header("손님 프리팹")]
    private CustomerUI currentCustomer;
    private int currentCustomerSpriteIndex = -1;
    private int lastCustomerSpriteIndex = -1;

    [Header("연출용 파스타박스 프리팹")]
    [SerializeField] private GameObject serveBoxPrefab;

    [Header("박스 등장 연출")]
    [SerializeField] private float serveBoxStartYOffset = -250f;
    [SerializeField] private float serveBoxMoveDuration = 0.45f;
    [SerializeField] private float serveBoxFadeDuration = 0.25f;
    [SerializeField] private float serveBoxStartScale = 0.95f;

    [Header("딩동 사운드")]
    [SerializeField] private AudioClip customerEnterSFX;

    private Coroutine customerEntranceRoutine;
    private Sequence customerEntranceSequence;

    private bool isAutoCooking = false;

    public enum ServiceState
    {
        WaitingForOrder,
        TakingOrder,
        Cooking,
        ServingDish,
        DayEnded
    }

    public ServiceState currentState;

    public Order currentOrder;
    private bool? pendingResult = null;
    private bool pendingSatisfactionZero = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            GameObject obj = Instantiate(customerUIPrefab);
            currentCustomer = obj.GetComponent<CustomerUI>();
            DontDestroyOnLoad(obj);
            currentCustomer.gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetState(ServiceState.WaitingForOrder);
    }

    public void SetState(ServiceState state)
    {
        currentState = state;

        switch (currentState)
        {
            case ServiceState.WaitingForOrder:
                break;
            case ServiceState.TakingOrder:
                break;
            case ServiceState.Cooking:
                break;
            case ServiceState.ServingDish:
                break;
            case ServiceState.DayEnded:
                break;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private bool BindCounterParents()
    {
        GameObject customerPanel = GameObject.Find("Panel_Customer");
        GameObject imageTable = GameObject.Find("Image_Table");

        if (customerPanel == null)
        {
            Debug.LogError("Panel_Customer를 찾을 수 없음!");
            return false;
        }

        customerUIParent = customerPanel.transform;

        if (imageTable == null)
        {
            Debug.LogError("Image_Table을 찾을 수 없음! serveBox는 생성되지 않음");
            serveBoxParent = null;
        }
        else
        {
            serveBoxParent = imageTable.transform;
        }

        return true;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "01_Counter")
            return;

        if (!BindCounterParents())
            return;

        if (currentCustomer == null)
        {
            GameObject obj = Instantiate(customerUIPrefab, customerUIParent);
            currentCustomer = obj.GetComponent<CustomerUI>();
            currentCustomer.transform.SetAsFirstSibling();
        }
        else
        {
            currentCustomer.transform.SetParent(customerUIParent, false);
            currentCustomer.transform.SetAsFirstSibling();
            currentCustomer.gameObject.SetActive(true);
        }

        if (pendingSatisfactionZero)
        {
            pendingSatisfactionZero = false;
            StartCoroutine(DontSubmitDish());
            return;
        }

        if (pendingResult.HasValue)
        {
            if (currentCustomer != null && currentCustomerSpriteIndex != -1)
            {
                currentCustomer.SetCustomerSprite(currentCustomerSpriteIndex);
                currentCustomer.gameObject.SetActive(true);
                currentCustomer.HideBubble();
            }

            StartCoroutine(ServeDishAndGoToNextCustomer(pendingResult.Value));
            pendingResult = null;
        }
        else
        {
            if (currentOrder == null)
            {
                StartService();
            }
        }
    }

    public void StartService()
    {
        SpawnCustomer();
    }

    void SpawnCustomer()
    {
        if (!dayManager.isTakingOrder)
        {
            CheckDayEndCondition();
            return;
        }

        currentState = ServiceState.TakingOrder;

        if (customerUIParent == null)
        {
            if (!BindCounterParents())
                return;
        }

        if (currentCustomer == null)
        {
            GameObject obj = Instantiate(customerUIPrefab, customerUIParent);
            currentCustomer = obj.GetComponent<CustomerUI>();
            currentCustomer.transform.SetAsFirstSibling();
        }
        else
        {
            currentCustomer.transform.SetParent(customerUIParent, false);
            currentCustomer.transform.SetAsFirstSibling();
        }

        if (currentCustomerSpriteIndex == -1)
        {
            int newIndex;

            do
            {
                newIndex = Random.Range(0, currentCustomer.customerSprites.Count);
            }
            while (newIndex == lastCustomerSpriteIndex && currentCustomer.customerSprites.Count > 1);

            currentCustomerSpriteIndex = newIndex;
            lastCustomerSpriteIndex = newIndex;
        }

        currentCustomer.Appear();
        currentCustomer.SetCustomerSprite(currentCustomerSpriteIndex);

        currentOrder = generator.GenerateOrder();

        if (currentOrder == null)
            return;

        DebugIngredientSet(currentOrder, "손님 주문");

        string message = currentOrder.GetOrderText(generator.ingredientDB);

        StopCustomerEntranceAnimation();
        customerEntranceRoutine = StartCoroutine(CustomerEntranceRoutine(message));

        if (currentCustomer.yesButton != null)
            currentCustomer.yesButton.SetActive(false);

        if (currentCustomer.autoButton != null)
            currentCustomer.autoButton.SetActive(false);
    }

    IEnumerator CustomerEntranceRoutine(string message)
    {
        if (currentCustomer == null)
        {
            customerEntranceRoutine = null;
            yield break;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(customerEnterSFX);
        }

        currentCustomer.gameObject.SetActive(true);

        RectTransform rect = currentCustomer.GetComponent<RectTransform>();
        if (rect == null)
        {
            customerEntranceRoutine = null;
            yield break;
        }

        Vector2 endPos = new Vector2(0f, -100f);
        Vector2 startPos = new Vector2(0f, endPos.y - 650f);
        Vector2 overshootPos = endPos + new Vector2(0f, 35f);

        rect.DOKill();

        Vector3 baseScale = rect.localScale;

        rect.anchoredPosition = startPos;
        rect.localScale = new Vector3(baseScale.x, baseScale.y * 1.08f, baseScale.z);

        yield return new WaitForSeconds(1.5f);

        if (currentCustomer == null || rect == null)
        {
            customerEntranceRoutine = null;
            yield break;
        }

        customerEntranceSequence = DOTween.Sequence();
        customerEntranceSequence.SetLink(rect.gameObject, LinkBehaviour.KillOnDestroy);

        customerEntranceSequence.Append(
            rect.DOAnchorPos(overshootPos, 0.45f)
                .SetEase(Ease.OutCubic)
        );

        customerEntranceSequence.Join(
            rect.DOScale(new Vector3(baseScale.x, baseScale.y * 0.94f, baseScale.z), 0.45f)
                .SetEase(Ease.OutQuad)
        );

        customerEntranceSequence.Append(
            rect.DOAnchorPos(endPos, 0.18f)
                .SetEase(Ease.OutQuad)
        );

        customerEntranceSequence.Join(
            rect.DOScale(baseScale, 0.18f)
                .SetEase(Ease.OutBack)
        );

        yield return customerEntranceSequence.WaitForCompletion();

        customerEntranceSequence = null;
        customerEntranceRoutine = null;

        if (currentCustomer == null)
            yield break;

        currentCustomer.ShowOrder(message);
    }

    private void StopCustomerEntranceAnimation()
    {
        if (customerEntranceRoutine != null)
        {
            StopCoroutine(customerEntranceRoutine);
            customerEntranceRoutine = null;
        }

        if (customerEntranceSequence != null)
        {
            if (customerEntranceSequence.IsActive())
                customerEntranceSequence.Kill();

            customerEntranceSequence = null;
        }

        if (currentCustomer != null)
        {
            RectTransform rt = currentCustomer.GetComponent<RectTransform>();
            if (rt != null)
                rt.DOKill();
        }
    }

    private void DestroyCurrentCustomerSafely()
    {
        StopCustomerEntranceAnimation();

        if (currentCustomer != null)
        {
            RectTransform rt = currentCustomer.GetComponent<RectTransform>();
            if (rt != null)
                rt.DOKill();

            Destroy(currentCustomer.gameObject);
            currentCustomer = null;
        }
    }

    public void GetPrice()
    {
        if (currentOrder == null)
            return;

        float menuPrice = currentOrder.Price(generator.ingredientDB);
        Gold_Manager.Instance.Earn(menuPrice);

        Debug.Log($"손님 주문 완료! 받은 금액: {menuPrice}, 총 골드: {Gold_Manager.Instance.totalGold}");

        GoToKitchen();
    }

    public void GoToKitchen()
    {
        SetState(ServiceState.Cooking);

        StopCustomerEntranceAnimation();

        if (currentCustomer != null)
        {
            currentCustomer.gameObject.SetActive(false);
        }

        if (CustomerSatisfaction_Manager.Instance != null)
        {
            CustomerSatisfaction_Manager.Instance.ResetSatisfaction();
        }

        SceneManager.LoadScene(2);
    }

    public void SubmitDish(PastaBox pastaBox)
    {
        if (currentOrder == null)
            return;

        bool success = IsCorrect(pastaBox, currentOrder);

        if (success)
        {
            float satisfactionRatio = 0f;
            if (CustomerSatisfaction_Manager.Instance != null)
            {
                satisfactionRatio = CustomerSatisfaction_Manager.Instance.GetSatisfactionRatio();
            }

            float tip = 0f;

            if (satisfactionRatio >= 0.8f)
            {
                tip = 2f;
                Level_Manager.Instance.EarnXP(3);
                Debug.Log("+3");
            }
            else if (satisfactionRatio >= 0.6f)
            {
                tip = 1f;
                Level_Manager.Instance.EarnXP(3);
                Debug.Log("+3");
            }
            else
            {
                tip = 0f;
            }

            Gold_Manager.Instance.EarnTip(tip);
            Level_Manager.Instance.EarnXP(5);
            Debug.Log("성공 +5");
            Debug.Log($"팁 지급: {tip}, 현재 골드: {Gold_Manager.Instance.totalGold}");
            Debug.Log($"현재 XP : {Level_Manager.Instance.currentXP}");
        }

        HashSet<int> usedIngredients = pastaBox.GetIngredientSet();
        HashSet<int> correctIngredients = currentOrder.GetIngredientSet();

        float totalingredientCost = 0f;
        float refund = 0f;

        foreach (int id in usedIngredients)
        {
            var ingredient = ingredientDB.GetIngredient(id);
            if (ingredient == null) continue;

            totalingredientCost += ingredient.ingredientCost;
        }

        foreach (int id in correctIngredients)
        {
            if (!usedIngredients.Contains(id))
            {
                var ingredient = ingredientDB.GetIngredient(id);
                if (ingredient == null) continue;

                refund += ingredient.price;
            }
        }

        Gold_Manager.Instance.SpendBusinessCost(totalingredientCost);

        if (refund > 0f)
        {
            Gold_Manager.Instance.Refund(refund);
            Debug.Log($"빠진 재료 환불: {refund}");
        }

        Debug.Log($"재료비 차감: {totalingredientCost}, 현재 골드: {Gold_Manager.Instance.totalGold}");

        pendingResult = success;
        SetState(ServiceState.ServingDish);

        currentOrder = null;
    }

    IEnumerator ServeDishAndGoToNextCustomer(bool success)
    {
        yield return new WaitForSeconds(1f);

        GameObject box = null;

        if (serveBoxPrefab != null && serveBoxParent != null)
        {
            box = Instantiate(serveBoxPrefab, serveBoxParent, false);
            yield return StartCoroutine(PlayServeBoxEntrance(box));
        }

        yield return new WaitForSeconds(1f);

        if (currentCustomer != null)
        {
            currentCustomer.SetEmotion(success);
            currentCustomer.ShowResult(serveMessageDB.GetRandomMessage(success));
        }

        yield return new WaitForSeconds(2f);

        Coroutine exitAnim = StartCoroutine(PlayCustomerExitAnimation());

        if (box != null)
            StartCoroutine(FadeOutServeBox(box));

        yield return exitAnim;

        DestroyCurrentCustomerSafely();
        currentCustomerSpriteIndex = -1;

        yield return new WaitForSeconds(2f);

        SpawnCustomer();
        CheckDayEndCondition();
    }


    private IEnumerator PlayServeBoxEntrance(GameObject box)
    {
        if (box == null)
            yield break;

        RectTransform boxRect = box.GetComponent<RectTransform>();
        CanvasGroup boxCg = box.GetComponent<CanvasGroup>();

        if (boxCg == null)
            boxCg = box.AddComponent<CanvasGroup>();

        if (boxRect == null)
        {
            boxCg.alpha = 1f;
            yield break;
        }

        boxRect.DOKill();
        boxCg.DOKill();

        Vector2 targetPos = boxRect.anchoredPosition;
        Vector2 startPos = targetPos + new Vector2(0f, serveBoxStartYOffset);

        boxRect.anchoredPosition = startPos;
        Vector3 targetScale = boxRect.localScale;
        Vector3 startScale = targetScale * serveBoxStartScale;

        boxRect.localScale = startScale;

        Sequence boxSequence = DOTween.Sequence();
        boxSequence.SetLink(box, LinkBehaviour.KillOnDestroy);

        boxSequence.Append(
            boxRect.DOAnchorPos(targetPos, serveBoxMoveDuration)
                   .SetEase(Ease.OutCubic)
        );

        boxSequence.Join(
            boxRect.DOScale(targetScale, serveBoxMoveDuration)
                   .SetEase(Ease.OutBack)
        );

        boxSequence.Join(
            boxCg.DOFade(1f, serveBoxFadeDuration)
                 .SetEase(Ease.Linear)
        );

        yield return boxSequence.WaitForCompletion();
    }

    public void SatisfactionZero()
    {
        if (currentCustomer != null)
        {
            currentCustomer.SetEmotion(false);
        }

        pendingSatisfactionZero = true;
        GoToCounterScene();
    }

    void GoToCounterScene()
    {
        SceneManager.LoadScene("01_Counter");
    }

    IEnumerator DontSubmitDish()
    {
        if (customerUIParent == null)
        {
            if (!BindCounterParents())
                yield break;
        }

        if (currentCustomer == null)
        {
            GameObject obj = Instantiate(customerUIPrefab, customerUIParent);
            currentCustomer = obj.GetComponent<CustomerUI>();
            currentCustomer.Appear();
        }
        else
        {
            currentCustomer.transform.SetParent(customerUIParent, false);
            currentCustomer.transform.SetAsFirstSibling();
            currentCustomer.gameObject.SetActive(true);
        }

        if (currentCustomerSpriteIndex == -1)
        {
            currentCustomerSpriteIndex = Random.Range(0, currentCustomer.customerSprites.Count);
        }

        currentCustomer.SetCustomerSprite(currentCustomerSpriteIndex);

        string resultMessage = serveMessageDB.GetRandomMessageNothing();

        currentCustomer.SetEmotion(false);
        currentCustomer.ShowResult(resultMessage);

        if (currentOrder != null)
        {
            float refund = currentOrder.Price(generator.ingredientDB);
            Gold_Manager.Instance.Refund(refund);
            Debug.Log($"전체환불 : {refund}");
        }

        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(PlayCustomerExitAnimation());

        DestroyCurrentCustomerSafely();
        currentCustomerSpriteIndex = -1;
        currentOrder = null;

        yield return new WaitForSeconds(2f);

        SpawnCustomer();

    }

    public IEnumerator PlayCustomerExitForDayEnd()
    {
        if (currentCustomer == null)
            yield break;

        yield return StartCoroutine(PlayCustomerExitAnimation());

        DestroyCurrentCustomerSafely();
        currentCustomerSpriteIndex = -1;
        currentOrder = null;
    }

    private IEnumerator PlayCustomerExitAnimation()
    {
        if (currentCustomer == null)
            yield break;

        RectTransform rect = currentCustomer.GetComponent<RectTransform>();
        if (rect == null)
            yield break;

        rect.DOKill();

        currentCustomer.HideBubble();

        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0f, -700f); 

        yield return rect.DOAnchorPos(endPos, 0.4f)
            .SetEase(Ease.InQuad)
            .WaitForCompletion();
    }


    private IEnumerator FadeOutServeBox(GameObject box)
    {
        if (box == null)
            yield break;

        CanvasGroup cg = box.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = box.AddComponent<CanvasGroup>();

        yield return cg.DOFade(0f, 0.4f).WaitForCompletion();

        Destroy(box);
    }


    public bool IsCorrect(PastaBox pastaBox, Order order)
    {
        if (pastaBox == null || order == null)
            return false;

        bool sameIngredients = pastaBox.GetIngredientSet().SetEquals(order.GetIngredientSet());
        bool sameBaked = pastaBox.IsBaked == order.IsBaked;

        Debug.Log($"재료 일치: {sameIngredients}, box baked: {pastaBox.IsBaked}, order baked: {order.IsBaked}");

        return sameIngredients && sameBaked;
    }

    public void OnClickAutoButton()
    {
        if (isAutoCooking)
            return;

        if (currentOrder == null)
            return;

        if (currentState != ServiceState.TakingOrder)
            return;

        if (dayManager != null && !dayManager.isTakingOrder)
            return;

        StartCoroutine(AutoCookRoutine());
    }

    private IEnumerator AutoCookRoutine()
    {
        isAutoCooking = true;

        SetState(ServiceState.ServingDish);
        StopCustomerEntranceAnimation();

        if (currentCustomer != null)
        {
            currentCustomer.gameObject.SetActive(true);
            currentCustomer.SetCustomerSprite(currentCustomerSpriteIndex);
            currentCustomer.HideBubble();
        }

        yield return new WaitForSeconds(0.3f);

        float menuPrice = currentOrder.Price(generator.ingredientDB);
        float ingredientCost = currentOrder.Ingredient_Cost(ingredientDB);
        float autoExtraCost = 5f;

        Gold_Manager.Instance.Earn(menuPrice);
        Gold_Manager.Instance.SpendBusinessCost(ingredientCost + autoExtraCost);

        Level_Manager.Instance.EarnXP(5);

        Debug.Log(
            $"[AUTO COOK] 자동 조리 완료 | " +
            $"주문금액 +{menuPrice}, 재료비 -{ingredientCost}, 자동조리비 -{autoExtraCost}, 현재 골드: {Gold_Manager.Instance.totalGold}"
        );

        currentOrder = null;

        yield return StartCoroutine(ServeDishAndGoToNextCustomer(true));

        isAutoCooking = false;
    }

    public void OnOrderTimeEnded()
    {
        if (currentState == ServiceState.TakingOrder)
        {
            StartCoroutine(HandleCustomerExitAndEndDay());
        }
    }

    private IEnumerator HandleCustomerExitAndEndDay()
    {
        yield return StartCoroutine(PlayCustomerExitAnimation());

        DestroyCurrentCustomerSafely();
        currentCustomerSpriteIndex = -1;
        currentOrder = null;

        yield return new WaitForSeconds(0.5f);

        CheckDayEndCondition();
    }


    void CheckDayEndCondition()
    {
        if (!dayManager.isTakingOrder && currentOrder == null && currentCustomer == null)
        {
            dayManager.EndDay();
        }
    }
    
    void DebugIngredientSet(IHasIngredients target, string label)
    {
        HashSet<int> set = target.GetIngredientSet();
        string result = string.Join(", ", set);

        Debug.Log($"{label} 재료 HashSet: [{result}]");

        if (target is Order order)
        {
            float menuPrice = order.Price(ingredientDB);
            float ingredientCost = order.Ingredient_Cost(ingredientDB);

            Debug.Log($"{label} - 메뉴 총 가격: {menuPrice} / 재료 비용: {ingredientCost}");
        }
    }

    public void ResetForAbandonDay()
    {
        StopAllCoroutines();
        StopCustomerEntranceAnimation();

        pendingResult = null;
        pendingSatisfactionZero = false;
        isAutoCooking = false;

        currentOrder = null;
        currentCustomerSpriteIndex = -1;

        if (currentCustomer != null)
        {
            currentCustomer.HideBubble();
            currentCustomer.gameObject.SetActive(false);
        }

        currentState = ServiceState.WaitingForOrder;
    }
}