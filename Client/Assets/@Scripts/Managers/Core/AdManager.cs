using Cysharp.Threading.Tasks;
using System;
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
    private bool _isBannerActivated = false;
    private CancellationTokenSource adsCts;
    private CancellationTokenSource initCts;

    public void Init()
    {
        LevelPlay.OnInitSuccess -= SdkInitializationCompletedEvent;
        LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed -= SdkInitializationFailedEvent;
        LevelPlay.OnInitFailed += SdkInitializationFailedEvent;

        // SDK init
        LevelPlay.Init(AppKey);
    }

    private async UniTaskVoid RetryInitialization()
    {
        initCts?.Cancel();
        initCts?.Dispose();
        initCts = new CancellationTokenSource();
        CancellationToken token = initCts.Token;

        try
        {
            Debug.Log("LevelPlay Init Failed. Retrying in 5 seconds...");
            await UniTask.Delay(TimeSpan.FromSeconds(5), cancellationToken: token);
            Debug.Log("Retrying LevelPlay Init...");
            LevelPlay.Init(AppKey);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("LevelPlay Initialization retry cancelled.");
        }
    }

    public void Clear()
    {
        adsCts?.Cancel();
        adsCts?.Dispose();
        adsCts = null;

        initCts?.Cancel();
        initCts?.Dispose();
        initCts = null;

        LevelPlay.OnInitSuccess -= SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed -= SdkInitializationFailedEvent;
    }

    // ILHAK temp 나중에 Clear호출하도록 변경
    private void OnDisable() 
    {
        adsCts?.Cancel();
        adsCts?.Dispose();
        adsCts = null;

        initCts?.Cancel();
        initCts?.Dispose();
        initCts = null;

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
        }

        CreateBanner();
        
        // 배너 감시 루프 시작 (한 번만 실행됨)
        if (adsCts != null)
            RetryLoadBannerAd(adsCts.Token).Forget();
    }

    private void CreateBanner()
    {
        if (bannerAd != null)
        {
            bannerAd.DestroyAd();
            bannerAd = null;
        }

        var configBuilder = new LevelPlayBannerAd.Config.Builder();
        configBuilder.SetSize(LevelPlayAdSize.BANNER);
        configBuilder.SetPosition(LevelPlayBannerPosition.BottomCenter);
        configBuilder.SetDisplayOnLoad(true);
        configBuilder.SetRespectSafeArea(true); 
        configBuilder.SetPlacementName("bannerPlacement");
        configBuilder.SetBidFloor(0.01); 
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

        // 설정 단계에서는 직접 LoadAd를 호출하지 않음 (LoadBanner에서 호출)
    }

    private async UniTaskVoid RetryLoadBannerAd(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                // 주기적인 모니터링 (30초 간격)
                await UniTask.Delay(TimeSpan.FromSeconds(30), cancellationToken: token);

                // 사용자가 활성화를 원하는데 배너가 없거나 문제가 발생한 경우 복구
                if (_isBannerActivated)
                {
                    if (bannerAd == null)
                    {
                        Debug.Log("Banner Ad object lost. Recreating...");
                        CreateBanner();
                        bannerAd?.LoadAd();
                    }
                }
            }
        }
        catch (OperationCanceledException) { }
        finally
        {

        }
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
        initCts?.Cancel();
        initCts?.Dispose();
        initCts = null;

        EnableAds();
        IsAdsEnabled = true;
    }

    private void SdkInitializationFailedEvent(LevelPlayInitError error)
    {
        Debug.LogError($"LevelPlay Init Failed: {error.ErrorMessage}");
        RetryInitialization().Forget();
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
        _isBannerActivated = true;

        if (bannerAd == null)
            CreateBanner();
        
        bannerAd?.LoadAd();
    }

    public void ShowRewardAd(Define.ECurrency currency)
    {
        if (rewardedVideoAds[(int)currency].IsAdReady())
        {
            Debug.Log($"(int)currency {(int)currency}");
            rewardedVideoAds[(int)currency].ShowAd();
        }
    }

    public void RefreshAds()
    {
        if (IsAdsEnabled == false)
        {
            Debug.Log("RefreshAds: LevelPlay not initialized. Retrying Init...");
            Init();
            return;
        }

        Debug.Log("RefreshAds: Refreshing Ads...");

        // 1. 배너 광고 복구
        if (_isBannerActivated)
        {
            if (bannerAd == null)
                CreateBanner();
            
            bannerAd?.LoadAd();
        }

        // 2. 보상형 광고 체크 및 로드
        if (rewardedVideoAds != null)
        {
            for (int i = 0; i < rewardedVideoAds.Length; i++)
            {
                var ad = rewardedVideoAds[i];
                if (ad == null) continue;

                Define.ECurrency currency = (Define.ECurrency)i;
                if (currency == Define.ECurrency.None) continue;

                // 광고가 준비되지 않았고, 현재 재시도 루틴이 도는 중이 아니라면 로드 시도
                if (ad.IsAdReady() == false && (retrying.Length > i && retrying[i] == false))
                {
                    Debug.Log($"RefreshAds: Refreshing Rewarded Ad for {currency}");
                    ad.LoadAd();
                }
            }
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
