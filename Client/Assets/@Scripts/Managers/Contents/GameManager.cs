using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;




public class GameManager
{
    private enum EWeaponMakeProcess
    {
        None,
        BeginHold, // 무기를 고르기 전
        Ready, // 무기를 고른 후
        Progress, // 작업 중
        Finish, // 무기 완성 후
        Enhancement, // 무기 업그레이드 대기
        EnhancementProgress,
        Sell, // 판매
    }

    #region Base

    private GameScene _scene;
    private bool _nowGameScene = true;

    
    float shakeCooldown = 0.4f;
    float lastShakeTime = -10f;

    public void Clear()
    {
        regenerateIronCTS?.Cancel();
        regenerateIronCTS?.Dispose();
        regenerateIronCTS = null;

        regenerateCoalCTS?.Cancel();
        regenerateCoalCTS?.Dispose();
        regenerateCoalCTS = null;
    }

    public void Init()
    {
        
    }

    public void Update()
    {
        UpdateInput();
    }

    private void UpdateInput()
    {
        if (IsPointerOverUIObject(Input.mousePosition))
            return;

        if (Input.GetMouseButtonDown(0))
        {

        }
        else if (Input.GetMouseButtonUp(0))
        {
            //Debug.Log("Touch Position: " + Input.mousePosition);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null)
            {
                //Debug.Log(hit.transform.name);

                if (hit.transform.TryGetComponent<ForgeController>(out var forge))
                {
                    forge.HitForge();

                    Vector2 basePos = hit.point;

                    float offsetRange = 1f; // 랜덤 Range
                    float offsetX = Random.Range(-offsetRange, offsetRange);
                    float offsetY = Random.Range(-offsetRange, offsetRange);

                    Vector2 spawnPos = new Vector2(basePos.x + offsetX, basePos.y + offsetY);

                    // 히트 이펙트 스폰
                    SpawnHitEffect(spawnPos).Forget();

                    TryShakeCameraRandom();
                }
            }
        }
    }

    private async UniTaskVoid SpawnHitEffect(Vector2 pos)
    {
        if (makeProcessState != EWeaponMakeProcess.Ready && makeProcessState != EWeaponMakeProcess.Progress)
            return;

        var effect = Managers.Object.SpawnGameObject(pos, "HitEffect01");
        await UniTask.Delay(100);
        Managers.Resource.Destroy(effect);
    }

    private void TryShakeCameraRandom()
    {
        if (makeProcessState == EWeaponMakeProcess.Ready || makeProcessState == EWeaponMakeProcess.Progress)
        {
            if (Time.time - lastShakeTime < shakeCooldown)
                return;

            lastShakeTime = Time.time;

            Camera.main.transform.DOShakePosition(
                duration: 0.12f,
                strength: 0.06f,
                vibrato: 8,
                randomness: 90f,
                fadeOut: true
            );
        }
        else if (makeProcessState == EWeaponMakeProcess.Enhancement)
        {
            Camera.main.transform.DOShakePosition(
                duration: 0.25f,
                strength: 0.22f,
                vibrato: 18,
                randomness: 90f,
                fadeOut: true
            );
        }

    }

