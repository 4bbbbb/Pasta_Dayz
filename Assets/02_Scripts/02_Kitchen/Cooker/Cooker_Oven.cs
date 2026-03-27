using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Cooker_Oven : MonoBehaviour, IInteractable
{
    [Header("굽기 전 빠네")]
    [SerializeField] public GameObject unbakedPanePrefab;

    [Header("구운 빠네")]
    [SerializeField] public GameObject bakedPanePrefab;

    [Header("탄 빠네")]
    [SerializeField] public GameObject burnedPanePrefab;

    [Header("굽기 전 파스타")]
    [SerializeField] public GameObject unbakedPastaPrefab;

    [Header("구운 파스타")]
    [SerializeField] public GameObject bakedPastaPrefab;

    [Header("탄 파스타")]
    [SerializeField] public GameObject burnedPastaPrefab;

    [Header("스폰 위치")]
    [SerializeField] private Transform bakedSpawnPoint;

    [Header("오븐 스프라이트")]
    [SerializeField] public Sprite ovenSprite;
    [SerializeField] public Sprite ovenBakingSprite;
    [SerializeField] public Sprite ovenFinishSprite;

    [Header("오븐 콜라이더 (3D!)")]
    [SerializeField] private Collider ovenCollider;

    [Header("벗어난 판정 거리")]
    [SerializeField] private float releaseDistance = 0.2f;

    private SpriteRenderer sr;

    private GameObject currentBakeObject;
    private Coroutine bakingCoroutine;
    private Coroutine colliderWaitCoroutine;

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

        if (ovenCollider == null)
            ovenCollider = GetComponent<Collider>(); // 🔥 3D로 변경
    }

    public bool Interact(IInteractable target)
    {
        switch (ovenState)
        {
            case OvenState.Empty:
                sr.sprite = ovenSprite;
                return TryInsert(target);

            case OvenState.Baking:
                Debug.Log("오븐 작동 중");
                return false;

            case OvenState.Ready:
                Debug.Log("꺼냄!");
                ReleaseBakedFood();
                return true;

            case OvenState.Burned:
                return true;
        }

        return false;
    }

    private bool TryInsert(IInteractable target)
    {
        if (target == null) return false;

        switch (target)
        {
            case Plate_Pane pane:
                bakeItem = BakeItemType.Pane;
                savedIngredientIDs = null;
                StartBaking();
                return true;

            case FinishedPasta pasta:
                HashSet<int> ids = pasta.GetIngredientSet();

                if (!ids.Contains(502) || !ids.Contains(402))
                    return false;

                savedIngredientIDs = new HashSet<int>(ids);

                Plates_OvenPlate plate = pasta.GetComponentInParent<Plates_OvenPlate>();
                if (plate != null)
                {
                    IngredientIDs plateID = plate.GetComponent<IngredientIDs>();
                    if (plateID != null)
                        savedIngredientIDs.Add(plateID.GetID());

                    Destroy(plate.gameObject);
                }

                Destroy(pasta.gameObject);  

                bakeItem = BakeItemType.Pasta;
                StartBaking();
                return true;
        }

        return false;
    }

    public void StartBaking()
    {
        if (bakingCoroutine != null)
            StopCoroutine(bakingCoroutine);

        bakingCoroutine = StartCoroutine(BakingRoutine());
    }

    IEnumerator BakingRoutine()
    {
        ovenState = OvenState.Baking;
        sr.sprite = ovenBakingSprite;

        GameObject prefab = (bakeItem == BakeItemType.Pane)
            ? unbakedPanePrefab
            : unbakedPastaPrefab;

        currentBakeObject = Instantiate(prefab, bakedSpawnPoint.position, Quaternion.identity, bakedSpawnPoint);

        yield return new WaitForSeconds(8f);

        SpawnBaked();

        SetCurrentFoodPickable(false); // 🔥 Ready에서도 클릭 금지

        ovenState = OvenState.Ready;
        sr.sprite = ovenFinishSprite;

        float timer = 0f;
        while (timer < 3f && ovenState == OvenState.Ready)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (ovenState != OvenState.Ready) yield break;

        ovenState = OvenState.Burned;
        sr.sprite = ovenSprite;

        SpawnBurned();
    }

    private void SetCurrentFoodPickable(bool canPick)
    {
        if (currentBakeObject == null) return;

        // 음식 스크립트에 맡김 
        var pasta = currentBakeObject.GetComponent<BakedPasta>();
        if (pasta != null)
        {
            pasta.SetPickable(canPick);
            return;
        }

        var pane = currentBakeObject.GetComponent<Plate_BakedPane>();
        if (pane != null)
        {
            pane.SetPickable(canPick);
            return;
        }
    }

    private void SpawnBaked()
    {
        if (currentBakeObject != null)
            Destroy(currentBakeObject);

        GameObject prefab = (bakeItem == BakeItemType.Pane)
            ? bakedPanePrefab
            : bakedPastaPrefab;

        currentBakeObject = Instantiate(prefab, bakedSpawnPoint.position, Quaternion.identity, bakedSpawnPoint);

        SetCurrentFoodPickable(false); // 🔥 무조건 막기

        if (bakeItem == BakeItemType.Pasta)
        {
            var baked = currentBakeObject.GetComponent<BakedPasta>();
            if (baked != null)
            {
                baked.SetIngredients(savedIngredientIDs);
                baked.SetPickable(false);
            }
        }
    }

    private void ReleaseBakedFood()
    {
        Debug.Log("꺼냄 → 클릭 가능");

        SetCurrentFoodPickable(true);

        ovenState = OvenState.Empty;
        bakeItem = BakeItemType.None;
        sr.sprite = ovenSprite;

        if (ovenCollider != null)
            ovenCollider.enabled = false;

        if (colliderWaitCoroutine != null)
            StopCoroutine(colliderWaitCoroutine);

        colliderWaitCoroutine = StartCoroutine(WaitUntilFoodLeavesOven());
    }

    private void SpawnBurned()
    {
        if (currentBakeObject != null)
            Destroy(currentBakeObject);

        GameObject prefab = (bakeItem == BakeItemType.Pane)
            ? burnedPanePrefab
            : burnedPastaPrefab;

        currentBakeObject = Instantiate(prefab, bakedSpawnPoint.position, Quaternion.identity, bakedSpawnPoint);

        if (bakeItem == BakeItemType.Pasta)
        {
            Burned burned = currentBakeObject.GetComponent<Burned>();
            if (burned != null)
            {
                burned.SetIngredients(savedIngredientIDs);
            }
        }

        SetCurrentFoodPickable(false);
    }

    private IEnumerator WaitUntilFoodLeavesOven()
    {
        while (currentBakeObject != null)
        {
            bool movedAway =
                Vector3.Distance(currentBakeObject.transform.position, bakedSpawnPoint.position) > releaseDistance;

            bool parentChanged =
                currentBakeObject.transform.parent != bakedSpawnPoint;

            if (movedAway || parentChanged)
                break;

            yield return null;
        }

        if (ovenCollider != null)
            ovenCollider.enabled = true;
    }
    public void OnBurnedRemoved()
    {
        currentBakeObject = null;
        ResetOven();
    }

    private void ResetOven()
    {
        if (bakingCoroutine != null)
            StopCoroutine(bakingCoroutine);

        if (colliderWaitCoroutine != null)
            StopCoroutine(colliderWaitCoroutine);

        if (ovenCollider != null)
            ovenCollider.enabled = true;

        ovenState = OvenState.Empty;
        bakeItem = BakeItemType.None;
        sr.sprite = ovenSprite;
    }

    public void Cancel() { }
}
