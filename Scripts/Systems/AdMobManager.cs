using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdMobManager : MonoBehaviour
{
    public static AdMobManager Instance;

    // ★ 테스트용 전면 광고 ID (나중에 출시할 때 진짜 ID로 바꾸세요!)
    private string adUnitId = "ca-app-pub-6842130925784520/3150293279";

    private InterstitialAd _interstitialAd;

    // ★ [추가] 게임 횟수 카운터
    private int gameOverCount = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        MobileAds.Initialize(initStatus => {
            LoadInterstitialAd();
        });
    }

    public void LoadInterstitialAd()
    {
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        var adRequest = new AdRequest();

        InterstitialAd.Load(adUnitId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("광고 로드 실패: " + error);
                    return;
                }

                _interstitialAd = ad;
                RegisterEventHandlers(_interstitialAd);
            });
    }

    // ★ [수정됨] 2판마다 광고 보여주는 로직
    public void ShowAd()
    {
        // 1. 호출될 때마다 카운트를 1 올림
        gameOverCount++;

        // 2. 짝수 번째(2, 4, 6...) 판인지 확인
        // (% 2 == 0 은 '2로 나눴을 때 나머지가 0'이라는 뜻)
        if (gameOverCount % 2 == 0)
        {
            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                Debug.Log($"[AdMob] {gameOverCount}번째 게임 종료: 광고 표시 O");
                _interstitialAd.Show();
            }
            else
            {
                Debug.Log("광고가 아직 준비 안 됨. 다시 로드 시도.");
                LoadInterstitialAd();
            }
        }
        else
        {
            // 홀수 번째(1, 3, 5...) 판은 그냥 넘어감
            Debug.Log($"[AdMob] {gameOverCount}번째 게임 종료: 광고 표시 X (건너뜀)");
        }
    }

    private void RegisterEventHandlers(InterstitialAd ad)
    {
        // 광고 닫으면 바로 다음 광고 로드
        ad.OnAdFullScreenContentClosed += () =>
        {
            LoadInterstitialAd();
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            LoadInterstitialAd();
        };
    }
}