using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;


public class GameManager
{
    #region Base

    private GameScene _scene;
    private bool _nowGameScene = true;

    public void Clear()
    {

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
                }
            }
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

    #region GameScene

    int weaponMaxHp = 100;
    int weaponHp = 0;

    bool isWork = false;
    bool useCoal = false;
    private CancellationTokenSource coalCTS;

    Data.WeaponData currentWeaponInfo;
    public float CalcWeaponHit()
    {
        if (isWork == false)
            return 0f;

        if(weaponHp >= weaponMaxHp)
        {
            weaponHp = 0;
        }

        // Check Coal
        var needCoal = currentWeaponInfo.Coal;
        
        if(useCoal == false)
        {
            if (needCoal > Managers.Player.GetCurrency(Define.ECurrency.Coal))
            {
                // TODO 석탄 부족 표시
                return (float)weaponHp / weaponMaxHp;
            }

            UseCoal().Forget();
        }

        weaponHp += Managers.Player.GetPlayerStat(Define.EPlayerStat.Str);

        if(weaponHp >= weaponMaxHp)
        {
            weaponHp = weaponMaxHp;
        }

        //Debug.Log(weaponHp);

        return (float)weaponHp / weaponMaxHp;
    }

    public bool StartWeaponMake(string weaponName)
    {
        var weaponInfo = Managers.Data.WeaponDict[weaponName];

        if (weaponInfo == null)
            return false;

        if (weaponInfo.Iron > Managers.Player.GetCurrency(Define.ECurrency.Iron))
            return false;


        Managers.Player.CurrencySubtract(Define.ECurrency.Iron, weaponInfo.Iron);

        isWork = true;
        currentWeaponInfo = weaponInfo;

        coalCTS?.Cancel();
        coalCTS?.Dispose();

        useCoal = false;

        return true;
    }

    public async UniTaskVoid UseCoal()
    {
        // 이전 작업이 있다면 취소
        coalCTS?.Cancel();
        coalCTS?.Dispose();

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


    #endregion
}