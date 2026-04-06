using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;
using static Sauces;
using static Topping;

public class Cooker_FryingPan : MonoBehaviour, IInteractable
{
    [Header("<<가스스토브>>")]
    [SerializeField] private Cooker_GasStove gasStove;

    [Header("토핑 그룹 - 팬 위")]
    [SerializeField] private Transform[] toppingGroupParents;

    [Header("<<완성 파스타 / 면 위치>>")]
    [SerializeField] private Transform finishedPastaSpawnPoint;
    [SerializeField] private Transform noodleSpawnPoint;

    [Header("<<오일 스프라이트>>")]
    [SerializeField] private GameObject oilOffSprite;
    [SerializeField] private GameObject oilOnSprite;
    [SerializeField] private Shader_Spread oilSpreadEffect;

    [Header("<<소스 스프라이트>>")]
    [SerializeField] private GameObject tomatoSauceSprite;
    [SerializeField] private GameObject creamSauceSprite;
    [SerializeField] private GameObject roseSauceSprite;
    [SerializeField] private GameObject vongoleSauceSprite;
    [SerializeField] private Shader_Spread tomatoEffect;
    [SerializeField] private Shader_Spread creamEffect;
    [SerializeField] private Shader_Spread vongoleEffect;

    [Header("<<완성 파스타>>")]
    [SerializeField] private GameObject finishedPastaPrefab;

    [Header("<<면 프리팹 삭제 이펙트 속도>>")]
    [SerializeField] private float noodleFadeDuration = 0.25f;

    [Header("<<프라잉 사운드>>")]
    [SerializeField] private AudioSource fryingAudioSource;
    [SerializeField] private AudioClip fryingLoopClip;
    [SerializeField] private float fryingFadeOutDuration = 0.7f;
    [SerializeField] private float fryingSoundEndEarly = 0.6f;

    [Header("<<소스 붓기 타겟>>")]
    [SerializeField] private Transform saucePourTarget;

    [Header("<<선택 연출>>")]
    [SerializeField] private float selectScaleDuration = 0.12f;
    [SerializeField] private float selectedScaleMultiplier = 1.08f;

    [Header("<<쓰레기 이펙트>>")]
    [SerializeField] private float trashEffectDuration = 0.22f;
    [SerializeField] private float trashFinalScaleMultiplier = 0.2f;
    [SerializeField] private Vector3 trashFadeOffset = new Vector3(0f, -0.15f, 0f);

    [Header("<<드래그 이동>>")]
    [SerializeField] private float dragLiftScaleMultiplier = 1.08f;
    [SerializeField] private float dragStartThreshold = 0.12f;

    private bool isPointerDown = false;
    private bool hasStartedRealDrag = false;

    private Vector3 dragStartWorldPos;
    private Vector3 dragStartLocalPos;
    private Vector3 dragOffset;
    private float dragScreenZ;
    private Vector3 mouseDownWorldPos;

    private bool hasOil = false;
    private bool isCooking = false;
    private bool hasFinishedPastaOnPan = false;

    private readonly HashSet<ToppingType> addedToppings = new HashSet<ToppingType>();
    private readonly HashSet<SauceType> addedSauces = new HashSet<SauceType>();
    private readonly HashSet<int> ingredientIDs = new HashSet<int>();

    private Coroutine fryingSoundFadeRoutine;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Vector3 originalScale;

    public bool isSelected { get; private set; }
    public bool isBeingTrashed { get; private set; } = false;

    public bool CanBeSelected => ingredientIDs.Count > 0 && !isBeingTrashed && !isCooking && !hasFinishedPastaOnPan;

    void Awake()
    {
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
        originalScale = transform.localScale;
    }

