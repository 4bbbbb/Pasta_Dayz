using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

            currentCustomer = FindObjectOfType<CustomerUI>();
            if (currentCustomer == null)
            {
                GameObject obj = Instantiate(customerUIPrefab, canvasTransform);
                currentCustomer = obj.GetComponent<CustomerUI>();
            }

            if (pendingResult.HasValue)
            {
                currentCustomer.Appear();
                currentCustomer.HideBubble();
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

        // Canvas 찾기
        if (canvasTransform == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                canvasTransform = canvas.transform;
            }                
            else
            {
                Debug.LogError("Canvas를 찾을 수 없음!");
                return;
            }
        }

        // CustomerUI 없으면 새로 생성
        if (currentCustomer == null)
        {
            GameObject obj = Instantiate(customerUIPrefab, canvasTransform);
            currentCustomer = obj.GetComponent<CustomerUI>();
        }

        currentOrder = generator.GenerateOrder();
        if (currentOrder == null)
        {
            return;
        }

        DebugIngredientSet(currentOrder, "손님 주문");

        string message = currentOrder.GetOrderText(generator.ingredientDB);

        currentCustomer.Appear();
        currentCustomer.ShowOrder(message);

        // 버튼은 숨김
        if (currentCustomer.yesButton != null)
            currentCustomer.yesButton.SetActive(false);
    }

    public void GoToKitchen()
    {
        // 현재 카운터에 있는 손님 Destroy
        if (currentCustomer != null)
        {
            Destroy(currentCustomer.gameObject);
            currentCustomer = null;
        }

        // 씬 전환
        SceneManager.LoadScene(2);
    }

    public void SubmitDish(PastaBox pastaBox)
    {
        if (currentOrder == null)
        {
            return;
        }            

        bool success = IsCorrect(pastaBox, currentOrder);

        Debug.Log(success ? "성공!" : "실패!");

        currentOrder = null;

        pendingResult = success;
    }

    IEnumerator ServeDishAndGoToNextCustomer(bool success)
    {
        yield return new WaitForSeconds(1f);

        // PastaBox 생성
        GameObject box = Instantiate(serveBoxPrefab, canvasTransform);

        yield return new WaitForSeconds(1f);

        // 결과 연출
        string resultMessage = success ? "성공!" : "실패!";
        currentCustomer.ShowResult(resultMessage);                   

        yield return new WaitForSeconds(2f);

        // 손님 제거
        if (currentCustomer != null)
        {
            Destroy(currentCustomer.gameObject);
            currentCustomer = null;
        }

        // PastaBox 제거
        Destroy(box);

        yield return new WaitForSeconds(2f);

        // 새 손님
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