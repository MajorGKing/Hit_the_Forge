using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Services.LevelPlay;
using UnityEngine;

public class AdManager
{
    private LevelPlayBannerAd bannerAd;
    private LevelPlayRewardedAd[] rewardedVideoAds;


    private bool _isAdsEnabled = false;
    public bool IsAdsEnabled
    {
        protected set { _isAdsEnabled = value; }
        get => _isAdsEnabled;
    }

    private bool[] retrying;
    private bool _isBannerRetrying = false;
    private CancellationTokenSource adsCts;

    public void Init()
    {
        LevelPlay.OnInitSuccess -= SdkInitializationCompletedEvent;
        LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed -= SdkInitializationFailedEvent;
        LevelPlay.OnInitFailed += SdkInitializationFailedEvent;
        // SDK init
        LevelPlay.Init(AppKey);
    }

    public void Clear()
    {
        adsCts?.Cancel();
        adsCts?.Dispose();
        adsCts = null;

        LevelPlay.OnInitSuccess -= SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed -= SdkInitializationFailedEvent;
    }

    // ILHAK temp 나중에 Clear호출하도록 변경
    private void OnDisable() 
    {
        adsCts?.Cancel();
        adsCts?.Dispose();
        adsCts = null;

        LevelPlay.OnInitSuccess -= SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed -= SdkInitializationFailedEvent;
    }

    private void EnableAds()
    {
        adsCts?.Cancel();
        adsCts?.Dispose();
        adsCts = new CancellationTokenSource();
        CancellationToken token = adsCts.Token;

        //// Register to ImpressionDataReadyEvent
        //LevelPlay.OnImpressionDataReady += ImpressionDataReadyEvent;

        // Create Rewarded Video object
        //rewardedVideoAd = new LevelPlayRewardedAd(AdConfig.RewardedVideoAdUnitId);
        int currencyCount = Enum.GetValues(typeof(Define.ECurrency)).Length;
        rewardedVideoAds = new LevelPlayRewardedAd[currencyCount];

        retrying = new bool[currencyCount];

        foreach (Define.ECurrency currency in Enum.GetValues(typeof(Define.ECurrency)))
        {
            if (currency == Define.ECurrency.None)
                continue;

            string adUnitId = RewardedVideoAdUnitId(currency);
            Debug.Log($"adUnitId : {adUnitId}");
            if (string.IsNullOrEmpty(adUnitId))
                continue;

            InitializeRewardedAd(currency, adUnitId, token);

            // var ad = new LevelPlayRewardedAd(adUnitId);

            // // currency를 캡처해서 이벤트 연결
            // Define.ECurrency capturedCurrency = currency;

            // Debug.Log($"capturedCurrency : {capturedCurrency}");

            // ad.OnAdLoaded += (adInfo) =>
            // {
            //     retrying[(int)capturedCurrency] = false; // 재시도 중단
            //     OnRewardedLoaded?.Invoke(capturedCurrency, adInfo);
            // };

            // ad.OnAdLoadFailed += error =>
            // {
            //     Debug.LogError($"{capturedCurrency} rewarded load failed: {error}");
            //     OnRewardedLoadFailed?.Invoke(capturedCurrency, error.ToString());

            //     RetryLoadRewardedAd(ad, capturedCurrency, token).Forget();
            // };

            // ad.OnAdClosed += (adInfo) =>
            // {
            //     Debug.Log($"{capturedCurrency} rewarded ad closed");
            //     OnRewardedClosed?.Invoke(capturedCurrency, adInfo);
            //     ad.LoadAd(); // 광고 닫히면 다시 로드
            // };

            // ad.OnAdDisplayFailed += (adInfo, error) =>
            // {
            //     Debug.LogError($"{capturedCurrency} rewarded ad display failed: {error}");
            //     OnRewardedLoadFailed?.Invoke(capturedCurrency, error.ToString());
            //     ad.LoadAd(); // 표시 실패시에도 다시 로드 시도
            // };

            // ad.OnAdRewarded += (adInfo, reward) =>
            // {
            //     Debug.Log($"{capturedCurrency} rewarded ad rewarded");
            //     OnRewardedEarned?.Invoke(capturedCurrency, adInfo, reward);
            // };

            // Debug.Log($"ads capturedCurrency : {(int)currency}");
            // rewardedVideoAds[(int)currency] = ad;
            // ad.LoadAd();
        }

        // Create Banner object
        //bannerAd = new LevelPlayBannerAd(AdConfig.BannerAdUnitId);
        var configBuilder = new LevelPlayBannerAd.Config.Builder();
        configBuilder.SetSize(LevelPlayAdSize.BANNER);
        configBuilder.SetPosition(LevelPlayBannerPosition.BottomCenter);
        configBuilder.SetDisplayOnLoad(true);
        configBuilder.SetRespectSafeArea(true); // Only relevant for Android
        configBuilder.SetPlacementName("bannerPlacement");
        configBuilder.SetBidFloor(0.01); // Minimum bid price in USD
        var bannerConfig = configBuilder.Build();

        bannerAd = new LevelPlayBannerAd(BannerAdUnitId, bannerConfig);

        bannerAd.OnAdLoaded += (info) =>
        {
            Debug.Log("Banner Ad Loaded Success");
        };

        bannerAd.OnAdLoadFailed += (error) =>
        {
            Debug.LogError($"Banner Load Failed: {error}");
        };

        RetryLoadBannerAd(token).Forget();

        //bannerAd.LoadAd();
    }

