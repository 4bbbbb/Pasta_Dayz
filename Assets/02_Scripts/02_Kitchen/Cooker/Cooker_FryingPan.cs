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

    [Header("<<스폰 위치>>")]
    [SerializeField] private Transform[] toppingSpawnPoints;
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
    [SerializeField] public Shader_Spread tomatoEffect;
    [SerializeField] public Shader_Spread creamEffect;
    [SerializeField] public Shader_Spread vongoleEffect;

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

    private bool isSauceAnimating = false;

    private bool hasOil = false;
    private bool isCooking = false;

    private HashSet<ToppingType> addedToppings = new HashSet<ToppingType>();
    private HashSet<SauceType> addedSauces = new HashSet<SauceType>();
    private HashSet<int> ingredientIDs = new HashSet<int>();

    public bool CanBeSelected => false;

    private bool hasFinishedPastaOnPan = false;

    private Coroutine fryingSoundFadeRoutine;

    void Start()
    {
        ResetState();

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

    public bool Interact(IInteractable target)
    {
        if (isCooking || hasFinishedPastaOnPan)
        {
            return false;
        }

        if (target is Topping_OliveOil oil)
        {
            return AddOil(oil);
        }

        if (target is Topping topping)
        {
            return AddTopping(topping);
        }

        if (target is Sauces sauce)
        {
            return AddSauce(sauce);
        }

        if (target is Noodles_CookedNoodle noodle)
        {
            return AddNoodle(noodle);
        }

        return false;
    }

    bool AddOil(Topping_OliveOil oil)
    {
        if (hasOil) return false;

        hasOil = true;
        gasStove.TurnOn();

        // 애니메이션 먼저 실행
        oil.PlayPourToPanAnimation(GetSaucePourWorldPos());

        // 오일 퍼지는 효과는 약간 딜레이
        DOVirtual.DelayedCall(0.5f, () =>
        {
            if (this != null)
                oilSpreadEffect.PlayOilSpread();
        });

        IngredientIDs id = oil.GetComponent<IngredientIDs>();
        if (id != null)
            ingredientIDs.Add(id.GetID());

        return true;
    }


    bool AddTopping(Topping topping)
    {
        if (addedToppings.Count >= 2) return false;
        if (addedToppings.Contains(topping.toppingType)) return false;

        Transform spawnPoint = GetRandomEmptyToppingPoint();
        if (spawnPoint == null) return false;

        IngredientIDs id = topping.GetComponent<IngredientIDs>();

        if (id != null)
            SpawnIngredientByID(id.GetID(), spawnPoint);

        addedToppings.Add(topping.toppingType);
        return true;
    }

    bool AddSauce(Sauces sauce)
    {
        IngredientIDs id = sauce.GetComponent<IngredientIDs>();
        if (id == null) return false;

        int newID = id.GetID();

        if (!CanAcceptSauce(newID))
            return false;

        // 데이터는 먼저 반영
        ApplySauceDataByID(newID, sauce.sauceType);

        // 국자 애니메이션 재생
        sauce.PlayPourToPanAnimation(GetSaucePourWorldPos());

        // 팬 스프라이트는 조금 늦게 표시
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

    void ApplySauceDataByID(int newID, SauceType sauceType)
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

    bool CanAcceptSauce(int newID)
    {
        // 로제 완성 상태면 더 이상 추가 불가
        if (ingredientIDs.Contains(204))
            return false;

        // 토마토 + 크림 / 크림 + 토마토 조합은 허용
        if ((ingredientIDs.Contains(202) && newID == 203) ||
            (ingredientIDs.Contains(203) && newID == 202))
        {
            return true;
        }

        // 일반 소스는 1개만 허용
        if (addedSauces.Count >= 1)
            return false;

        return true;
    }

    void ApplySauceByID(int newID, SauceType sauceType)
    {
        if (ingredientIDs.Contains(202) && newID == 203)
        {
            ingredientIDs.Remove(202);
            ingredientIDs.Add(204);

            addedSauces.Clear();
            addedSauces.Add(SauceType.Rose);

            ShowSauceSprite(204);
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

            ShowSauceSprite(204);
            oilOffSprite.SetActive(false);
            oilOnSprite.SetActive(false);
            return;
        }

        addedSauces.Add(sauceType);
        ingredientIDs.Add(newID);

        ShowSauceSprite(newID);
    }

    Vector3 GetSaucePourWorldPos()
    {
        if (saucePourTarget != null)
            return saucePourTarget.position;

        return transform.position;
    }

    void ShowSauceSprite(int sauceID)
    {
        tomatoSauceSprite.SetActive(false);
        creamSauceSprite.SetActive(false);
        roseSauceSprite.SetActive(false);
        vongoleSauceSprite.SetActive(false);

        switch (sauceID)
        {
            case 202:
                tomatoSauceSprite.SetActive(true);
                if (tomatoEffect != null)
                    tomatoEffect.PlayOilSpread();
                break;

            case 203:
                creamSauceSprite.SetActive(true);
                if (creamEffect != null)
                    creamEffect.PlayOilSpread();
                break;

            case 204:
                roseSauceSprite.SetActive(true);
                break;

            case 205:
                vongoleSauceSprite.SetActive(true);
                if (vongoleEffect != null)
                    vongoleEffect.PlayOilSpread();
                break;
        }
    }

    bool AddNoodle(Noodles_CookedNoodle cookedNoodle)
    {
        if (!hasOil) return false;
        if (noodleSpawnPoint.childCount > 0) return false;

        IngredientIDs id = cookedNoodle.GetComponent<IngredientIDs>();
        if (id == null) return false;

        SpawnIngredientByID(id.GetID(), noodleSpawnPoint);

        Destroy(cookedNoodle.gameObject);

        StartCoroutine(CookRoutine());
        return true;
    }

    void SpawnIngredientByID(int ingredientID, Transform spawnPoint)
    {
        GameObject prefab = Order_Manager.Instance
            .ingredientDB
            .GetPrefab(ingredientID);

        if (prefab == null) return;

        Instantiate(
            prefab,
            spawnPoint.position,
            Quaternion.identity,
            spawnPoint
        );

        ingredientIDs.Add(ingredientID);
    }

    Transform GetRandomEmptyToppingPoint()
    {
        List<Transform> empty = new List<Transform>();

        foreach (var point in toppingSpawnPoints)
        {
            if (point.childCount == 0)
                empty.Add(point);
        }

        if (empty.Count == 0) return null;

        return empty[Random.Range(0, empty.Count)];
    }

    IEnumerator CookRoutine()
    {
        isCooking = true;
        PlayFryingSound();

        Vector3 originalPanPos = transform.localPosition;
        Vector3 panMoveDir = transform.localRotation * Vector3.up;

        float cycleCount = 20.25f;
        float cycleDuration = 0.25f;
        float totalTime = cycleCount * cycleDuration;

        float panMoveAmount = 0.12f;
        float elapsed = 0f;

        Dictionary<Transform, Vector3> ingredientOriginalLocalPos = new Dictionary<Transform, Vector3>();

        foreach (Transform point in toppingSpawnPoints)
        {
            foreach (Transform child in point)
            {
                ingredientOriginalLocalPos[child] = child.localPosition;
            }
        }

        foreach (Transform child in noodleSpawnPoint)
        {
            ingredientOriginalLocalPos[child] = child.localPosition;
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

                float offsetY = Mathf.Sin(angle - 0.6f + phase) * 0.05f * envelope;
                float offsetX = Mathf.Cos(angle * 1.2f + phase) * 0.02f * envelope;

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

        GameObject finishedPasta = Instantiate(finishedPastaPrefab, finishedPastaSpawnPoint);
        finishedPasta.transform.localPosition = Vector3.zero;
        finishedPasta.transform.localRotation = Quaternion.identity;
        finishedPasta.transform.localScale = Vector3.one;

        FinishedPasta pasta = finishedPasta.GetComponent<FinishedPasta>();

        pasta.SetIngredients(new HashSet<int>(ingredientIDs));
        pasta.Init(gasStove);
        pasta.SetFryingPan(this);
        pasta.PreparePanSpriteHidden();

        StartCoroutine(FadeOutAndDestroyNoodle());
        yield return StartCoroutine(pasta.FadeInCurrentSprite());

        ClearPanForFinishedPasta();

        hasFinishedPastaOnPan = true;
        isCooking = false;
        gasStove.TurnOff();
    }

    IEnumerator FadeOutAndDestroyNoodle()
    {
        if (noodleSpawnPoint.childCount == 0)
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

    void ClearPanForFinishedPasta()
    {
        oilOffSprite.SetActive(true);
        oilOnSprite.SetActive(false);

        tomatoSauceSprite.SetActive(false);
        creamSauceSprite.SetActive(false);
        roseSauceSprite.SetActive(false);
        vongoleSauceSprite.SetActive(false);
    }

    public void ClearPanAfterServing()
    {
        foreach (Transform point in toppingSpawnPoints)
        {
            foreach (Transform child in point)
                Destroy(child.gameObject);
        }

        if (finishedPastaSpawnPoint.childCount > 0)
        {
            Transform child = finishedPastaSpawnPoint.GetChild(0);
            if (child != null)
            {
                child.SetParent(null, true);
            }
        }

        ResetState();
        hasFinishedPastaOnPan = false;
    }

    void ResetState()
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

    public void Cancel()
    {
    }
}