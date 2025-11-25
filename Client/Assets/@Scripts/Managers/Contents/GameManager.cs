using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
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
        if (makeProcess != EWeaponMakeProcess.Ready && makeProcess != EWeaponMakeProcess.Progress)
            return;

        var effect = Managers.Object.SpawnGameObject(pos, "HitEffect01");
        await UniTask.Delay(100);
        Managers.Resource.Destroy(effect);
    }

    private void TryShakeCameraRandom()
    {
        if (makeProcess == EWeaponMakeProcess.Ready || makeProcess == EWeaponMakeProcess.Progress)
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
        else if (makeProcess == EWeaponMakeProcess.Enhancement)
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
    public event Action OnWeaponHpChagned;
    #endregion

    #region GameScene

    int weaponMaxHp = 100;
    public int WeaponMaxHp
    {
        protected set { weaponMaxHp = value; OnWeaponHpChagned?.Invoke(); }
        get { return weaponMaxHp; }
    }
    int weaponHp = 0;
    public int WeaponHp
    {
        protected set { weaponHp = value; OnWeaponHpChagned?.Invoke(); }
        get { return weaponHp; }
    }

    EWeaponMakeProcess makeProcess = EWeaponMakeProcess.None;
    bool useCoal = false;
    private CancellationTokenSource coalCTS;

    Data.WeaponData currentWeaponInfo;

    private CancellationTokenSource regenerateIronCTS;
    private CancellationTokenSource regenerateCoalCTS;


    // 게임씬 들어가면 호출
    public void GameInit()
    {
        makeProcess = EWeaponMakeProcess.BeginHold;

        regenerateIronCTS = new CancellationTokenSource();
        regenerateCoalCTS = new CancellationTokenSource();

        RegenerateResource(Define.ECurrency.Iron, Define.EPlayerTownStat.RegenerateIron, regenerateIronCTS.Token).Forget();
        RegenerateResource(Define.ECurrency.Coal, Define.EPlayerTownStat.RegenerateCoal, regenerateCoalCTS.Token).Forget();
    }

    public float CalcWeaponHit()
    {
        if (makeProcess != EWeaponMakeProcess.BeginHold && makeProcess != EWeaponMakeProcess.Ready && makeProcess != EWeaponMakeProcess.Progress)
            return 0f;

        makeProcess = EWeaponMakeProcess.Progress;

        // Check Coal
        var needCoal = currentWeaponInfo.Coal;

        if (useCoal == false)
        {
            if (needCoal > Managers.Player.GetCurrency(Define.ECurrency.Coal))
            {
                // TODO 석탄 부족 표시
                return (float)WeaponHp / WeaponMaxHp;
            }

            UseCoal().Forget();
        }

        var currentWeaponHp = WeaponHp + Managers.Player.GetPlayerStat(Define.EPlayerStat.Str);

        if (currentWeaponHp >= WeaponMaxHp)
        {
            currentWeaponHp = WeaponMaxHp;
            WeaponHp = currentWeaponHp;
            MakeFinish();
        }
        else
        {
            int randomIndex = Random.Range(1, 5);
            var effectSound = $"HitEffectSound{randomIndex}";
            Managers.Sound.Play(Define.ESound.Effect, effectSound);
            WeaponHp = currentWeaponHp;
        }

        //Debug.Log(weaponHp);

        return (float)WeaponHp / WeaponMaxHp;
    }

    public bool StartWeaponMake(int templateId)
    {
        if(makeProcess != EWeaponMakeProcess.BeginHold) 
            return false;

        var weaponInfo = Managers.Data.WeaponDict[templateId];

        if (weaponInfo == null)
            return false;

        if (weaponInfo.Iron > Managers.Player.GetCurrency(Define.ECurrency.Iron))
            return false;


        Managers.Player.CurrencySubtract(Define.ECurrency.Iron, weaponInfo.Iron);

        makeProcess = EWeaponMakeProcess.Ready;
        currentWeaponInfo = weaponInfo;

        coalCTS?.Cancel();
        coalCTS = null;

        useCoal = false;

        WeaponHp = 0;
        WeaponMaxHp = currentWeaponInfo.HP;

        return true;
    }

    private void MakeFinish()
    {
        if (currentWeaponInfo == null)
            return;

        Managers.Sound.Play(Define.ESound.Effect, "FinishEffectSound1");
        makeProcess = EWeaponMakeProcess.Finish;

        // TODO Enhancement
        makeProcess = EWeaponMakeProcess.Enhancement;
        TryShakeCameraRandom();

        // TODO Sell
        SellWeapon();

        // TODO Restart
        //StartWeaponMake(currentWeaponInfo.TemplateId);
        makeProcess = EWeaponMakeProcess.BeginHold;
    }

    private void SellWeapon()
    {
        if (makeProcess != EWeaponMakeProcess.Finish && makeProcess != EWeaponMakeProcess.Enhancement)
            return;

        if (currentWeaponInfo == null)
            return;

        makeProcess = EWeaponMakeProcess.Sell;

        Managers.Player.CurrencyAdd(Define.ECurrency.Gold, currentWeaponInfo.Price);

        WeaponHp = 0;
    }

    public async UniTaskVoid UseCoal()
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
            await UniTask.Delay(
                Managers.Player.GetForgeStat(Define.EPlayerForgeStat.CoalTime),
                cancellationToken: coalCTS.Token
            );
        }
        catch (OperationCanceledException)
        {

        }

        useCoal = false;
    }

    private async UniTaskVoid RegenerateIron()
    {
        while(true)
        {
            await UniTask.Delay(3000);
            Managers.Player.CurrencyAdd(Define.ECurrency.Iron, Managers.Player.GetTownStat(Define.EPlayerTownStat.RegenerateIron));
        }
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
}