using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : MonoBehaviour
{
    [Header("UI References")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    public GameObject panelRoot; // 팝업 전체 패널 (켜고 끄기용)

    void Start()
    {
        // 팝업이 처음 켜질 때 현재 볼륨을 슬라이더에 반영
        float currentBGM = PlayerPrefs.GetFloat("BGM_Volume", 1f);
        float currentSFX = PlayerPrefs.GetFloat("SFX_Volume", 1f);

        if (bgmSlider != null)
        {
            bgmSlider.value = currentBGM;
            bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = currentSFX;
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        }

        // 시작 시엔 꺼두기
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    public void OnBGMChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetBGMVolume(value);
    }

    public void OnSFXChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
    }

    // --- 열기 / 닫기 ---
    public void OpenSettings()
    {
        if (panelRoot != null) panelRoot.SetActive(true);
    }

    public void CloseSettings()
    {
        if (panelRoot != null) panelRoot.SetActive(false);

        // 닫을 때 '딸깍' 소리 재생
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();
    }
}