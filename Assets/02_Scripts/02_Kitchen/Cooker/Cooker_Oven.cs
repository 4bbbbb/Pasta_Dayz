using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Cooker_Oven : MonoBehaviour, IInteractable
{
    [Header("굽기 전 빠네")]    
    [SerializeField] public GameObject unbakedPanePrefab;

    [Header("<<구운 빠네 >>")]
    [SerializeField] public GameObject bakedPanePrefab;

    [Header("<<탄 빠네 >>")]
    [SerializeField] public GameObject burnedPanePrefab;

    [Header("굽기 전 파스타")]
    [SerializeField] public GameObject unbakedPastaPrefab;

    [Header("<<구운 파스타 >>")]
    [SerializeField] public GameObject bakedPastaPrefab;

    [Header("<<탄 파스타 >>")]
    [SerializeField] public GameObject burnedPastaPrefab;

    [Header("<< 빠네,파스타 스폰 위치>>")]
    [SerializeField] private Transform bakedSpawnPoint;

    [Header("오븐 스프라이트")]
    [SerializeField] public Sprite ovenSprite;
    [SerializeField] public Sprite ovenBakingSprite;
    [SerializeField] public Sprite ovenFinishSprite;

    private SpriteRenderer sr;

    private GameObject currentBakeObject;
    private Coroutine bakingCoroutine;

    private OvenState ovenState = OvenState.Empty;
    private BakeItemType bakeItem = BakeItemType.None;

    private HashSet<int> savedIngredientIDs;

    public bool CanBeSelected => false;
    public enum OvenState
    {
        Empty,
        Baking,
        Ready,
        Burned,
    }

    public enum BakeItemType
    {
        None,
        Pane,
        Pasta,
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public bool Interact(IInteractable target)
    {
        switch (ovenState)
        {
            case OvenState.Empty:
                sr.sprite = ovenSprite;
                return TryInsert(target);

            case OvenState.Baking:
                Debug.Log("오븐이 이미 작동 중입니다!");
                sr.sprite = ovenBakingSprite;
                return false;

            case OvenState.Ready:
                SpawnBaked();
                ResetOven();
                return true;

            case OvenState.Burned:
                RemoveBurnedFood();
                ResetOven();
                return true;
        }

        return false;
    }

    private bool TryInsert(IInteractable target)
    {     
        if (target == null)
        {
            Debug.Log("어떤걸 구울건가요?");
            return false;
        }

        switch (target)
        {
            case Plate_Pane pane:
                bakeItem = BakeItemType.Pane;                
                StartBaking();
                return true;

            case FinishedPasta pasta:

                HashSet<int> ids = pasta.GetIngredientSet();

                if (!ids.Contains(502))
                {
                    Debug.Log("오븐 전용 그릇에 담겨야 합니다!");
                    return false;
                }

                if (!ids.Contains(402))
                {
                    Debug.Log("모짜렐라 치즈가 필요합니다!");
                    return false;
                }

                savedIngredientIDs = new HashSet<int>(pasta.GetIngredientSet());

                Plates_OvenPlate plate = pasta.GetComponentInParent<Plates_OvenPlate>();

                if (plate != null)
                {
                    IngredientIDs plateID = plate.GetComponent<IngredientIDs>();
                    if (plateID != null)
                    {
                        savedIngredientIDs.Add(plateID.GetID());
                    }

                    Destroy(plate.gameObject);
                }

                bakeItem = BakeItemType.Pasta;
                StartBaking();
                return true;
        }

        return false;
    }

    public void StartBaking()
    {
        bakingCoroutine = StartCoroutine(BakingRoutine());
    }  

    IEnumerator BakingRoutine()
    {
        ovenState = OvenState.Baking;
        sr.sprite = ovenBakingSprite;

        GameObject prefab = null;

        switch (bakeItem)
        {
            case BakeItemType.Pane:
                prefab = unbakedPanePrefab;
                break;

            case BakeItemType.Pasta:
                prefab = unbakedPastaPrefab;
                break;
        }

        currentBakeObject = Instantiate(prefab, bakedSpawnPoint.position, Quaternion.identity, bakedSpawnPoint);

        for (int i = 1; i <= 8; i++)
        {
            yield return new WaitForSeconds(1f);
            Debug.Log($"{i}초...");
        }
                
        ovenState = OvenState.Ready;
        sr.sprite = ovenFinishSprite;
        Debug.Log("완료! 3초 안에 꺼내세요!");

        float timer = 0f;
        while (timer < 3f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        
        ovenState = OvenState.Burned;
        sr.sprite = ovenSprite;
        Debug.Log("타버렸습니다!");

        SpawnBurned();
    }

    private void SpawnBaked()
    {
        if (currentBakeObject != null)
        {
            Destroy(currentBakeObject);
            currentBakeObject = null;
        }

        GameObject prefab = null;

        switch (bakeItem)
        {
            case BakeItemType.Pane:
                prefab = bakedPanePrefab;
                break;

            case BakeItemType.Pasta:
                prefab = bakedPastaPrefab;
                break;
        }

        GameObject obj = Instantiate(prefab, bakedSpawnPoint.position, Quaternion.identity, bakedSpawnPoint);

        if (bakeItem == BakeItemType.Pasta)
        {
            BakedPasta baked = obj.GetComponent<BakedPasta>();
            baked.SetIngredients(savedIngredientIDs);

            baked.SetState(BakedPasta.BakedState.InOven); 
        }
    }

    private void SpawnBurned()
    {
        if (currentBakeObject != null)
        {
            Destroy(currentBakeObject);
            currentBakeObject = null;
        }

        GameObject prefab = null;

        switch (bakeItem)
        {
            case BakeItemType.Pane:
                prefab = burnedPanePrefab;
                break;

            case BakeItemType.Pasta:
                prefab = burnedPastaPrefab;
                break;
        }

        currentBakeObject = Instantiate(prefab, bakedSpawnPoint.position, Quaternion.identity, bakedSpawnPoint);
    }
    private void RemoveBurnedFood()
    {
        if (currentBakeObject != null)
        {
            Destroy(currentBakeObject);
            currentBakeObject = null;
        }

        Debug.Log("탄 음식을 치웠습니다!");
    }

    private void ResetOven()
    {
        if (bakingCoroutine != null)
        {
            StopCoroutine(bakingCoroutine);
            bakingCoroutine = null;
        }

        ovenState = OvenState.Empty;
        bakeItem = BakeItemType.None;
        sr.sprite = ovenSprite;
    }

    public void Cancel()
    {

    }

}