    public bool IsPointerOverUIObject(Vector2 touchPos)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = touchPos;
        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        return results.Count > 0;
    }
    #endregion

    #region Action
    public event Action OnWeaponHpChanged;
    public event Action OnEnhancementCountChanged;
    public event Action OnEnhancementPercentChanged;
    public event Action OnWeaponEnhancementSucess;
    public event Action OnWeaponEnhancementFail;
    public event Action OnWeaponFinish;
    public event Action OnDoSave;
    public event Action OnNewWeaponAdded;
    public event Action OnWeaponSelected;
    #endregion

    #region Variables

    long weaponMaxHp = 100;
    public long WeaponMaxHp
    {
        protected set { weaponMaxHp = value; OnWeaponHpChanged?.Invoke(); }
        get { return weaponMaxHp; }
    }
    long weaponHp = 0;
    public long WeaponHp
    {
        protected set { weaponHp = value; OnWeaponHpChanged?.Invoke(); }
        get { return weaponHp; }
    }

    EWeaponMakeProcess makeProcessState = EWeaponMakeProcess.None;
    bool useCoal = false;
    
    private CancellationTokenSource coalCTS;
    private CancellationTokenSource regenerateIronCTS;
    private CancellationTokenSource regenerateCoalCTS;

    private float enhancementCountTime;
    public float EnhancementCountTime
    {
        protected set { enhancementCountTime = value; OnEnhancementCountChanged?.Invoke(); }
        get { return enhancementCountTime; }
    }
    private int enhancementLevel;
    protected int EnhancementLevel
    {
        set { enhancementLevel = value; OnEnhancementPercentChanged?.Invoke(); }
        get { return enhancementLevel; }
    }

    public int GetEnhancementLevel()
    {
        if (makeProcessState == EWeaponMakeProcess.Enhancement)
                return EnhancementLevel;

            return 0;
    }

    private Data.WeaponData currentWeaponInfo;
    public Data.WeaponData CurrentWeaponInfo
    {
        get => currentWeaponInfo;
    }

    #endregion

    #region FSM Core
    private Coroutine _currentStateRoutine = null;

    private void ChangeState(EWeaponMakeProcess next)
    {
        if (makeProcessState == next)
            return;

        makeProcessState = next;

        if (_currentStateRoutine != null)
            Managers.Instance.StopCoroutine(_currentStateRoutine);

        _currentStateRoutine = Managers.Instance.StartCoroutine(GetStateCoroutine(next));
    }

    private IEnumerator GetStateCoroutine(EWeaponMakeProcess state)
    {
        switch (state)
        {
            case EWeaponMakeProcess.BeginHold: return CoBeginHold();
            case EWeaponMakeProcess.Ready: return CoReady();
            case EWeaponMakeProcess.Progress: return CoProgress();
            case EWeaponMakeProcess.Finish: return CoFinish();
            case EWeaponMakeProcess.Enhancement: return CoEnhancement();
            case EWeaponMakeProcess.EnhancementProgress: return CoEnhancementProgress();
            case EWeaponMakeProcess.Sell: return CoSell();
        }

        return null;
    }
    #endregion

    #region FSM States
    private IEnumerator CoBeginHold()
    {
        WeaponHp = 0;
        //currentWeaponInfo = null;
        useCoal = false;

        while (makeProcessState == EWeaponMakeProcess.BeginHold)
            yield return null;
    }

    private IEnumerator CoReady()
    {
        Managers.Player.CurrencySubtract(Define.ECurrency.Iron, currentWeaponInfo.Iron);

        coalCTS?.Cancel();
        coalCTS = null;

        useCoal = false;

        WeaponHp = 0;
        WeaponMaxHp = currentWeaponInfo.HP;

        while (makeProcessState == EWeaponMakeProcess.Ready)
            yield return null;
    }

    private IEnumerator CoProgress()
    {
        while (makeProcessState == EWeaponMakeProcess.Progress)
            yield return null;
    }

    private IEnumerator CoFinish()
    {
        TryShakeCameraRandom();
        Managers.Sound.Play(Define.ESound.Effect, "FinishEffectSound1");
        OnWeaponFinish?.Invoke();

        yield return new WaitForSeconds(0.15f);

        ChangeState(EWeaponMakeProcess.Enhancement);
    }

    private IEnumerator CoEnhancement()
    {
        enhancementCountTime = 3f;
        EnhancementLevel = 1;

        while (makeProcessState == EWeaponMakeProcess.Enhancement)
        {
            EnhancementCountTime -= Time.deltaTime;

            if (enhancementCountTime <= 0)
            {
                EnhancementCountTime = 0f;
                ChangeState(EWeaponMakeProcess.Sell);
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator CoEnhancementProgress()
    {
        while (makeProcessState == EWeaponMakeProcess.EnhancementProgress)
        {
            yield return null;
        }
    }

    private IEnumerator CoSell()
    {
        EnhancementCountTime = 0;

        if (currentWeaponInfo != null)
        {
            long price = GetSellPrice();

            Managers.Player.CurrencyAdd(Define.ECurrency.Gold, price);
            Managers.Sound.Play(Define.ESound.Effect, "SellEffect");
        }

        if (currentWeaponInfo.NextTemplateId > 0)
        {
            if (Managers.Player.HasWeapon(currentWeaponInfo.NextTemplateId) == false)
            {
                Managers.Player.AddOwnedWeapon(currentWeaponInfo.NextTemplateId);

                var nextWeaponName = Managers.Data.WeaponDict[currentWeaponInfo.NextTemplateId].WeaponName;

                Managers.UI.ShowToast($"{nextWeaponName} 추가 되었습니다.", 1, Define.EToastColor.Blue, Define.EToastPosition.MiddleCenter);

                OnNewWeaponAdded?.Invoke();
            }
        }

        ChangeState(EWeaponMakeProcess.BeginHold);
        yield return null;
    }
    #endregion

    #region GameLogic

    // 게임씬 들어가면 호출
    public void GameInit()
    {
        makeProcessState = EWeaponMakeProcess.None;
        ChangeState(EWeaponMakeProcess.BeginHold);

        regenerateIronCTS = new CancellationTokenSource();
        regenerateCoalCTS = new CancellationTokenSource();

        RegenerateResource(Define.ECurrency.Iron, Define.EPlayerTownStat.IronRegeneration, regenerateIronCTS.Token).Forget();
        RegenerateResource(Define.ECurrency.Coal, Define.EPlayerTownStat.CoalRegeneration, regenerateCoalCTS.Token).Forget();
    }

    public void CalcWeaponHit()
    {
        //if (makeProcessState != EWeaponMakeProcess.BeginHold && makeProcessState != EWeaponMakeProcess.Ready && makeProcessState != EWeaponMakeProcess.Progress)
        //    return (float)WeaponHp / WeaponMaxHp;

        if(currentWeaponInfo == null)
                return;

        if(makeProcessState == EWeaponMakeProcess.BeginHold)
        {
            StartWeaponMake(currentWeaponInfo.TemplateId);
        }

        if (makeProcessState == EWeaponMakeProcess.Ready)
            ChangeState(EWeaponMakeProcess.Progress);

        //makeProcessState = EWeaponMakeProcess.Progress;

        if (makeProcessState != EWeaponMakeProcess.Progress)
            return;

        // Check Coal
        var needCoal = currentWeaponInfo.Coal;

        if (useCoal == false)
        {
            if (needCoal > Managers.Player.GetCurrency(Define.ECurrency.Coal))
            {
                // TODO ILHAK 석탄 부족 표시
                Managers.UI.ShowToast("연료가 부족합니다.", 1, Define.EToastColor.Red, Define.EToastPosition.MiddleCenter);
                return;
            }

            UseCoal().Forget();
        }

        long currentWeaponHp = WeaponHp + Managers.Player.GetPlayerStat(Define.EPlayerStat.Str);

        if (currentWeaponHp >= WeaponMaxHp)
        {
            WeaponHp = WeaponMaxHp;
            ChangeState(EWeaponMakeProcess.Finish);
        }
        else
        {
            int randomIndex = Random.Range(1, 5);
            var effectSound = $"HitEffectSound{randomIndex}";
            Managers.Sound.Play(Define.ESound.Effect, effectSound);
            WeaponHp = currentWeaponHp;
        }

        //Debug.Log(weaponHp);

        return;
    }


    public bool StartWeaponMake(int templateId)
    {
        if(makeProcessState != EWeaponMakeProcess.BeginHold) 
            return false;

        var weaponInfo = Managers.Data.WeaponDict[templateId];

        if (weaponInfo == null)
            return false;

        if (weaponInfo.Iron > Managers.Player.GetCurrency(Define.ECurrency.Iron))
        {
            Managers.UI.ShowToast("재료가 부족합니다.", 1, Define.EToastColor.Red, Define.EToastPosition.MiddleCenter);
            return false;
        }

        currentWeaponInfo = weaponInfo;
        ChangeState(EWeaponMakeProcess.Ready);

        OnWeaponSelected?.Invoke();

        return true;
    }

    public float GetEnhancementCount()
    {
        if(makeProcessState == EWeaponMakeProcess.Enhancement)
            return enhancementCountTime;

        return -1f;
    }

    public float GetEnhancementPercent()
    {
        var info = Managers.Data.EnhancementDict[EnhancementLevel];
        var suceeValue = CalEnhancemenetPercent(info);
        float returnValue = suceeValue / info.BasicSucess;
        returnValue = Mathf.Round(returnValue * 10000f)/100f;

        return returnValue;
    }

    public long GetSellPrice()
    {
        var price = currentWeaponInfo.Price * Managers.Data.EnhancementDict[EnhancementLevel - 1].Price;
        var bonusePrice = price * (Managers.Player.GetTownStat(Define.EPlayerTownStat.ShopSellBonus)/(float)1000);
        //Debug.Log($"Sell Price {price} + Bounse Price {bonusePrice} = {(int)price + (int)bonusePrice}");
        
        return (long)(price + bonusePrice);
    }

    public void SellWeapon()
    {
        if (makeProcessState != EWeaponMakeProcess.Finish && makeProcessState != EWeaponMakeProcess.Enhancement)
            return;

        if (currentWeaponInfo == null)
            return;

        ChangeState(EWeaponMakeProcess.Sell);
    }

    public void DoEnhancemenet()
    {
        if (makeProcessState != EWeaponMakeProcess.Enhancement)
            return;

        Data.EnhancementData enhancementData = Managers.Data.EnhancementDict[EnhancementLevel];

        if (enhancementData == null)
            return;

        var value = Random.Range(0, enhancementData.BasicSucess);

        var suceeValue = CalEnhancemenetPercent(enhancementData);
        //var suceeValue = enhancementData.EnhancementSucess + (enhancementData.EnhancementSucess * Managers.Player.GetPlayerStat(Define.EPlayerStat.Mastery))/100f;


        if (value <= suceeValue)
        {
            EnhancementLevel++;
            enhancementCountTime = 3f;

            Managers.Sound.Play(Define.ESound.Effect, "EnhancementSucessSound1");

            OnWeaponEnhancementSucess?.Invoke();
        }
        else
        {
            WaitFail().Forget();
        }
    }

    private async UniTaskVoid WaitFail()
    {
        EnhancementLevel = 0;
        enhancementCountTime = 0f;
        
        Managers.Sound.Play(Define.ESound.Effect, "EnhancementFailSound1");

        OnWeaponEnhancementFail?.Invoke();

        ChangeState(EWeaponMakeProcess.EnhancementProgress);

        await UniTask.Delay(500);

        ChangeState(EWeaponMakeProcess.BeginHold);
    }

    private async UniTaskVoid UseCoal()
    {
        // 이전 작업이 있다면 취소
        coalCTS?.Cancel();
        coalCTS = null;

        // 새로운 CTS 생성
        coalCTS = new CancellationTokenSource();

        Managers.Player.CurrencySubtract(Define.ECurrency.Coal, currentWeaponInfo.Coal);

        useCoal = true;

        try
        {
            var waitTime = (int)Managers.Player.GetForgeStat(Define.EPlayerForgeStat.CoalTime);
                
            await UniTask.Delay(waitTime, cancellationToken: coalCTS.Token);
        }
        catch (OperationCanceledException)
        {

        }

        useCoal = false;
    }

    private async UniTaskVoid RegenerateResource(Define.ECurrency currency, Define.EPlayerTownStat regenStat, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                await UniTask.Delay(3000, cancellationToken: token);

                Managers.Player.CurrencyAdd(currency, Managers.Player.GetTownStat(regenStat));
            }
        }
        catch (OperationCanceledException)
        {
            
        }
    }
    #endregion

    #region Helper
    private float CalEnhancemenetPercent(Data.EnhancementData enhancementData)
    {
        var returnValue = enhancementData.EnhancementSucess
            + (enhancementData.EnhancementSucess * Managers.Player.GetPlayerStat(Define.EPlayerStat.Mastery)) / 100f
            + (enhancementData.EnhancementSucess * Managers.Player.GetForgeStat(Define.EPlayerForgeStat.Mastery)) / 100f;

        if (returnValue > enhancementData.BasicSucess)
        {
            returnValue = enhancementData.BasicSucess;
        }

        return returnValue;
    }
    #endregion
}