using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip mainMenuBGM;
    public AudioClip inGameBGM;

    public AudioClip clickSFX;
    public AudioClip placeBlockSFX;
    public AudioClip gameOverSFX;
    public AudioClip victorySFX;

    // 저장 키값
    private const string KEY_BGM_VOL = "BGM_Volume";
    private const string KEY_SFX_VOL = "SFX_Volume";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitVolume(); // ★ 시작할 때 볼륨 불러오기
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitVolume()
    {
        // 저장된 값이 없으면 1.0(최대)로 설정
        float bgmVol = PlayerPrefs.GetFloat(KEY_BGM_VOL, 1f);
        float sfxVol = PlayerPrefs.GetFloat(KEY_SFX_VOL, 1f);

        // 초기값 적용
        if (bgmSource != null) bgmSource.volume = bgmVol;
        if (sfxSource != null) sfxSource.volume = sfxVol;
    }

    // ★ 설정창 슬라이더에서 호출할 함수들
    public void SetBGMVolume(float volume)
    {
        if (bgmSource != null) bgmSource.volume = volume;
        PlayerPrefs.SetFloat(KEY_BGM_VOL, volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null) sfxSource.volume = volume;
        PlayerPrefs.SetFloat(KEY_SFX_VOL, volume);
    }

    // --- 기존 재생 함수들 ---
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM() => bgmSource.Stop();
    public void PlaySFX(AudioClip clip) { if (clip != null) sfxSource.PlayOneShot(clip); }

    public void PlayMainMenuBGM() => PlayBGM(mainMenuBGM);
    public void PlayInGameBGM() => PlayBGM(inGameBGM);
    public void PlayClick() => PlaySFX(clickSFX);
    public void PlayPlaceBlock() => PlaySFX(placeBlockSFX);
    public void PlayGameOver() => PlaySFX(gameOverSFX);
    public void PlayVictory() => PlaySFX(victorySFX);
}