    private void InitializeRewardedAd(Define.ECurrency currency, string adUnitId, CancellationToken token)
    {
        var ad = new LevelPlayRewardedAd(adUnitId);

        Debug.Log($"Initializing Ad for: {currency}");
        ad.OnAdLoaded += (adInfo) =>
        {
            Debug.Log($"{currency} Ad Loaded");
            retrying[(int)currency] = false;
            OnRewardedLoaded?.Invoke(currency, adInfo);
        };
        ad.OnAdLoadFailed += error =>
        {
            Debug.LogError($"{currency} rewarded load failed: {error}");
            OnRewardedLoadFailed?.Invoke(currency, error.ToString());

            RetryLoadRewardedAd(ad, currency, token).Forget();
        };
        ad.OnAdClosed += (adInfo) =>
        {
            Debug.Log($"{currency} rewarded ad closed");
            OnRewardedClosed?.Invoke(currency, adInfo);
            ad.LoadAd();
        };
        ad.OnAdDisplayFailed += (adInfo, error) =>
        {
            Debug.LogError($"{currency} rewarded ad display failed: {error}");
            OnRewardedLoadFailed?.Invoke(currency, error.ToString());
            ad.LoadAd();
        };
        ad.OnAdRewarded += (adInfo, reward) =>
        {
            Debug.Log($"{currency} rewarded ad earned: {reward.Amount}");
            OnRewardedEarned?.Invoke(currency, adInfo, reward);
        };
        rewardedVideoAds[(int)currency] = ad;
        ad.LoadAd();
    }

