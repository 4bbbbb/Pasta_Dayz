using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("공용 AudioSource")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("볼륨")]
    [Range(0f, 1f)][SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float bgmVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;

    [Header("BGM")]
    [SerializeField] private AudioClip mainBgm;

    public float MasterVolume => masterVolume;
    public float BgmVolume => bgmVolume;
    public float SfxVolume => sfxVolume;

    public System.Action<float> OnMasterVolumeChanged;
    public System.Action<float> OnBgmVolumeChanged;
    public System.Action<float> OnSfxVolumeChanged;

    private const string MASTER_KEY = "MASTER_VOLUME";
    private const string BGM_KEY = "BGM_VOLUME";
    private const string SFX_KEY = "SFX_VOLUME";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadVolume();
            ApplyVolume();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 0번 씬에서는 브금 끄기
        if (scene.buildIndex == 0)
        {
            StopBGM();
            return;
        }

        // 1번, 2번 씬에서는 같은 브금 유지/재생
        if (mainBgm != null)
        {
            PlayBGM(mainBgm);
        }
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        ApplyVolume();

        PlayerPrefs.SetFloat(MASTER_KEY, masterVolume);
        PlayerPrefs.Save();

        OnMasterVolumeChanged?.Invoke(masterVolume);
    }

    public void SetBgmVolume(float value)
    {
        bgmVolume = Mathf.Clamp01(value);
        ApplyVolume();

        PlayerPrefs.SetFloat(BGM_KEY, bgmVolume);
        PlayerPrefs.Save();

        OnBgmVolumeChanged?.Invoke(bgmVolume);
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        ApplyVolume();

        PlayerPrefs.SetFloat(SFX_KEY, sfxVolume);
        PlayerPrefs.Save();

        OnSfxVolumeChanged?.Invoke(sfxVolume);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (clip == null || bgmSource == null) return;

        // 같은 곡이 이미 재생 중이면 재시작 안 함
        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();

        ApplyVolume();
    }

    public void StopBGM()
    {
        if (bgmSource != null)
            bgmSource.Stop();
    }

    private void LoadVolume()
    {
        masterVolume = PlayerPrefs.GetFloat(MASTER_KEY, 1f);
        bgmVolume = PlayerPrefs.GetFloat(BGM_KEY, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_KEY, 1f);
    }

    private void ApplyVolume()
    {
        float finalBgm = masterVolume * bgmVolume;
        float finalSfx = masterVolume * sfxVolume;

        if (bgmSource != null)
            bgmSource.volume = finalBgm;

        if (sfxSource != null)
            sfxSource.volume = finalSfx;
    }
}