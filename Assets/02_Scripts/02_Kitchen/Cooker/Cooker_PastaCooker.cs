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

    [Header("면 프리팹 매핑")]
    [SerializeField] private List<NoodlePrefabData> noodlePrefabs;

    [Header("스폰 위치")]
    [SerializeField] private Transform cookedNoodleSpawnPoint;

    [Header("쿠커 연출 대상")]
    [SerializeField] private Transform cookerVisual;

    [Header("쿠커 선택 연출")]
    [SerializeField] private Vector3 normalScale = Vector3.one;
    [SerializeField] private Vector3 selectedScale = new Vector3(1.17f, 1.17f, 1f);
    [SerializeField] private Vector3 selectedOffset = new Vector3(0f, 0.12f, 0f);
    [SerializeField] private float animDuration = 0.2f;

    [Header("삶는 사운드")]
    [SerializeField] private AudioSource boilingAudioSource;
    [SerializeField] private AudioClip boilingLoopClip;
    [SerializeField] private float fadeOutDuration = 0.7f;

    private SpriteRenderer sr;
    private bool isCooking = false;

    private Vector3 originalLocalPos;
    private Coroutine visualRoutine;
    private Coroutine soundFadeRoutine;
    private Coroutine cookingRoutine;

    public bool CanBeSelected => false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (cookerVisual == null)
            cookerVisual = transform;

        originalLocalPos = cookerVisual.localPosition;
        cookerVisual.localScale = normalScale;

        if (boilingAudioSource == null)
            boilingAudioSource = GetComponent<AudioSource>();

        if (boilingAudioSource == null)
            boilingAudioSource = gameObject.AddComponent<AudioSource>();

        boilingAudioSource.playOnAwake = false;
        boilingAudioSource.loop = true;
        boilingAudioSource.clip = boilingLoopClip;
    }

    void Start()
    {
        SyncSfxVolume();

        if (SoundManager.Instance != null)
            SoundManager.Instance.OnSfxVolumeChanged += OnSfxVolumeChanged;
    }

    void OnDestroy()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.OnSfxVolumeChanged -= OnSfxVolumeChanged;
    }

    private void OnDisable()
    {
        StopAllRunningProcesses();
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
            StartBoiling(noodles);
            return true;
        }

        if (target == null)
        {
            Debug.Log("면을 선택해주세요");
            return false;
        }

        return false;
    }

    private GameObject GetNoodlePrefab(int id)
    {
        foreach (var data in noodlePrefabs)
        {
            if (data.id == id)
                return data.prefab;
        }

        return null;
    }

    public void StartBoiling(Noodles noodles)
    {
        if (noodles == null || isCooking)
            return;

        OnBoiling();

        if (cookingRoutine != null)
            StopCoroutine(cookingRoutine);

        cookingRoutine = StartCoroutine(BoilingRoutine(noodles));
    }

    private IEnumerator BoilingRoutine(Noodles noodles)
    {
        for (int i = 1; i <= 7; i++)
        {
            yield return new WaitForSeconds(1f);
            Debug.Log($"{i}초...");
        }

        if (noodles != null)
        {
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

                    cooked.transform.position = cookedNoodleSpawnPoint.position;
                    cooked.transform.rotation = Quaternion.identity;
                    cooked.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

                    Noodles_CookedNoodle cookedNoodle = cooked.GetComponent<Noodles_CookedNoodle>();
                    if (cookedNoodle != null)
                    {
                        cookedNoodle.SetPastaCooker(this);
                    }
                }
                else
                {
                    Debug.LogWarning($"삶은 면 프리팹이 없습니다. id={id.GetID()}");
                }
            }
        }

        cookingRoutine = null;
        StopBoiling();
    }

    public void OnBoiling()
    {
        isCooking = true;
        Debug.Log("면이 삶아지고 있습니다. 보글보글 oOoOO ....");
        sr.color = Color.cyan;

        PlayBoilingSound();
    }

    public void StopBoiling()
    {
        isCooking = false;
        sr.color = Color.white;
        Debug.Log("면이 다 익었습니다 !");

        StopBoilingSoundWithFade();
    }

    private void PlayBoilingSound()
    {
        if (boilingAudioSource == null || boilingLoopClip == null)
            return;

        if (soundFadeRoutine != null)
        {
            StopCoroutine(soundFadeRoutine);
            soundFadeRoutine = null;
        }

        boilingAudioSource.clip = boilingLoopClip;
        boilingAudioSource.loop = true;

        SyncSfxVolume();

        if (!boilingAudioSource.isPlaying)
            boilingAudioSource.Play();
    }

    private void StopBoilingSoundWithFade()
    {
        if (boilingAudioSource == null)
            return;

        if (!boilingAudioSource.isPlaying)
            return;

        if (soundFadeRoutine != null)
            StopCoroutine(soundFadeRoutine);

        soundFadeRoutine = StartCoroutine(FadeOutBoilingSound());
    }

    private IEnumerator FadeOutBoilingSound()
    {
        float startVolume = boilingAudioSource.volume;
        float time = 0f;

        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeOutDuration);
            boilingAudioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        boilingAudioSource.Stop();
        SyncSfxVolume();
        soundFadeRoutine = null;
    }

    private void OnSfxVolumeChanged(float value)
    {
        if (boilingAudioSource == null)
            return;

        // 루프음 재생 중이 아닐 때도 다음 재생 대비해서 항상 최신값 반영
        if (soundFadeRoutine == null)
            boilingAudioSource.volume = value;
    }

    private void SyncSfxVolume()
    {
        if (boilingAudioSource == null)
            return;

        if (SoundManager.Instance != null)
            boilingAudioSource.volume = SoundManager.Instance.SfxVolume;
        else
            boilingAudioSource.volume = 1f;
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
            t = t * t * (3f - 2f * t);

            cookerVisual.localScale = Vector3.Lerp(startScale, targetScale, t);
            cookerVisual.localPosition = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        cookerVisual.localScale = targetScale;
        cookerVisual.localPosition = targetPos;
        visualRoutine = null;
    }

    private void StopAllRunningProcesses()
    {
        if (cookingRoutine != null)
        {
            StopCoroutine(cookingRoutine);
            cookingRoutine = null;
        }

        if (visualRoutine != null)
        {
            StopCoroutine(visualRoutine);
            visualRoutine = null;
        }

        if (soundFadeRoutine != null)
        {
            StopCoroutine(soundFadeRoutine);
            soundFadeRoutine = null;
        }

        if (boilingAudioSource != null)
        {
            boilingAudioSource.Stop();
            SyncSfxVolume();
        }

        isCooking = false;

        if (sr != null)
            sr.color = Color.white;

        if (cookerVisual != null)
        {
            cookerVisual.localScale = normalScale;
            cookerVisual.localPosition = originalLocalPos;
        }
    }

    public void Cancel()
    {
    }
}