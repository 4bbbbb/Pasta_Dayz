using UnityEngine;
using UnityEngine.UI;

public class SoundSettingUI : MonoBehaviour
{
    [Header("슬라이더")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        if (SoundManager.Instance == null)
            return;

        // 현재 저장된 값 반영
        if (masterSlider != null)
        {
            masterSlider.SetValueWithoutNotify(SoundManager.Instance.MasterVolume);
            masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
        }

        if (bgmSlider != null)
        {
            bgmSlider.SetValueWithoutNotify(SoundManager.Instance.BgmVolume);
            bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(SoundManager.Instance.SfxVolume);
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        }
    }

    private void OnDestroy()
    {
        if (masterSlider != null)
            masterSlider.onValueChanged.RemoveListener(OnMasterSliderChanged);

        if (bgmSlider != null)
            bgmSlider.onValueChanged.RemoveListener(OnBgmSliderChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);
    }

    private void OnMasterSliderChanged(float value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetMasterVolume(value);
    }

    private void OnBgmSliderChanged(float value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetBgmVolume(value);
    }

    private void OnSfxSliderChanged(float value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetSfxVolume(value);
    }
}