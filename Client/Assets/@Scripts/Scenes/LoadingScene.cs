using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebPacket;

public class LoadingScene : BaseScene
{
    private UI_LoadingPopup _ui;
    public Define.EScene _nextSceneType;


    protected override void Awake()
    {
        base.Awake();
        SceneType = Define.EScene.LoadingScene;
        _ui = Managers.UI.ShowPopupUI<UI_LoadingPopup>();
    }

    protected override void Start()
    {
        base.Start();

        _ui.SetSliderValue(0f);

        // Temp
        StartCoroutine(LoadNextScene());

        // Player데이터 가저오기
        //LoadPlayerData().Forget();
    }

    //private async UniTask LoadPlayerData()
    //{
    //    var req = new PlayerPacketReq()
    //    {
    //        jwt = Managers.Web.jwt,
    //    };

    //    PlayerPacketRes res = await Managers.Web.SendPostRequestAsync<PlayerPacketRes>("api/game/player", req);

    //    if (res.Success)
    //    {
    //        Managers.Game.UpdatePlayerData(res.PlayerData);

    //        LoadCurrencyData().Forget();
    //    }
    //    else
    //    {
    //        Log.LogError("Get Player Failed.");
    //    }
    //}

    //private async UniTask LoadCurrencyData()
    //{
    //    var req = new CurrencyAllReq()
    //    {
    //        jwt = Managers.Web.jwt,
    //    };

    //    CurrencyAllRes res = await Managers.Web.SendPostRequestAsync<CurrencyAllRes>("api/game/currency", req);

    //    if (res.Success)
    //    {
    //        Log.Log($"Currency Gold : {res.currencyData.Gold}");

    //        // Update local currency data
    //        Managers.Game.UpdateCurrency(res.currencyData);

    //        // Load hero data next
    //        LoadHeroData().Forget();
    //    }
    //    else
    //    {
    //        Log.LogError($"Get Currency Failed.");
    //    }
    //}

    //private async UniTask LoadHeroData()
    //{
    //    {
    //        var req = new HeroListReq()
    //        {
    //            Jwt = Managers.Web.jwt,
    //        };

    //        HeroListRes res = await Managers.Web.SendPostRequestAsync<HeroListRes>("api/game/hero", req);

    //        if (res.Success)
    //        {
    //            // 1. Update hero data
    //            await Managers.Game.UpdateHeroData(res.Heroes);
    //        }
    //        else
    //        {
    //            Log.LogError($"Get Hero Failed.");
    //        }
    //    }

    //    {
    //        await Managers.Game.UpdateBuddyData();
    //    }

    //    {
    //        await Managers.Game.UpdateStageClearList();
    //    }

    //    {
    //        await Managers.Game.PlayerTimeCheck();
    //    }

    //    {
    //        await Managers.Game.UpdateMission();
    //    }

    //    {
    //        await Managers.Game.UpdateAchievement();
    //    }

    //    // 2. Proceed to next scene
    //    _nextSceneType = Managers.Scene.NextSceneType;

    //    if(_nextSceneType == Define.EScene.GameScene)
    //    {
    //        await LoadStageData();
    //    }

    //    StartCoroutine(LoadNextScene());
    //}

    public override void Clear()
    {
    }

    //private async UniTask LoadStageData()
    //{
    //    var req = new StageStartDataReq()
    //    {
    //        Jwt = Managers.Web.jwt,
    //    };

    //    StageStartDataRes res = await Managers.Web.SendPostRequestAsync<StageStartDataRes>("api/game/stage/getStageData", req);

    //    if (res.Success)
    //    {
    //        // 1. Update hero data
    //        await Managers.Game.SaveStageData(res);
    //    }
    //    else
    //    {
    //        Log.LogError($"Get Stage data Failed.");
    //    }
    //}

    IEnumerator LoadNextScene()
    {
        // TODO fake loading
        yield return new WaitForSeconds(1f);

        AsyncOperation operation = SceneManager.LoadSceneAsync(Managers.Scene.GetSceneName(_nextSceneType));
        operation.allowSceneActivation = false; // 씬의 자동 전환 false

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            _ui.SetSliderValue(operation.progress);

            if (operation.progress >= 0.9f)
            {
                _ui.SetSliderValue(1f);

                // progressBar.value = 1f;
                // progressText.text = "100%";
                operation.allowSceneActivation = true;
                Managers.Clear();
            }

            yield return null;
        }
    }
}