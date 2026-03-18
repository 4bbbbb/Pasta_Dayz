using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;

public class Cooker_PastaCooker : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public class NoodlePrefabData
    {
        public int id;
        public GameObject prefab;
    }

    [SerializeField] private List<NoodlePrefabData> noodlePrefabs;

    [Header("<<스폰 위치>>")]
    [SerializeField] private Transform cookedNoodleSpawnPoint;

    [Header("쿠커 연출 대상")]
    [SerializeField] private Transform cookerVisual;

    [Header("쿠커 선택 연출")]
    [SerializeField] private Vector3 normalScale = Vector3.one;
    [SerializeField] private Vector3 selectedScale = new Vector3(1.17f, 1.17f, 1f);
    [SerializeField] private Vector3 selectedOffset = new Vector3(0f, 0.12f, 0f);
    [SerializeField] private float animDuration = 0.2f;

    private SpriteRenderer sr;
    private bool isCooking = false;

    private Vector3 originalLocalPos;
    private Coroutine visualRoutine;

    public bool CanBeSelected => false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (cookerVisual == null)
            cookerVisual = transform;

        originalLocalPos = cookerVisual.localPosition;
        cookerVisual.localScale = normalScale;
    }

    public bool Interact(IInteractable target)
    {
        if (isCooking)
        {
            Debug.Log($"{name}(이)가 이미 작동 중입니다!");
            return false;
        }

        if (target is Noodles noodles)
        {
            StartBowling(noodles);
            return true;
        }

        if (target == null)
        {
            Debug.Log("면을 선택해주세요");
            return false;
        }

        return false;
    }

    GameObject GetNoodlePrefab(int id)
    {
        foreach (var data in noodlePrefabs)
        {
            if (data.id == id)
                return data.prefab;
        }

        return null;
    }

    public void StartBowling(Noodles noodles)
    {
        OnBowling();
        StartCoroutine(BowlingRoutine(noodles));
    }

    IEnumerator BowlingRoutine(Noodles noodles)
    {
        for (int i = 1; i <= 7; i++)
        {
            yield return new WaitForSeconds(1f);
            Debug.Log($"{i}초...");
        }

        IngredientIDs id = noodles.GetComponent<IngredientIDs>();

        if (id != null)
        {
            GameObject prefab = GetNoodlePrefab(id.GetID());

            if (prefab != null)
            {
                GameObject cooked = Instantiate(
                    prefab,
                    cookedNoodleSpawnPoint.position,
                    Quaternion.identity,
                    cookedNoodleSpawnPoint
                );

                // spawnPoint 기준 위치 유지
                cooked.transform.position = cookedNoodleSpawnPoint.position;
                cooked.transform.rotation = Quaternion.identity;
                cooked.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

                Noodles_CookedNoodle cookedNoodle = cooked.GetComponent<Noodles_CookedNoodle>();
                if (cookedNoodle != null)
                {
                    cookedNoodle.SetPastaCooker(this);
                }
            }
        }

        StopBowling();
    }

    public void OnBowling()
    {
        isCooking = true;
        Debug.Log("면이 삶아지고 있습니다. 보글보글 oOoOO ....");
        sr.color = Color.cyan;
    }

    public void StopBowling()
    {
        isCooking = false;
        sr.color = Color.white;
        Debug.Log("면이 다 익었습니다 !");
    }

    public void OnCookedNoodleSelected()
    {
        PlayCookerAnimation(true);
    }

    public void OnCookedNoodleCanceled()
    {
        PlayCookerAnimation(false);
    }

    private void PlayCookerAnimation(bool selected)
    {
        if (visualRoutine != null)
            StopCoroutine(visualRoutine);

        Vector3 targetScale = selected ? selectedScale : normalScale;
        Vector3 targetPos = selected ? originalLocalPos + selectedOffset : originalLocalPos;

        visualRoutine = StartCoroutine(AnimateCooker(targetScale, targetPos));
    }

    private IEnumerator AnimateCooker(Vector3 targetScale, Vector3 targetPos)
    {
        Vector3 startScale = cookerVisual.localScale;
        Vector3 startPos = cookerVisual.localPosition;

        float time = 0f;

        while (time < animDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / animDuration);

            // 부드럽게
            t = t * t * (3f - 2f * t);

            cookerVisual.localScale = Vector3.Lerp(startScale, targetScale, t);
            cookerVisual.localPosition = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        cookerVisual.localScale = targetScale;
        cookerVisual.localPosition = targetPos;
        visualRoutine = null;
    }

    public void Cancel()
    {
    }
}