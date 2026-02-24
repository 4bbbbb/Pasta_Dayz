using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OrderManager : MonoBehaviour
{
    [SerializeField] public OrderGenerator generator;
    public DayManager dayManager;

    [Header("UI")]
    public GameObject customerUIPrefab;
    private Transform canvasTransform;

    [Header("손님 프리팹")]
    private CustomerUI currentCustomer;

    [Header("연출용 파스타박스 프리팹")]
    [SerializeField] GameObject serveBoxPrefab;  

    public enum ServiceState
    {
        WaitingCustomer,
        TakingOrder,
        Cooking,
        WaitingNextCustomer
    }

    public ServiceState currentState;
    public float nextCustomerDelay = 2f;

    private Order currentOrder;
    private bool? pendingResult = null;

    void Awake()
    {        
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "02_Counter")
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            canvasTransform = canvas.transform;

            //  이미 존재하는 CustomerUI가 있다면
            if (currentCustomer != null)
            {                
                currentCustomer.transform.SetParent(canvasTransform, false);
            }
            else
            {
                currentCustomer = FindObjectOfType<CustomerUI>();

                if (currentCustomer == null)
                {
                    GameObject obj = Instantiate(customerUIPrefab, canvasTransform);
                    currentCustomer = obj.GetComponent<CustomerUI>();
                    DontDestroyOnLoad(obj);
                }
            }

            //  결과 연출이 있다면 먼저 처리
            if (pendingResult.HasValue)
            {
                StartCoroutine(ServeDishAndGoToNextCustomer(pendingResult.Value));
                pendingResult = null;
            }
            else
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
            return;

        currentState = ServiceState.TakingOrder;

        // 이미 주문된 손님이 있다면 그 손님은 계속 보여준다.
        if (currentCustomer != null)
        {
            currentCustomer.Appear();  // 손님 등장
        }

        StartCoroutine(GenerateOrderAfterDelay(0.2f));
    }

    IEnumerator GenerateOrderAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (currentCustomer == null)
            yield break;

        currentOrder = generator.GenerateOrder();
        DebugIngredientSet(currentOrder, "주문");
        if (currentOrder == null)
            yield break;

        string message = currentOrder.GetOrderText(generator.ingredientDB);

        if (currentCustomer != null)
            currentCustomer.ShowOrder(message);
    }

    public void SubmitDish(PastaBox pastaBox)
    {
        if (currentOrder == null)
            return;

        bool success = IsCorrect(pastaBox, currentOrder);

        Debug.Log(success ? "성공!" : "실패!");

        currentOrder = null;

        pendingResult = success;
    }

    IEnumerator ServeDishAndGoToNextCustomer(bool success)
    {
        // 1초 대기 후 PastaBox 생성
        yield return new WaitForSeconds(1f);

        GameObject box = Instantiate(serveBoxPrefab, canvasTransform);

        if (currentCustomer != null)
        {
            currentCustomer.HideOrder();

            string resultMessage = success ? "성공!" : "실패!";
            currentCustomer.orderText.text = resultMessage;
        }

        yield return new WaitForSeconds(2f);

        if (currentCustomer != null)
        {
            currentCustomer.Disappear();
        }

        yield return new WaitForSeconds(1f);

        SpawnCustomer();
    }

    public bool IsCorrect(IHasIngredients a, IHasIngredients b)
    {
        return a.GetIngredientSet()
                .SetEquals(b.GetIngredientSet());
    }

    void DebugIngredientSet(IHasIngredients target, string label)
    {
        HashSet<int> set = target.GetIngredientSet();

        string result = string.Join(", ", set);

        Debug.Log($"{label} 재료 HashSet: [{result}]");
    }
}