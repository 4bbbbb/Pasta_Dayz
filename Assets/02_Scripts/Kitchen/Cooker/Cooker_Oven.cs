using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Cooker_Oven : MonoBehaviour, IInteractable
{
    [Header("<<구운 빠네 >>")]
    [SerializeField] public GameObject bakedPanePrefab;

    [Header("<<탄 빠네 >>")]
    [SerializeField] public GameObject burnedPanePrefab;

    [Header("<<구운 파스타 >>")]
    [SerializeField] public GameObject bakedPastaPrefab;

    [Header("<<탄 파스타 >>")]
    [SerializeField] public GameObject burnedPastaPrefab;

    [Header("<< 빠네,파스타 스폰 위치>>")]
    [SerializeField] private Transform bakedSpawnPoint;

    private SpriteRenderer sr;
    private Collider ovenCollider;

    private Coroutine bakingCoroutine;

    private OvenState ovenState = OvenState.Empty;
    private BakeItemType bakeItem = BakeItemType.None;

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
                return TryInsert(target);

            case OvenState.Baking:
                Debug.Log("오븐이 이미 작동 중입니다!");
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

                if (!pasta.IsOnOvenPlate())
                {
                    Debug.Log("오븐 전용 그릇에 담겨야 합니다!");
                    return false;
                }

                if (!pasta.HasMozzarella())
                {
                    Debug.Log("모짜렐라 치즈가 필요합니다!");
                    return false;
                }

                Plates_OvenPlate plate = pasta.GetComponentInParent<Plates_OvenPlate>();
                Destroy(plate.gameObject);

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
        sr.color = Color.cyan;
        for (int i = 1; i <= 8; i++)
        {
            yield return new WaitForSeconds(1f);
            Debug.Log($"{i}초...");
        }

        ovenState = OvenState.Ready;
        sr.color = Color.red;
        Debug.Log("완료! 3초 안에 꺼내세요!");

        float timer = 0f;
        while (timer < 3f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        ovenState = OvenState.Burned;
        sr.color = Color.black;
        Debug.Log("타버렸습니다!");

        SpawnBurned();
    }

    private void SpawnBaked()
    {
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

        Instantiate(prefab, bakedSpawnPoint.position, Quaternion.identity, bakedSpawnPoint);
    }

    private void SpawnBurned()
    {
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

        Instantiate(prefab, bakedSpawnPoint.position, Quaternion.identity, bakedSpawnPoint);
    }
    private void RemoveBurnedFood()
    {
        if (bakedSpawnPoint.childCount > 0)
        {
            Destroy(bakedSpawnPoint.GetChild(0).gameObject);
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
        sr.color = Color.white;
    }

    public void Cancel()
    {

    }

}