    private async UniTaskVoid RetryLoadBannerAd(CancellationToken token)
    {
        // 1. 활성화될 때까지 짧은 주기로 체크 (응답성 향상)
        while (_isBannerRetrying == false)
        {
            if (token.IsCancellationRequested) return;
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
        }


        // 이미 로직 진입 시 true로 고정 (LoadBanner에서 설정됨)
        try
        {
            while (!token.IsCancellationRequested)
            {
                // 주기적 로드
                await UniTask.Delay(TimeSpan.FromSeconds(30), cancellationToken: token);

                if (bannerAd != null)
                {
                    //Debug.Log("Banner Ad Refreshing (30s interval)...");
                    //bannerAd.LoadAd();
                }
                else
                {
                    var configBuilder = new LevelPlayBannerAd.Config.Builder();
                    configBuilder.SetSize(LevelPlayAdSize.BANNER);
                    configBuilder.SetPosition(LevelPlayBannerPosition.BottomCenter);
                    configBuilder.SetDisplayOnLoad(true);
                    configBuilder.SetRespectSafeArea(true); // Only relevant for Android
                    configBuilder.SetPlacementName("bannerPlacement");
                    configBuilder.SetBidFloor(0.01); // Minimum bid price in USD
                    var bannerConfig = configBuilder.Build();

                    bannerAd = new LevelPlayBannerAd(BannerAdUnitId, bannerConfig);
                    bannerAd.LoadAd();
                }
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            _isBannerRetrying = false;
        }
    }

    private async UniTaskVoid RetryLoadRewardedAd(LevelPlayRewardedAd ad, Define.ECurrency currency, CancellationToken token)
    {
        int index = (int)currency;

        if (retrying[index])
            return;

        retrying[index] = true;

        try
        {
            while (!token.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(10), cancellationToken: token);

                Debug.Log($"{currency} rewarded retry load");
                ad.LoadAd();
            }
        }
        finally
        {
            retrying[index] = false;
        }
    }

    public bool IsRewardedReady(Define.ECurrency currency)
    {
        var ad = rewardedVideoAds[(int)currency];
        return ad != null && ad.IsAdReady();
    }

    #region Events

    private void SdkInitializationCompletedEvent(LevelPlayConfiguration config)
    {
        EnableAds();
        IsAdsEnabled = true;
    }

    private void SdkInitializationFailedEvent(LevelPlayInitError error)
    {

    }

    void RewardedVideoOnLoadedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnLoadedEvent With AdInfo: {adInfo}");
    }

    void RewardedVideoOnAdLoadFailedEvent(LevelPlayAdError error)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdLoadFailedEvent With Error: {error}");
    }

    void RewardedVideoOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdDisplayedEvent With AdInfo: {adInfo}");
    }

    void RewardedVideoOnAdDisplayedFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdDisplayedFailedEvent With AdInfo: {adInfo} and Error: {error}");
    }

    void RewardedVideoOnAdRewardedEvent(LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdRewardedEvent With AdInfo: {adInfo} and Reward: {reward}");
    }

    void RewardedVideoOnAdClickedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdClickedEvent With AdInfo: {adInfo}");
    }

    void RewardedVideoOnAdClosedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdClosedEvent With AdInfo: {adInfo}");
    }

    void RewardedVideoOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received RewardedVideoOnAdInfoChangedEvent With AdInfo {adInfo}");
    }

    void InterstitialOnAdLoadedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdLoadedEvent With AdInfo: {adInfo}");
    }

    void InterstitialOnAdLoadFailedEvent(LevelPlayAdError error)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdLoadFailedEvent With Error: {error}");
    }

    void InterstitialOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdDisplayedEvent With AdInfo: {adInfo}");
    }

    void InterstitialOnAdDisplayFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdDisplayFailedEvent With AdInfo: {adInfo} and Error: {error}");
    }

    void InterstitialOnAdClickedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdClickedEvent With AdInfo: {adInfo}");
    }

    void InterstitialOnAdClosedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdClosedEvent With AdInfo: {adInfo}");
    }

    void InterstitialOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received InterstitialOnAdInfoChangedEvent With AdInfo: {adInfo}");
    }

    void BannerOnAdLoadedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdLoadedEvent With AdInfo: {adInfo}");
    }

    void BannerOnAdLoadFailedEvent(LevelPlayAdError error)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdLoadFailedEvent With Error: {error}");
    }

    void BannerOnAdClickedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdClickedEvent With AdInfo: {adInfo}");
    }

    void BannerOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdDisplayedEvent With AdInfo: {adInfo}");
    }

    void BannerOnAdDisplayFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdDisplayFailedEvent With AdInfo: {adInfo} and Error: {error}");
    }

    void BannerOnAdCollapsedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdCollapsedEvent With AdInfo: {adInfo}");
    }

    void BannerOnAdLeftApplicationEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdLeftApplicationEvent With AdInfo: {adInfo}");
    }

    void BannerOnAdExpandedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"[LevelPlaySample] Received BannerOnAdExpandedEvent With AdInfo: {adInfo}");
    }

    void ImpressionDataReadyEvent(LevelPlayImpressionData impressionData)
    {
        Debug.Log($"[LevelPlaySample] Received ImpressionDataReadyEvent ToString(): {impressionData}");
        Debug.Log($"[LevelPlaySample] Received ImpressionDataReadyEvent allData: {impressionData.AllData}");
    }
    #endregion

    public void SetBannerPosition(Vector2 newPosition)
    {
        //bannerAd = new LevelPlayBannerAd("bwdq9r9c6nw4d7t9", bannerConfig);

        LevelPlayBannerPosition position = new LevelPlayBannerPosition(newPosition);

        var configBuilder = new LevelPlayBannerAd.Config.Builder();
        configBuilder.SetSize(LevelPlayAdSize.LARGE);
        configBuilder.SetPosition(position);
        configBuilder.SetDisplayOnLoad(true);
        configBuilder.SetRespectSafeArea(true); // Only relevant for Android
        configBuilder.SetPlacementName("bannerPlacement");
        configBuilder.SetBidFloor(0.01); // Minimum bid price in USD
        var bannerConfig = configBuilder.Build();

        bannerAd = new LevelPlayBannerAd(BannerAdUnitId, bannerConfig);
    }

    public void LoadBanner()
    {
        bannerAd.LoadAd();

        _isBannerRetrying = true;
    }

    public void ShowRewardAd(Define.ECurrency currency)
    {
        if (rewardedVideoAds[(int)currency].IsAdReady())
        {
            Debug.Log($"(int)currency {(int)currency}");
            rewardedVideoAds[(int)currency].ShowAd();
        }
    }

    #region Helper
    private static string AppKey => GetAppKey();
    private static string BannerAdUnitId => GetBannerAdUnitId();
    private static string InterstitalAdUnitId => GetInterstitialAdUnitId();
    private static string RewardedVideoAdUnitId(Define.ECurrency currency) => GetRewardedVideoAdUnitId(currency);

    static string GetAppKey()
    {
#if UNITY_ANDROID
        return "24948f305";
#elif UNITY_IPHONE
            return "8545d445";
#else
            return "unexpected_platform";
#endif
    }

    static string GetBannerAdUnitId()
    {
#if UNITY_ANDROID
        return "bwdq9r9c6nw4d7t9";
#elif UNITY_IPHONE
            return "iep3rxsyp9na3rw8";
#else
            return "unexpected_platform";
#endif
    }
    static string GetInterstitialAdUnitId()
    {
#if UNITY_ANDROID
        return "aeyqi3vqlv6o8sh9";
#elif UNITY_IPHONE
            return "wmgt0712uuux8ju4";
#else
            return "unexpected_platform";
#endif
    }

    static string GetRewardedVideoAdUnitId(Define.ECurrency currency)
    {
#if UNITY_ANDROID
        if (currency == Define.ECurrency.Gold)
            return "nqtypyp6wpx0i4f5";
        else if (currency == Define.ECurrency.Iron)
            return "rhftz04os3wbbimo";
        else if (currency == Define.ECurrency.Coal)
            return "xmsw7fw2utqqmtw2";
        else return null;
#elif UNITY_IPHONE
            return "qwouvdrkuwivay5q";
#else
            return "unexpected_platform";
#endif
    }
    #endregion

    #region Action
    public Action<Define.ECurrency, LevelPlayAdInfo> OnRewardedLoaded;
    public Action<Define.ECurrency, string> OnRewardedLoadFailed;
    public Action<Define.ECurrency, LevelPlayAdInfo> OnRewardedClosed;
    public Action<Define.ECurrency, LevelPlayAdInfo, LevelPlayReward> OnRewardedEarned;
    #endregion

}
