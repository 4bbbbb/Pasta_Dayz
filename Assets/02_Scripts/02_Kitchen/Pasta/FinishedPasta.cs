using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IInteractableScript;
using DG.Tweening;

public class FinishedPasta : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public class PanSpriteEntry
    {
        public int noodleID;
        public int sauceID;
        public Sprite sprite;
    }

    [System.Serializable]
    public class BasicPlateSpriteEntry
    {
        public int noodleID;
        public int sauceID;
        public int plateID;
        public bool hasPane;
        public Sprite sprite;
    }

    [System.Serializable]
    public class OvenPlateSpriteEntry
    {
        public int noodleID;
        public int sauceID;
        public int plateID;
        public Sprite sprite;
    }

    [System.Serializable]
    public class OvenPlateCheeseSpriteEntry
    {
        public int sauceID;
        public int plateID;
        public int cheeseID;
        public Sprite sprite;
    }

    [Header("<<후라이팬>>")]
    [SerializeField] private Cooker_FryingPan fryingPan;

    [Header("<<가스 스토브>>")]
    [SerializeField] private Cooker_GasStove gasStove;

    [Header("<<치즈 프리팹>>")]
    [SerializeField] private GameObject parmesanCheesePrefab;

    [Header("<<파슬리 프리팹>>")]
    [SerializeField] private GameObject parsleyPrefab;

    [Header("<<치즈, 파슬리 스폰 위치>>")]
    [SerializeField] private Transform cheeseSpawnPoint;
    [SerializeField] private Transform parsleySpawnPoint;

    [Header("<<접시 위 토핑 그룹>>")]
    [SerializeField] private Transform[] plateToppingGroupParents;

    [SerializeField] private List<PanSpriteEntry> panSpriteEntries = new List<PanSpriteEntry>();
    [SerializeField] private List<BasicPlateSpriteEntry> basicplateSpriteEntries = new List<BasicPlateSpriteEntry>();
    [SerializeField] private List<OvenPlateSpriteEntry> ovenPlateSpriteEntries = new List<OvenPlateSpriteEntry>();
    [SerializeField] private List<OvenPlateCheeseSpriteEntry> cheeseSpriteEntries = new List<OvenPlateCheeseSpriteEntry>();

    [Header("<<이펙트 속도>>")]
    [SerializeField] private float fadeDuration = 0.15f;

    [Header("<<선택 연출>>")]
    [SerializeField] private float selectScaleDuration = 0.12f;
    [SerializeField] private float selectedScaleMultiplier = 1.08f;

    [Header("<<쓰레기 이펙트>>")]
    [SerializeField] private float trashEffectDuration = 0.22f;
    [SerializeField] private float trashFinalScaleMultiplier = 0.2f;
    [SerializeField] private Vector3 trashFadeOffset = new Vector3(0f, -0.15f, 0f);

    [Header("<<드래그 이동>>")]
    private Collider myCollider;
    [SerializeField] private float dragLiftScaleMultiplier = 1.08f;
    [SerializeField] private Transform dragToppingRoot;

    private bool isDragging = false;
    private Vector3 dragStartWorldPos;
    private Vector3 dragStartLocalPos;
    private Transform dragStartParent;
    private Vector3 dragOffset;
    private float dragScreenZ;
    private int originalSortingOrder;
    private bool hasTransferredPanToppingsForDrag = false;

    [Header("<<드래그 판정>>")]
    [SerializeField] private float dragStartThreshold = 0.12f;

    private bool isPointerDown = false;
    private bool hasStartedRealDrag = false;
    private Vector3 mouseDownWorldPos;

    private Plates_BasicPlate dragStartBasicPlate;
    private Plates_OvenPlate dragStartOvenPlate;

    private SpriteRenderer sr;
    private Coroutine spriteFadeRoutine;
    private Vector3 originalScale;

    public bool isSelected { get; private set; }
    public bool CanBeSelected => true;

    private bool hasInitializedSprite = false;
    public bool isBeingTrashed { get; private set; } = false;
    public bool isOnPlate { get; private set; }

    private HashSet<int> ingredientIDs = new HashSet<int>();
    private Cheese.CheeseType? addedCheeseType = null;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        myCollider = GetComponent<Collider>();
    }

    public bool Interact(IInteractable target)
    {
        if (target == null)
        {
            return false;
        }

        if (target is Cheese cheese)
        {
            if (!isOnPlate)
            {
                Debug.Log("그릇 위에 올려진 파스타에만 치즈를 추가할 수 있어요!");
                return false;
            }

            if (addedCheeseType != null)
            {
                Debug.Log("이미 치즈가 추가되어 있어요!");
                return false;
            }

            IngredientIDs id = cheese.GetComponent<IngredientIDs>();
            if (id != null)
            {
                ingredientIDs.Add(id.GetID());
            }

            addedCheeseType = cheese.cheeseType;

            if (cheese.cheeseType == Cheese.CheeseType.Parmesan)
            {
                cheese.Sprinkle(cheeseSpawnPoint, () =>
                {
                    Instantiate(
                        parmesanCheesePrefab,
                        cheeseSpawnPoint.position,
                        Quaternion.identity,
                        cheeseSpawnPoint
                    );
                });
            }
            else if (cheese.cheeseType == Cheese.CheeseType.Mozzarella)
            {
                ClearExistingPlateToppings();
                UpdatePlateSprite();
                Destroy(cheese.gameObject);
            }

            return true;
        }

        if (isOnPlate && target is Plates_BasicPlate)
        {
            return false;
        }

        if (isOnPlate && target is Plates_OvenPlate)
        {
            return false;
        }

        if (target is Topping_Parsley parsley)
        {
            if (!isOnPlate)
            {
                Debug.Log("그릇 위에 올려진 파스타에만 파슬리를 추가할 수 있어요!");
                return false;
            }

            if (!ingredientIDs.Contains(401) && !ingredientIDs.Contains(402))
            {
                Debug.Log("치즈를 먼저 뿌려주세요!");
                return false;
            }

            Debug.Log("파슬리를 뿌렸어요");

            parsley.Sprinkle(parsleySpawnPoint, () =>
            {
                Instantiate(
                    parsleyPrefab,
                    parsleySpawnPoint.position,
                    Quaternion.identity,
                    parsleySpawnPoint
                );
            });

            IngredientIDs id = parsley.GetComponent<IngredientIDs>();
            if (id != null)
            {
                ingredientIDs.Add(id.GetID());
            }

            return true;
        }

        return false;
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
        if (isBeingTrashed)
            return;

        if (Camera.main == null)
            return;

        dragStartBasicPlate = GetComponentInParent<Plates_BasicPlate>();
        dragStartOvenPlate = GetComponentInParent<Plates_OvenPlate>();

        isPointerDown = true;
        hasStartedRealDrag = false;

        dragStartWorldPos = transform.position;
        dragStartLocalPos = transform.localPosition;
        dragStartParent = transform.parent;
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
                BeginRealDrag();
            }
        }

        if (!hasStartedRealDrag)
            return;

        transform.position = currentMouseWorld + dragOffset;
    }

    private void BeginRealDrag()
    {
        if (!isOnPlate && fryingPan != null && !hasTransferredPanToppingsForDrag)
        {
            fryingPan.TransferPanToppingsToFinishedPasta(this);
        }

        hasStartedRealDrag = true;
        isDragging = true;
        isSelected = false;

        transform.DOKill();
        transform.localScale = originalScale * dragLiftScaleMultiplier;

        if (sr != null)
        {
            originalSortingOrder = sr.sortingOrder;
            sr.sortingOrder = 999;
        }

        if (myCollider != null)
            myCollider.enabled = false;

        transform.SetParent(null, true);
    }    

    private void OnMouseUp()
    {
        if (!isPointerDown)
            return;

        isPointerDown = false;

        // 드래그가 실제로 시작되지 않았으면 클릭으로 처리되게 그냥 종료
        if (!hasStartedRealDrag)
        {
            isDragging = false;
            return;
        }

        isDragging = false;
        hasStartedRealDrag = false;

        bool placed = TryDropTarget();

        if (!placed)
        {
            transform.SetParent(dragStartParent, true);
            transform.position = dragStartWorldPos;
            transform.localPosition = dragStartLocalPos;
            transform.localScale = originalScale;
        }

        if (myCollider != null)
            myCollider.enabled = true;

        if (sr != null)
            sr.sortingOrder = originalSortingOrder;
    }

    private bool TryDropTarget()
    {
        if (Camera.main == null)
            return false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Debug.Log("드롭 실패: 아무 콜라이더도 맞지 않음");
            return false;
        }

        Debug.Log("드롭 시 맞은 오브젝트: " + hit.collider.name);

        Plates_BasicPlate basicPlate = hit.collider.GetComponentInParent<Plates_BasicPlate>();
        if (basicPlate != null)
        {
            Debug.Log("BasicPlate 감지됨");
            return basicPlate.Interact(this);
        }

        Plates_OvenPlate ovenPlate = hit.collider.GetComponentInParent<Plates_OvenPlate>();
        if (ovenPlate != null)
        {
            Debug.Log("OvenPlate 감지됨");
            return ovenPlate.Interact(this);
        }

        Cooker_Trashcan trashcan = hit.collider.GetComponentInParent<Cooker_Trashcan>();
        if (trashcan != null)
        {
            Debug.Log("Trashcan 감지됨");
            return trashcan.Interact(this);
        }

        Debug.Log("드롭 실패: 접시/쓰레기통이 아님");
        return false;
    }   

    public void Init(Cooker_GasStove stove)
    {
        gasStove = stove;
    }

    public void SetFryingPan(Cooker_FryingPan pan)
    {
        fryingPan = pan;
    }

    public bool CanMoveToPlate(int plateID, bool targetHasPane)
    {
        int noodleID = GetNoodleID();
        int sauceID = GetSauceID();

        if (noodleID == -1 || sauceID == -1)
            return false;

        if (plateID == 501)
        {
            foreach (var entry in basicplateSpriteEntries)
            {
                if (entry.noodleID == noodleID &&
                    entry.sauceID == sauceID &&
                    entry.plateID == plateID &&
                    entry.hasPane == targetHasPane)
                {
                    return true;
                }
            }
        }
        else if (plateID == 502)
        {
            foreach (var entry in ovenPlateSpriteEntries)
            {
                if (entry.noodleID == noodleID &&
                    entry.sauceID == sauceID &&
                    entry.plateID == plateID)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void MovePanToppingsForDrag(Transform[] sourceGroupParents)
    {
        if (sourceGroupParents == null || dragToppingRoot == null)
            return;

        hasTransferredPanToppingsForDrag = true;

        for (int g = 0; g < sourceGroupParents.Length; g++)
        {
            Transform sourceGroup = sourceGroupParents[g];
            if (sourceGroup == null)
                continue;

            foreach (Transform sourcePoint in sourceGroup)
            {
                if (sourcePoint == null)
                    continue;

                List<Transform> children = new List<Transform>();

                foreach (Transform child in sourcePoint)
                {
                    if (child != null)
                        children.Add(child);
                }

                foreach (Transform child in children)
                {
                    // 위치/회전/스케일 유지한 채 부모만 변경
                    child.SetParent(dragToppingRoot, true);
                }
            }
        }
    }

    public void BuildPlateToppingsFromPan(Transform[] sourceGroupParents)
    {
        Debug.Log($"sourceGroupParents null? {sourceGroupParents == null}");
        Debug.Log($"plateToppingGroupParents null? {plateToppingGroupParents == null}");

        if (sourceGroupParents == null || plateToppingGroupParents == null)
            return;

        ClearExistingPlateToppings();

        int groupCount = Mathf.Min(sourceGroupParents.Length, plateToppingGroupParents.Length);

        for (int g = 0; g < groupCount; g++)
        {
            Transform sourceGroup = sourceGroupParents[g];
            Transform targetGroup = plateToppingGroupParents[g];

            if (sourceGroup == null || targetGroup == null)
            {
                Debug.Log($"group {g}: sourceGroup 또는 targetGroup null");
                continue;
            }

            int pointCount = Mathf.Min(sourceGroup.childCount, targetGroup.childCount);

            for (int p = 0; p < pointCount; p++)
            {
                Transform sourcePoint = sourceGroup.GetChild(p);
                Transform targetPoint = targetGroup.GetChild(p);

                if (sourcePoint == null || targetPoint == null)
                {
                    Debug.Log($"group {g}, point {p}: sourcePoint 또는 targetPoint null");
                    continue;
                }

                Debug.Log($"group {g}, point {p}, sourcePoint childCount = {sourcePoint.childCount}");

                foreach (Transform sourceChild in sourcePoint)
                {
                    if (sourceChild == null)
                    {
                        Debug.Log($"group {g}, point {p}: sourceChild null");
                        continue;
                    }

                    IngredientIDs idComp = sourceChild.GetComponentInChildren<IngredientIDs>(true);
                    if (idComp == null)
                    {
                        Debug.Log($"group {g}, point {p}: IngredientIDs 없음 -> {sourceChild.name}");
                        continue;
                    }

                    int ingredientID = idComp.GetID();
                    GameObject prefab = Order_Manager.Instance.ingredientDB.GetPrefab(ingredientID);

                    Debug.Log($"group {g}, point {p}: ingredientID = {ingredientID}, prefab null? {prefab == null}");

                    if (prefab == null)
                        continue;

                    GameObject obj = Instantiate(prefab, targetPoint.position, Quaternion.identity, targetPoint);
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.rotation = sourceChild.rotation;
                    obj.transform.localScale = Vector3.one;
                    obj.SetActive(true);

                    SpriteRenderer sr = obj.GetComponentInChildren<SpriteRenderer>(true);
                    Debug.Log(
                        $"group {g}, point {p}: 생성 성공 -> {obj.name}, " +
                        $"activeSelf={obj.activeSelf}, activeInHierarchy={obj.activeInHierarchy}, " +
                        $"localPos={obj.transform.localPosition}, worldPos={obj.transform.position}, " +
                        $"localScale={obj.transform.localScale}, " +
                        $"spriteRendererNull={sr == null}" +
                        $"{(sr != null ? $", sortingLayer={sr.sortingLayerName}, order={sr.sortingOrder}, colorA={sr.color.a}" : "")}"
                    );
                }
            }
        }
    }

    private void ClearExistingPlateToppings()
    {
        if (plateToppingGroupParents == null)
            return;

        foreach (Transform groupParent in plateToppingGroupParents)
        {
            if (groupParent == null)
                continue;

            foreach (Transform point in groupParent)
            {
                foreach (Transform child in point)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    public void OnMovedToPlate()
    {
        Debug.Log("OnMovedToPlate 호출됨");

        if (sr == null)
        {
            sr = GetComponent<SpriteRenderer>();
        }

        if (spriteFadeRoutine != null)
        {
            StopCoroutine(spriteFadeRoutine);
            spriteFadeRoutine = null;
        }

        Color c = sr.color;
        sr.color = new Color(c.r, c.g, c.b, 1f);

        isOnPlate = true;
        originalScale = transform.localScale;   // 접시에 올라간 현재 크기를 기준값으로 다시 저장
        UpdatePlateSprite();

        fryingPan?.CopyPanToppingsToFinishedPasta(this);
        fryingPan?.ClearPanAfterServing();

        if (gasStove != null)
        {
            gasStove.DestroyFryingPan();
        }

        // 접시에 올라간 뒤에는 원래 팬/스토브 참조 끊기
        fryingPan = null;
        gasStove = null;

        Debug.Log("완성된 파스타를 그릇에 담았어요 !!");
        PrintIngredients();
    }

    public bool IsOnOvenPlate()
    {
        return ingredientIDs.Contains(502);
    }

    public bool HasMozzarella()
    {
        return ingredientIDs.Contains(402);
    }

    private int GetCheeseID()
    {
        if (ingredientIDs.Contains(401)) return 401;
        if (ingredientIDs.Contains(402)) return 402;
        return -1;
    }

    public void SetIngredients(HashSet<int> ids)
    {
        ingredientIDs = new HashSet<int>(ids);
    }

    public void AddIngredient(int id)
    {
        ingredientIDs.Add(id);
    }

    public HashSet<int> GetIngredientSet()
    {
        return new HashSet<int>(ingredientIDs);
    }

    public void RefreshSprite()
    {
        if (isOnPlate)
            UpdatePlateSprite();
        else
            UpdatePanSprite();
    }

    private int GetNoodleID()
    {
        foreach (int id in ingredientIDs)
        {
            if (id >= 100 && id < 200)
                return id;
        }

        return -1;
    }

    private int GetSauceID()
    {
        if (ingredientIDs.Contains(202)) return 202;
        if (ingredientIDs.Contains(203)) return 203;
        if (ingredientIDs.Contains(204)) return 204;
        if (ingredientIDs.Contains(205)) return 205;
        if (ingredientIDs.Contains(201)) return 201;

        return -1;
    }

    private int GetPlateID()
    {
        if (ingredientIDs.Contains(501)) return 501;
        if (ingredientIDs.Contains(502)) return 502;

        return -1;
    }

    private bool HasPane()
    {
        return ingredientIDs.Contains(601);
    }

    public void UpdatePanSprite()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        int noodleID = GetNoodleID();
        int sauceID = GetSauceID();

        if (noodleID == -1 || sauceID == -1)
        {
            Debug.LogWarning($"면 또는 소스 ID를 찾지 못함. noodleID={noodleID}, sauceID={sauceID}");
            return;
        }

        foreach (var entry in panSpriteEntries)
        {
            if (entry.noodleID == noodleID && entry.sauceID == sauceID)
            {
                if (entry.sprite != null)
                {
                    if (!hasInitializedSprite)
                    {
                        StopSpriteFadeAndRestoreAlpha();
                        sr.sprite = entry.sprite;
                        hasInitializedSprite = true;
                        Debug.Log($"초기 팬 스프라이트 바로 적용: noodle={noodleID}, sauce={sauceID}");
                        return;
                    }

                    if (sr.sprite == entry.sprite)
                    {
                        StopSpriteFadeAndRestoreAlpha();
                        return;
                    }

                    StopSpriteFadeAndRestoreAlpha();
                    spriteFadeRoutine = StartCoroutine(FadeChangeSprite(entry.sprite));

                    Debug.Log($"팬 스프라이트 변경 시작: noodle={noodleID}, sauce={sauceID}");
                }
                else
                {
                    Debug.LogWarning($"매칭은 됐지만 sprite가 비어있음: noodle={noodleID}, sauce={sauceID}");
                }
                return;
            }
        }

        Debug.LogWarning($"팬 스프라이트 매칭 실패: noodle={noodleID}, sauce={sauceID}");
    }

    private IEnumerator FadeChangeSprite(Sprite newSprite)
    {
        if (sr == null) yield break;

        Color c = sr.color;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            sr.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        sr.color = new Color(c.r, c.g, c.b, 0f);
        sr.sprite = newSprite;

        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            sr.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        sr.color = new Color(c.r, c.g, c.b, 1f);
        spriteFadeRoutine = null;
    }

    public void PreparePanSpriteHidden()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        if (spriteFadeRoutine != null)
        {
            StopCoroutine(spriteFadeRoutine);
            spriteFadeRoutine = null;
        }

        int noodleID = GetNoodleID();
        int sauceID = GetSauceID();

        if (noodleID == -1 || sauceID == -1)
        {
            Debug.LogWarning($"면 또는 소스 ID를 찾지 못함. noodleID={noodleID}, sauceID={sauceID}");
            return;
        }

        foreach (var entry in panSpriteEntries)
        {
            if (entry.noodleID == noodleID && entry.sauceID == sauceID)
            {
                if (entry.sprite != null)
                {
                    sr.sprite = entry.sprite;

                    Color c = sr.color;
                    sr.color = new Color(c.r, c.g, c.b, 0f);
                }
                else
                {
                    Debug.LogWarning($"매칭은 됐지만 sprite가 비어있음: noodle={noodleID}, sauce={sauceID}");
                }
                return;
            }
        }

        Debug.LogWarning($"팬 스프라이트 매칭 실패: noodle={noodleID}, sauce={sauceID}");
    }

    public IEnumerator FadeInCurrentSprite()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        Color c = sr.color;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            sr.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        sr.color = new Color(c.r, c.g, c.b, 1f);
    }

    public void UpdatePlateSprite()
    {
        int cheeseID = GetCheeseID();

        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        if (spriteFadeRoutine != null)
        {
            StopCoroutine(spriteFadeRoutine);
            spriteFadeRoutine = null;
        }

        Color c = sr.color;
        sr.color = new Color(c.r, c.g, c.b, 1f);

        int noodleID = GetNoodleID();
        int sauceID = GetSauceID();
        int plateID = GetPlateID();
        bool hasPane = HasPane();

        if (noodleID == -1 || sauceID == -1 || plateID == -1)
        {
            Debug.LogWarning($"ID를 찾지 못함. noodleID={noodleID}, sauceID={sauceID}, plateID={plateID}");
            return;
        }

        if (plateID == 501)
        {
            foreach (var entry in basicplateSpriteEntries)
            {
                if (entry.noodleID == noodleID &&
                    entry.sauceID == sauceID &&
                    entry.plateID == plateID &&
                    entry.hasPane == hasPane)
                {
                    if (entry.sprite != null)
                    {
                        sr.sprite = entry.sprite;
                        hasInitializedSprite = true;
                        Debug.Log($"basic plate 스프라이트 변경 완료: noodle={noodleID}, sauce={sauceID}, plate={plateID}, hasPane={hasPane}");
                    }
                    else
                    {
                        Debug.LogWarning($"basic plate 스프라이트가 비어있음: noodle={noodleID}, sauce={sauceID}, plate={plateID}, hasPane={hasPane}");
                    }
                    return;
                }
            }
        }
        else if (plateID == 502)
        {
            if (cheeseID == 402)
            {
                foreach (var entry in cheeseSpriteEntries)
                {
                    if (entry.sauceID == sauceID &&
                        entry.plateID == plateID &&
                        entry.cheeseID == cheeseID)
                    {
                        if (entry.sprite != null)
                        {
                            sr.sprite = entry.sprite;
                            hasInitializedSprite = true;
                            Debug.Log("모짜렐라 스프라이트 적용");
                            return;
                        }
                    }
                }
            }

            foreach (var entry in ovenPlateSpriteEntries)
            {
                if (entry.noodleID == noodleID &&
                    entry.sauceID == sauceID &&
                    entry.plateID == plateID)
                {
                    if (entry.sprite != null)
                    {
                        sr.sprite = entry.sprite;
                        hasInitializedSprite = true;
                        Debug.Log($"oven plate 스프라이트 변경 완료: noodle={noodleID}, sauce={sauceID}, plate={plateID}");
                    }
                    return;
                }
            }
        }
    }

    private void StopSpriteFadeAndRestoreAlpha()
    {
        if (spriteFadeRoutine != null)
        {
            StopCoroutine(spriteFadeRoutine);
            spriteFadeRoutine = null;
        }

        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        Color c = sr.color;
        sr.color = new Color(c.r, c.g, c.b, 1f);
    }

    public void PrintIngredients()
    {
        Debug.Log("현재 ingredientIDs 개수: " + ingredientIDs.Count);

        foreach (int id in ingredientIDs)
        {
            Debug.Log("Plate에 포함된 ID: " + id);
        }
    }

    public void OnTrashed()
    {
        if (isBeingTrashed)
            return;

        isBeingTrashed = true;
        isSelected = false;
        isDragging = false;

        transform.DOKill();

        if (myCollider != null)
            myCollider.enabled = false;

        if (spriteFadeRoutine != null)
        {
            StopCoroutine(spriteFadeRoutine);
            spriteFadeRoutine = null;
        }

        ClearExistingPlateToppings();

        if (dragStartBasicPlate != null)
        {
            Destroy(dragStartBasicPlate.gameObject);
            dragStartBasicPlate = null;
        }

        if (dragStartOvenPlate != null)
        {
            Destroy(dragStartOvenPlate.gameObject);
            dragStartOvenPlate = null;
        }

        // 접시 위 파스타를 버릴 때는 현재 돌아가는 팬을 건드리면 안 됨
        if (!isOnPlate)
        {
            fryingPan?.ClearPanAfterServing();

            if (gasStove != null)
                gasStove.DestroyFryingPan();
        }

        fryingPan = null;
        gasStove = null;
    }

    public void PlayTrashEffect(Transform trashTarget)
    {
        transform.DOKill();

        float effectDuration = 0.22f;
        float finalScaleMultiplier = 0.2f;
        Vector3 trashFadeOffset = new Vector3(0f, -0.15f, 0f);

        if (trashTarget != null)
            transform.position = trashTarget.position + trashFadeOffset;

        Sequence seq = DOTween.Sequence();

        seq.Append(
            transform.DOScale(originalScale * finalScaleMultiplier, effectDuration)
                     .SetEase(Ease.InQuad)
        );

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var r in renderers)
        {
            if (r != null)
                seq.Join(r.DOFade(0f, effectDuration));
        }

        seq.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    private void ResetIngredientState()
    {
        ingredientIDs.Clear();

        addedCheeseType = null;
        isSelected = false;
        isOnPlate = false;
        hasInitializedSprite = false;
        transform.localScale = originalScale;

        if (spriteFadeRoutine != null)
        {
            StopCoroutine(spriteFadeRoutine);
            spriteFadeRoutine = null;
        }

        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            Color c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, 1f);
        }
    }

    public void Cancel()
    {
        if (isBeingTrashed)
            return;

        isSelected = false;

        transform.DOKill();
        transform.DOScale(originalScale, selectScaleDuration)
                 .SetEase(Ease.OutQuad);
    }


}