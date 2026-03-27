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

    [Header("버블 이펙트")]
    [SerializeField] private GameObject bubbleEffect;

    [Header("삶는 사운드")]
    [SerializeField] private AudioSource boilingAudioSource;
    [SerializeField] private AudioClip boilingLoopClip;
    [SerializeField] private float fadeOutDuration = 0.7f;

    private SpriteRenderer[] bubbleRenderers;

    private bool isCooking = false;

    private Vector3 originalLocalPos;
    private Coroutine visualRoutine;
    private Coroutine soundFadeRoutine;
    private Coroutine cookingRoutine;
    private Coroutine bubbleFadeRoutine;

    public bool CanBeSelected => false;

    void Awake()
    {

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

        if (bubbleEffect != null)
        {
            bubbleRenderers = bubbleEffect.GetComponentsInChildren<SpriteRenderer>();
            SetBubbleAlpha(0f);
        }

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

    private void OnDisable()
    {
        StopAllRunningProcesses();
    }

    public bool Interact(IInteractable target)
    {
        if (isCooking)
            return false;

        if (target is Noodles noodles)
        {
            StartBoiling(noodles);
            return true;
        }

        return false;
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
        GameObject cooked = null;

        if (noodles != null)
        {
            IngredientIDs id = noodles.GetComponent<IngredientIDs>();
            if (id != null)
            {
                GameObject prefab = GetNoodlePrefab(id.GetID());

                if (prefab != null)
                {
                    cooked = Instantiate(
                        prefab,
                        cookedNoodleSpawnPoint.position,
                        Quaternion.identity,
                        cookedNoodleSpawnPoint
                    );

                    cooked.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

                    Collider2D col = cooked.GetComponent<Collider2D>();
                    if (col != null)
                        col.enabled = false;

                    var cookedNoodle = cooked.GetComponent<Noodles_CookedNoodle>();
                    if (cookedNoodle != null)
                        cookedNoodle.SetPastaCooker(this);
                }
            }
        }

        yield return new WaitForSeconds(7f);

        StopBoiling();

        if (cooked != null)
        {
            Collider2D col = cooked.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = true;

            var noodle = cooked.GetComponent<Noodles_CookedNoodle>();
            if (noodle != null)
                noodle.Unlock();
        }
    }


    private GameObject GetNoodlePrefab(int id)
    {
        foreach (var data in noodlePrefabs)
            if (data.id == id)
                return data.prefab;

        return null;
    }

    public void OnBoiling()
    {
        isCooking = true;

        SetBubbleAlpha(0f);

        foreach (var b in bubbleEffect.GetComponentsInChildren<Bubble>())
        {
            b.StartBubble();
        }

        StartCoroutine(StartBubbleRoutine());

        PlayBoilingSound();
    }


    IEnumerator StartBubbleRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        FadeBubble(0f, 1f, 0.5f);
    }


    public void StopBoiling()
    {
        isCooking = false;

        FadeBubble(1f, 0f, 1f);

        StartCoroutine(StopBubbleAfterFade());

        StopBoilingSoundWithFade();
    }

    IEnumerator StopBubbleAfterFade()
    {
        yield return new WaitForSeconds(0.8f); // ← Fade 시간과 맞추기

        foreach (var b in bubbleEffect.GetComponentsInChildren<Bubble>())
        {
            b.StopBubble();
        }
    }


    void FadeBubble(float start, float end, float duration)
    {
        if (bubbleFadeRoutine != null)
            StopCoroutine(bubbleFadeRoutine);

        bubbleFadeRoutine = StartCoroutine(FadeBubbleRoutine(start, end, duration));
    }

    IEnumerator FadeBubbleRoutine(float start, float end, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            float a = Mathf.Lerp(start, end, t);

            SetBubbleAlpha(a);
            yield return null;
        }

        SetBubbleAlpha(end);
    }

    void SetBubbleAlpha(float alpha)
    {
        if (bubbleRenderers == null) return;

        foreach (var r in bubbleRenderers)
        {
            Color c = r.color;
            c.a = alpha;
            r.color = c;
        }
    }
    

    private void PlayBoilingSound()
    {
        if (boilingAudioSource == null || boilingLoopClip == null)
            return;

        boilingAudioSource.loop = true;
        SyncSfxVolume();

        if (!boilingAudioSource.isPlaying)
            boilingAudioSource.Play();
    }

    private void StopBoilingSoundWithFade()
    {
        if (boilingAudioSource == null || !boilingAudioSource.isPlaying)
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
            boilingAudioSource.volume = Mathf.Lerp(startVolume, 0f, time / fadeOutDuration);
            yield return null;
        }

        boilingAudioSource.Stop();
        SyncSfxVolume();
    }

    private void SyncSfxVolume()
    {
        if (boilingAudioSource == null) return;

        if (SoundManager.Instance != null)
            boilingAudioSource.volume =
                SoundManager.Instance.MasterVolume * SoundManager.Instance.SfxVolume;
        else
            boilingAudioSource.volume = 1f;
    }    

    private void StopAllRunningProcesses()
    {
        if (cookingRoutine != null) StopCoroutine(cookingRoutine);
        if (visualRoutine != null) StopCoroutine(visualRoutine);
        if (soundFadeRoutine != null) StopCoroutine(soundFadeRoutine);
        if (bubbleFadeRoutine != null) StopCoroutine(bubbleFadeRoutine);

        if (boilingAudioSource != null)
        {
            boilingAudioSource.Stop();
            SyncSfxVolume();
        }

        SetBubbleAlpha(0f);

        isCooking = false;
    }

    private void OnSfxVolumeChanged(float value)
    {
        SyncSfxVolume();
    }

    private void OnMasterVolumeChanged(float value)
    {
        SyncSfxVolume();
    }


    public void Cancel() { }
}
