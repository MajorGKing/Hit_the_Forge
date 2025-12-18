using Unity.Services.LevelPlay;
using UnityEngine;

public class AdManager
{
    public LevelPlayBannerAd bannerAd;

    public void Init()
    {
        LevelPlay.OnInitSuccess -= SdkInitializationCompletedEvent;
        LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed -= SdkInitializationFailedEvent;
        LevelPlay.OnInitFailed += SdkInitializationFailedEvent;
        // SDK init
        LevelPlay.Init("24948f305");

       

    }

    public void Clear()
    {
        LevelPlay.OnInitSuccess -= SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed -= SdkInitializationFailedEvent;
    }

    private void SdkInitializationCompletedEvent(LevelPlayConfiguration config)
    {

    }

    private void SdkInitializationFailedEvent(LevelPlayInitError error)
    {

    }

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
        configBuilder.SetBidFloor(1.0); // Minimum bid price in USD
        var bannerConfig = configBuilder.Build();

        bannerAd = new LevelPlayBannerAd("bwdq9r9c6nw4d7t9", bannerConfig);
    }

}