    void Start()
    {
        if (fryingAudioSource == null)
            fryingAudioSource = GetComponent<AudioSource>();

        if (fryingAudioSource == null)
            fryingAudioSource = gameObject.AddComponent<AudioSource>();

        fryingAudioSource.playOnAwake = false;
        fryingAudioSource.loop = true;
        fryingAudioSource.clip = fryingLoopClip;

        SyncSfxVolume();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.OnSfxVolumeChanged += OnSfxVolumeChanged;
            SoundManager.Instance.OnMasterVolumeChanged += OnMasterVolumeChanged;
        }
    }

    void OnDestroy()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.OnSfxVolumeChanged -= OnSfxVolumeChanged;
            SoundManager.Instance.OnMasterVolumeChanged -= OnMasterVolumeChanged;
        }
    }

    public void PrepareForReuse()
    {
        transform.DOKill();
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;
        transform.localScale = originalScale;

        isSelected = false;
        isBeingTrashed = false;
        isCooking = false;
        hasFinishedPastaOnPan = false;

        isPointerDown = false;
        hasStartedRealDrag = false;

        StopAllCoroutines();
        fryingSoundFadeRoutine = null;

        ResetState();
        RestoreRenderers();
        RestoreColliders();
        ClearPanChildren();

        if (fryingAudioSource != null)
        {
            fryingAudioSource.Stop();
            SyncSfxVolume();
        }
    }

    private void RestoreRenderers()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (var r in renderers)
        {
            if (r == null) continue;

            Color c = r.color;
            c.a = 1f;
            r.color = c;
        }
    }

    private void RestoreColliders()
    {
        Collider[] cols = GetComponentsInChildren<Collider>(true);

        foreach (var col in cols)
        {
            if (col != null)
                col.enabled = true;
        }
    }

    private void ClearPanChildren()
    {
        if (toppingGroupParents != null)
        {
            foreach (Transform groupParent in toppingGroupParents)
            {
                if (groupParent == null) continue;

                foreach (Transform point in groupParent)
                {
                    for (int i = point.childCount - 1; i >= 0; i--)
                    {
                        GameObject obj = point.GetChild(i).gameObject;
                        if (obj != null)
                        {
                            obj.SetActive(false);
                            Destroy(obj);
                        }
                    }
                }
            }
        }

        if (noodleSpawnPoint != null)
        {
            for (int i = noodleSpawnPoint.childCount - 1; i >= 0; i--)
            {
                GameObject obj = noodleSpawnPoint.GetChild(i).gameObject;
                if (obj != null)
                {
                    obj.SetActive(false);
                    Destroy(obj);
                }
            }
        }

        if (finishedPastaSpawnPoint != null)
        {
            for (int i = finishedPastaSpawnPoint.childCount - 1; i >= 0; i--)
            {
                GameObject obj = finishedPastaSpawnPoint.GetChild(i).gameObject;
                if (obj != null)
                {
                    obj.SetActive(false);
                    Destroy(obj);
                }
            }
        }
    }

    public bool Interact(IInteractable target)
    {
        if (isBeingTrashed)
            return false;

        if (target == null)
        {
            if (ingredientIDs.Count == 0) return false;
            if (isCooking) return false;
            if (hasFinishedPastaOnPan) return false;

            Select();
            return true;
        }

        if (isCooking || hasFinishedPastaOnPan)
            return false;

        if (target is Topping_OliveOil oil)
            return AddOil(oil);

        if (target is Topping topping)
            return AddTopping(topping);

        if (target is Sauces sauce)
            return AddSauce(sauce);

        if (target is Noodles_CookedNoodle noodle)
            return AddNoodle(noodle);

        return false;
    }

    public float GetPanContentCost()
    {
        if (IngredientDatabase.Instance == null)
            return 0f;

        float total = 0f;

        foreach (int id in ingredientIDs)
        {
            IngredientData data = IngredientDatabase.Instance.GetIngredient(id);
            if (data != null)
                total += data.ingredientCost;
        }

        return total;
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (Camera.main == null)
            return transform.position;

        Vector3 mouse = Input.mousePosition;
        mouse.z = dragScreenZ;

        Vector3 world = Camera.main.ScreenToWorldPoint(mouse);
        world.z = dragStartWorldPos.z;
        return world;
    }

    private void OnMouseDown()
    {
        if (!CanBeSelected || isBeingTrashed)
            return;

        if (Camera.main == null)
            return;

        isPointerDown = true;
        hasStartedRealDrag = false;

        dragStartWorldPos = transform.position;
        dragStartLocalPos = transform.localPosition;
        dragScreenZ = Camera.main.WorldToScreenPoint(transform.position).z;

        mouseDownWorldPos = GetMouseWorldPosition();
        dragOffset = transform.position - mouseDownWorldPos;
    }

    private void OnMouseDrag()
    {
        if (!isPointerDown)
            return;

        Vector3 currentMouseWorld = GetMouseWorldPosition();

        if (!hasStartedRealDrag)
        {
            float dragDistance = Vector3.Distance(currentMouseWorld, mouseDownWorldPos);
            if (dragDistance >= dragStartThreshold)
            {
                hasStartedRealDrag = true;
                isSelected = false;

                transform.DOKill();
                transform.localScale = originalScale * dragLiftScaleMultiplier;
            }
        }

        if (!hasStartedRealDrag)
            return;

        transform.position = currentMouseWorld + dragOffset;
    }

    private void OnMouseUp()
    {
        if (!isPointerDown)
            return;

        isPointerDown = false;

        if (!hasStartedRealDrag)
        {
            Select();
            return;
        }

        hasStartedRealDrag = false;

        bool trashed = TryDropTrashcan();

        if (!trashed)
        {
            transform.position = dragStartWorldPos;
            transform.localPosition = dragStartLocalPos;
            transform.localRotation = originalLocalRotation;
            transform.localScale = originalScale;
        }
    }

    private bool TryDropTrashcan()
    {
        if (Camera.main == null)
            return false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
            return false;

        Cooker_Trashcan trashcan = hit.collider.GetComponentInParent<Cooker_Trashcan>();
        if (trashcan != null)
            return trashcan.Interact(this);

        return false;
    }

    public void OnTrashed()
    {
        isBeingTrashed = true;
        isSelected = false;

        StopFryingSoundWithFade();

        transform.DOKill();
        transform.localScale = originalScale;

        Collider[] cols = GetComponentsInChildren<Collider>(true);
        foreach (var col in cols)
        {
            if (col != null)
                col.enabled = false;
        }

        if (gasStove != null)
            gasStove.DestroyFryingPan();
    }

    public void PlayTrashEffect(Transform trashTarget)
    {
        transform.DOKill();

        if (trashTarget != null)
            transform.position = trashTarget.position + trashFadeOffset;

        Sequence seq = DOTween.Sequence();

        seq.Append(
            transform.DOScale(originalScale * trashFinalScaleMultiplier, trashEffectDuration)
                     .SetEase(Ease.InQuad)
        );

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var r in renderers)
        {
            if (r != null)
                seq.Join(r.DOFade(0f, trashEffectDuration));
        }

        seq.OnComplete(() =>
        {
            PrepareForReuse();

            if (gasStove != null)
                gasStove.DestroyFryingPan();
            else
                gameObject.SetActive(false);
        });
    }

    private void Select()
    {
        isSelected = true;
        transform.DOKill();
        transform.DOScale(originalScale * selectedScaleMultiplier, selectScaleDuration)
                 .SetEase(Ease.OutBack);
    }

    public void Cancel()
    {
        isSelected = false;
        transform.DOKill();
        transform.DOScale(originalScale, selectScaleDuration)
                 .SetEase(Ease.OutQuad);
    }

    private bool AddOil(Topping_OliveOil oil)
    {
        if (!IsKitchenActionAllowed(TutorialController.KitchenPracticeTarget.DragOilToPan))
            return false;

        if (hasOil) return false;

        hasOil = true;

        if (gasStove != null)
            gasStove.TurnOn();

        oilOnSprite.SetActive(true);

        oil.PlayPourOnPanAnimation();

        DOVirtual.DelayedCall(0.5f, () =>
        {
            if (this != null && gameObject.activeInHierarchy && oilSpreadEffect != null)
            {
                oilSpreadEffect.PlayOilSpread(() =>
                {
                    if (this != null && gameObject.activeInHierarchy && oilOffSprite != null)
                        oilOffSprite.SetActive(false);
                });
            }
        });

        IngredientIDs id = oil.GetComponent<IngredientIDs>();
        if (id != null)
            ingredientIDs.Add(id.GetID());

        ConsumeKitchenAction(TutorialController.KitchenPracticeTarget.DragOilToPan);
        return true;
    }


    private bool AddTopping(Topping topping)
    {
        if (!IsKitchenActionAllowed(TutorialController.KitchenPracticeTarget.DragGarlicToPan))
            return false;

        IngredientIDs id = topping.GetComponent<IngredientIDs>();
        if (id == null) return false;

        if (IsFirstKitchenTutorialActive() && id.GetID() != 302)
            return false;

        if (addedToppings.Count >= 2) return false;
        if (addedToppings.Contains(topping.toppingType)) return false;

        int groupIndex = addedToppings.Count;
        Transform[] spawnPoints = GetSpawnPointsFromGroup(groupIndex);

        if (spawnPoints == null || spawnPoints.Length != 3)
            return false;

        addedToppings.Add(topping.toppingType);
        ingredientIDs.Add(id.GetID());

        topping.Cancel();
        StartCoroutine(SpawnToppingBurst(id.GetID(), spawnPoints));

        ConsumeKitchenAction(TutorialController.KitchenPracticeTarget.DragGarlicToPan);
        return true;
    }



    private Transform[] GetSpawnPointsFromGroup(int groupIndex)
    {
        if (toppingGroupParents == null || groupIndex < 0 || groupIndex >= toppingGroupParents.Length)
            return null;

        Transform parent = toppingGroupParents[groupIndex];
        if (parent == null || parent.childCount < 3)
            return null;

        Transform[] points = new Transform[3];
        for (int i = 0; i < 3; i++)
            points[i] = parent.GetChild(i);

        return points;
    }

    private IEnumerator SpawnToppingBurst(int ingredientID, Transform[] spawnPoints)
    {
        List<Transform> pointList = new List<Transform>(spawnPoints);
        ShufflePoints(pointList);

        foreach (Transform point in pointList)
        {
            if (point == null) continue;

            float dropDuration = Random.Range(0.18f, 0.38f);
            SpawnIngredientByIDWithDrop(ingredientID, point, dropDuration);

            float nextDelay = Random.Range(0.05f, 0.18f);
            yield return new WaitForSeconds(nextDelay);
        }
    }

    private void ShufflePoints(List<Transform> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            Transform temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    private bool AddSauce(Sauces sauce)
    {
        if (IsFirstKitchenTutorialActive())
            return false;


        IngredientIDs id = sauce.GetComponent<IngredientIDs>();
        if (id == null) return false;

        int newID = id.GetID();

        if (!CanAcceptSauce(newID))
            return false;

        ApplySauceDataByID(newID, sauce.sauceType);

        sauce.PlayPourToPanAnimation(GetSaucePourWorldPos());

        DOVirtual.DelayedCall(1.15f, () =>
        {
            if (this != null && gameObject.activeInHierarchy)
            {
                int showID = ingredientIDs.Contains(204) ? 204 : newID;
                ShowSauceSprite(showID);
            }
        });

        return true;
    }

    private void ApplySauceDataByID(int newID, SauceType sauceType)
    {
        if (ingredientIDs.Contains(202) && newID == 203)
        {
            ingredientIDs.Remove(202);
            ingredientIDs.Add(204);

            addedSauces.Clear();
            addedSauces.Add(SauceType.Rose);

            oilOffSprite.SetActive(false);
            oilOnSprite.SetActive(false);
            return;
        }

        if (ingredientIDs.Contains(203) && newID == 202)
        {
            ingredientIDs.Remove(203);
            ingredientIDs.Add(204);

            addedSauces.Clear();
            addedSauces.Add(SauceType.Rose);

            oilOffSprite.SetActive(false);
            oilOnSprite.SetActive(false);
            return;
        }

        addedSauces.Add(sauceType);
        ingredientIDs.Add(newID);
    }

    private bool CanAcceptSauce(int newID)
    {
        if (ingredientIDs.Contains(204))
            return false;

        if ((ingredientIDs.Contains(202) && newID == 203) ||
            (ingredientIDs.Contains(203) && newID == 202))
        {
            return true;
        }

        if (addedSauces.Count >= 1)
            return false;

        return true;
    }

    private Vector3 GetSaucePourWorldPos()
    {
        if (saucePourTarget != null)
            return saucePourTarget.position;

        return transform.position;
    }

    private void ShowSauceSprite(int sauceID)
    {
        tomatoSauceSprite.SetActive(false);
        creamSauceSprite.SetActive(false);
        roseSauceSprite.SetActive(false);
        vongoleSauceSprite.SetActive(false);

        switch (sauceID)
        {
            case 202:
                tomatoSauceSprite.SetActive(true);
                if (tomatoEffect != null) tomatoEffect.PlayOilSpread();
                break;

            case 203:
                creamSauceSprite.SetActive(true);
                if (creamEffect != null) creamEffect.PlayOilSpread();
                break;

            case 204:
                roseSauceSprite.SetActive(true);
                break;

            case 205:
                vongoleSauceSprite.SetActive(true);
                if (vongoleEffect != null) vongoleEffect.PlayOilSpread();
                break;
        }
    }

    private bool AddNoodle(Noodles_CookedNoodle cookedNoodle)
    {
        if (!IsKitchenActionAllowed(TutorialController.KitchenPracticeTarget.DragCookedNoodleToPan))
            return false;

        if (!hasOil) return false;
        if (noodleSpawnPoint == null) return false;
        if (noodleSpawnPoint.childCount > 0) return false;

        IngredientIDs id = cookedNoodle.GetComponent<IngredientIDs>();
        if (id == null) return false;

        if (IsFirstKitchenTutorialActive() && id.GetID() != 101)
            return false;

        SpawnIngredientByID(id.GetID(), noodleSpawnPoint);
        Destroy(cookedNoodle.gameObject);

        StartCoroutine(CookRoutine());

        ConsumeKitchenAction(TutorialController.KitchenPracticeTarget.DragCookedNoodleToPan);
        return true;
    }


    private void SpawnIngredientByID(int ingredientID, Transform spawnPoint)
    {
        GameObject prefab = IngredientDatabase.Instance.GetPrefab(ingredientID);
        if (prefab == null) return;

        GameObject obj = Instantiate(prefab, spawnPoint.position, Quaternion.identity, spawnPoint);

        ingredientIDs.Add(ingredientID);

        if (ingredientID >= 301 && ingredientID <= 313)
        {
            Vector3 startPos = spawnPoint.position + Vector3.up * 1.2f;
            obj.transform.position = startPos;

            obj.transform.localScale = Vector3.one * 0.8f;

            obj.transform.DOMove(spawnPoint.position, 0.35f).SetEase(Ease.OutQuad);
            obj.transform.DORotate(
                new Vector3(0f, 0f, Random.Range(-120f, 120f)),
                0.35f,
                RotateMode.FastBeyond360
            );
            obj.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutQuad);
        }
    }

    private void SpawnIngredientByIDWithDrop(int ingredientID, Transform spawnPoint, float dropDuration)
    {
        GameObject prefab = IngredientDatabase.Instance.GetPrefab(ingredientID);
        if (prefab == null) return;

        GameObject obj = Instantiate(prefab, spawnPoint.position, Quaternion.identity, spawnPoint);

        if (ingredientID >= 301 && ingredientID <= 313)
        {
            Vector3 endPos = spawnPoint.position;

            float randomX = Random.Range(-0.15f, 0.15f);
            float randomY = Random.Range(1.0f, 1.35f);

            Vector3 startPos = endPos + new Vector3(randomX, randomY, 0f);
            obj.transform.position = startPos;
            obj.transform.localScale = Vector3.one * Random.Range(0.82f, 0.92f);

            float rotZ = Random.Range(-160f, 160f);
            obj.transform.rotation = Quaternion.Euler(0f, 0f, rotZ);

            Sequence seq = DOTween.Sequence();

            seq.Join(
                obj.transform.DOMove(endPos, dropDuration)
                    .SetEase(Ease.InQuad)
            );

            seq.Join(
                obj.transform.DOScale(Vector3.one, dropDuration)
                    .SetEase(Ease.OutQuad)
            );

            seq.OnComplete(() =>
            {
                if (obj != null)
                {
                    obj.transform.DOPunchPosition(
                        new Vector3(0f, -0.05f, 0f),
                        0.12f,
                        4,
                        0.4f
                    );

                    obj.transform.DOPunchScale(
                        new Vector3(0.08f, 0.08f, 0f),
                        0.12f,
                        4,
                        0.4f
                    );
                }
            });
        }
    }

    private IEnumerator CookRoutine()
    {
        isCooking = true;
        PlayFryingSound();

        Vector3 originalPanPos = transform.localPosition;
        Vector3 panMoveDir = transform.localRotation * Vector3.up;

        float cycleCount = 20.25f;
        float cycleDuration = 0.25f;
        float totalTime = cycleCount * cycleDuration;

        float panMoveAmount = 0.20f;
        float elapsed = 0f;

        Dictionary<Transform, Vector3> ingredientOriginalLocalPos = new Dictionary<Transform, Vector3>();

        if (toppingGroupParents != null)
        {
            foreach (Transform groupParent in toppingGroupParents)
            {
                if (groupParent == null) continue;

                foreach (Transform point in groupParent)
                {
                    foreach (Transform child in point)
                    {
                        ingredientOriginalLocalPos[child] = child.localPosition;
                    }
                }
            }
        }

        if (noodleSpawnPoint != null)
        {
            foreach (Transform child in noodleSpawnPoint)
            {
                ingredientOriginalLocalPos[child] = child.localPosition;
            }
        }

        bool fryingStopped = false;

        while (elapsed < totalTime)
        {
            elapsed += Time.deltaTime;

            if (!fryingStopped && elapsed >= totalTime - fryingSoundEndEarly)
            {
                StopFryingSoundWithFade();
                fryingStopped = true;
            }

            float t = Mathf.Clamp01(elapsed / totalTime);
            float angle = t * cycleCount * Mathf.PI * 2f;

            float envelope = 1f;
            if (t > 0.7f)
            {
                float fadeT = (t - 0.7f) / 0.3f;
                envelope = Mathf.SmoothStep(1f, 0f, fadeT);
            }

            float panOffset = Mathf.Sin(angle) * panMoveAmount * envelope;
            transform.localPosition = originalPanPos + panMoveDir * panOffset;

            foreach (var pair in ingredientOriginalLocalPos)
            {
                Transform ingredient = pair.Key;
                if (ingredient == null) continue;

                Vector3 basePos = pair.Value;
                float phase = ingredient.GetInstanceID() * 0.01f;

                float offsetY = Mathf.Sin(angle - 0.6f + phase) * 0.08f * envelope;
                float offsetX = Mathf.Cos(angle * 1.2f + phase) * 0.04f * envelope;

                ingredient.localPosition = basePos + new Vector3(offsetX, offsetY, 0f);
            }

            yield return null;
        }

        transform.localPosition = originalPanPos;

        foreach (var pair in ingredientOriginalLocalPos)
        {
            if (pair.Key != null)
                pair.Key.localPosition = pair.Value;
        }

        if (finishedPastaPrefab == null || finishedPastaSpawnPoint == null)
        {
            isCooking = false;
            if (gasStove != null)
                gasStove.TurnOff();
            yield break;
        }

        GameObject finishedPasta = Instantiate(finishedPastaPrefab, finishedPastaSpawnPoint);
        finishedPasta.transform.localPosition = Vector3.zero;
        finishedPasta.transform.localRotation = Quaternion.identity;
        finishedPasta.transform.localScale = Vector3.one;

        FinishedPasta pasta = finishedPasta.GetComponent<FinishedPasta>();
        if (pasta != null)
        {
            pasta.SetIngredients(new HashSet<int>(ingredientIDs));
            pasta.Init(gasStove);
            pasta.SetFryingPan(this);
            pasta.PreparePanSpriteHidden();

            StartCoroutine(FadeOutAndDestroyNoodle());
            yield return StartCoroutine(pasta.FadeInCurrentSprite());
        }

        ClearPanForFinishedPasta();

        hasFinishedPastaOnPan = true;
        isCooking = false;

        if (gasStove != null)
            gasStove.TurnOff();
    }

    public void CopyPanToppingsToFinishedPasta(FinishedPasta targetPasta)
    {
        if (targetPasta == null)
            return;

        targetPasta.BuildPlateToppingsFromPan(toppingGroupParents);
    }

    private IEnumerator FadeOutAndDestroyNoodle()
    {
        if (noodleSpawnPoint == null || noodleSpawnPoint.childCount == 0)
            yield break;

        Transform noodle = noodleSpawnPoint.GetChild(0);
        if (noodle == null)
            yield break;

        SpriteRenderer sr = noodle.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Destroy(noodle.gameObject);
            yield break;
        }

        Color original = sr.color;

        float t = 0f;
        while (t < noodleFadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / noodleFadeDuration);
            sr.color = new Color(original.r, original.g, original.b, a);
            yield return null;
        }

        sr.color = new Color(original.r, original.g, original.b, 0f);
        Destroy(noodle.gameObject);
    }

    private void ClearPanForFinishedPasta()
    {
        oilOffSprite.SetActive(false);
        oilOnSprite.SetActive(false);

        tomatoSauceSprite.SetActive(false);
        creamSauceSprite.SetActive(false);
        roseSauceSprite.SetActive(false);
        vongoleSauceSprite.SetActive(false);
    }

    public void ClearPanAfterServing()
    {
        if (toppingGroupParents != null)
        {
            foreach (Transform groupParent in toppingGroupParents)
            {
                if (groupParent == null) continue;

                foreach (Transform point in groupParent)
                {
                    for (int i = point.childCount - 1; i >= 0; i--)
                    {
                        Destroy(point.GetChild(i).gameObject);
                    }
                }
            }
        }

        if (finishedPastaSpawnPoint != null && finishedPastaSpawnPoint.childCount > 0)
        {
            Transform child = finishedPastaSpawnPoint.GetChild(0);
            if (child != null)
                child.SetParent(null, true);
        }

        ResetState();
        hasFinishedPastaOnPan = false;
        isSelected = false;
        transform.localScale = originalScale;
    }

    private void ResetState()
    {
        addedToppings.Clear();
        addedSauces.Clear();
        ingredientIDs.Clear();

        hasOil = false;

        oilOffSprite.SetActive(true);
        oilOnSprite.SetActive(false);

        tomatoSauceSprite.SetActive(false);
        creamSauceSprite.SetActive(false);
        roseSauceSprite.SetActive(false);
        vongoleSauceSprite.SetActive(false);

        if (oilSpreadEffect != null)
            oilSpreadEffect.HideOil();

        if (tomatoEffect != null)
            tomatoEffect.HideOil();

        if (creamEffect != null)
            creamEffect.HideOil();

        if (vongoleEffect != null)
            vongoleEffect.HideOil();
    }

    public void TransferPanToppingsToFinishedPasta(FinishedPasta targetPasta)
    {
        if (targetPasta == null)
            return;

        targetPasta.MovePanToppingsForDrag(toppingGroupParents);
    }

    private void PlayFryingSound()
    {
        if (fryingAudioSource == null || fryingLoopClip == null)
            return;

        if (fryingSoundFadeRoutine != null)
        {
            StopCoroutine(fryingSoundFadeRoutine);
            fryingSoundFadeRoutine = null;
        }

        fryingAudioSource.clip = fryingLoopClip;
        fryingAudioSource.loop = true;

        SyncSfxVolume();

        if (!fryingAudioSource.isPlaying)
            fryingAudioSource.Play();
    }

    private void StopFryingSoundWithFade()
    {
        if (fryingAudioSource == null || !fryingAudioSource.isPlaying)
            return;

        if (fryingSoundFadeRoutine != null)
            StopCoroutine(fryingSoundFadeRoutine);

        fryingSoundFadeRoutine = StartCoroutine(FadeOutFryingSound());
    }

    private IEnumerator FadeOutFryingSound()
    {
        float startVolume = fryingAudioSource.volume;
        float time = 0f;

        while (time < fryingFadeOutDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fryingFadeOutDuration);
            fryingAudioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        fryingAudioSource.Stop();
        SyncSfxVolume();
        fryingSoundFadeRoutine = null;
    }

    private void OnSfxVolumeChanged(float value)
    {
        if (fryingAudioSource == null)
            return;

        if (fryingSoundFadeRoutine == null)
            SyncSfxVolume();
    }

    private void OnMasterVolumeChanged(float value)
    {
        if (fryingAudioSource == null)
            return;

        if (fryingSoundFadeRoutine == null)
            SyncSfxVolume();
    }

    private void SyncSfxVolume()
    {
        if (fryingAudioSource == null)
            return;

        if (SoundManager.Instance != null)
            fryingAudioSource.volume =
                SoundManager.Instance.MasterVolume * SoundManager.Instance.SfxVolume;
        else
            fryingAudioSource.volume = 1f;
    }

    private bool IsFirstKitchenTutorialActive()
    {
        return TutorialController.Instance != null
            && TutorialController.Instance.IsTutorialActive
            && TutorialController.Instance.CurrentStep == TutorialController.TutorialStep.Kitchen_FirstCookProgress;
    }

    private bool IsKitchenActionAllowed(TutorialController.KitchenPracticeTarget action)
    {
        if (!IsFirstKitchenTutorialActive())
            return true;

        if (TutorialController.Instance == null)
            return true;

        return TutorialController.Instance.IsKitchenActionAllowed(action);
    }

    private void ConsumeKitchenAction(TutorialController.KitchenPracticeTarget action)
    {
        if (!IsFirstKitchenTutorialActive())
            return;

        TutorialController.Instance?.TryConsumeKitchenAction(action);

    }
}