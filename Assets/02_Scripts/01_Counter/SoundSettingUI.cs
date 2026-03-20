using UnityEngine;
using UnityEngine.UI;

public class SoundSettingUI : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    void Start()
    {
        if (SoundManager.Instance == null) return;

        bgmSlider.value = SoundManager.Instance.BgmVolume;
        sfxSlider.value = SoundManager.Instance.SfxVolume;

        bgmSlider.onValueChanged.AddListener(OnChangeBgm);
        sfxSlider.onValueChanged.AddListener(OnChangeSfx);
    }

    void OnChangeBgm(float value)
    {
        SoundManager.Instance.SetBgmVolume(value);
    }

    void OnChangeSfx(float value)
    {
        SoundManager.Instance.SetSfxVolume(value);
    }
}