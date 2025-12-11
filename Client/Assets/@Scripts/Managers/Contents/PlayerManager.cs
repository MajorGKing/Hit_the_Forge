using System;
using System.Collections.Generic;
using UnityEngine;


public class PlayerManager
{
    private long[] currency = new long[Enum.GetValues(typeof(Define.ECurrency)).Length];
    private int[] playerStatLevel = new int[Enum.GetValues(typeof(Define.EPlayerStat)).Length];
    private int[] forgeStatLevel = new int[Enum.GetValues(typeof(Define.EPlayerForgeStat)).Length];
    private int[] townStatLevel = new int[Enum.GetValues(typeof(Define.EPlayerTownStat)).Length];
    private int[] shopProducts = new int[Enum.GetValues(typeof(Define.EShopProductType)).Length];
    private List<int> ownedWeapons = new List<int>();


    public void Clear()
    {
        Array.Clear(currency, 0, currency.Length);
        Array.Clear(playerStatLevel, 0, playerStatLevel.Length);
        Array.Clear(forgeStatLevel, 0, forgeStatLevel.Length);
        Array.Clear(townStatLevel, 0, townStatLevel.Length);
        Array.Clear(shopProducts, 0, shopProducts.Length);
    }

    public void Init()
    {
        if (false == Managers.Save.LoadGame())
        {

            currency[(int)Define.ECurrency.Gold] = 0;
            currency[(int)Define.ECurrency.Iron] = 5000;
            currency[(int)Define.ECurrency.Coal] = 5000;

            playerStatLevel[(int)Define.EPlayerStat.Str] = 100001;
            playerStatLevel[(int)Define.EPlayerStat.Skill] = 200001;
            playerStatLevel[(int)Define.EPlayerStat.Dex] = 0;
            playerStatLevel[(int)Define.EPlayerStat.Mastery] = 300001;

            forgeStatLevel[(int)Define.EPlayerForgeStat.CoalTime] = 100001;
            forgeStatLevel[(int)Define.EPlayerForgeStat.Skill] = 200001;
            forgeStatLevel[(int)Define.EPlayerForgeStat.Mastery] = 300001;

            townStatLevel[(int)Define.EPlayerTownStat.GoldMax] = 100001;
            townStatLevel[(int)Define.EPlayerTownStat.IronMax] = 200001;
            townStatLevel[(int)Define.EPlayerTownStat.IronRegeneration] = 300001;
            townStatLevel[(int)Define.EPlayerTownStat.CoalMax] = 400001;
            townStatLevel[(int)Define.EPlayerTownStat.CoalRegeneration] = 500001;
            townStatLevel[(int)Define.EPlayerTownStat.ShopSellBonus] = 600001;
            townStatLevel[(int)Define.EPlayerTownStat.ShopBuyBonus] = 700001;

            shopProducts[(int)Define.EShopProductType.BuyIron] = 1;
            shopProducts[(int)Define.EShopProductType.BuyCoal] = 11;

            ownedWeapons.Add(1);

            OnPlayerSave?.Invoke();
        }
    }

    public void SetCurrency(Define.ECurrency type, long value)
    {
        int index = (int)type;

        // 음수는 최소 0
        if (value < 0)
            value = 0;

        long max = GetCurrenyMax(type);
        long newValue = Math.Clamp(value, 0, max);

        if (currency[index] != newValue)
        {
            currency[index] = newValue;
            OnCurrenciesChanged?.Invoke();
        }

        OnPlayerSave?.Invoke();
    }

    private void ChangeCurrency(Define.ECurrency type, long value)
    {
        if (value == 0)
            return;

        int index = (int)type;
        long current = currency[index];

        SetCurrency(type, current + value);
    }

    public void CurrencyAdd(Define.ECurrency type, long value)
    {
        if (value <= 0)
            return;

        ChangeCurrency(type, value);
    }

    public void CurrencySubtract(Define.ECurrency type, long value)
    {
        if (value <= 0)
            return;

        ChangeCurrency(type, -value);
    }

    public long GetCurrency(Define.ECurrency type)
    {
        return currency[(int)type];
    }

    public long GetCurrenyMax(Define.ECurrency type)
    {
        return type switch
        {
            Define.ECurrency.Gold => GetCurrencyMaxFromTowndata(Define.EPlayerTownStat.GoldMax),
            Define.ECurrency.Iron => GetCurrencyMaxFromTowndata(Define.EPlayerTownStat.IronMax),
            Define.ECurrency.Coal => GetCurrencyMaxFromTowndata(Define.EPlayerTownStat.CoalMax),
            _ => long.MaxValue
        };
    }

    private long GetCurrencyMaxFromTowndata(Define.EPlayerTownStat stat)
    {
        int templateId = townStatLevel[(int)stat];
        if (Managers.Data.TownUpgradeDict.TryGetValue(templateId, out var data))
            return data.CurrentValue;

        return long.MaxValue;
    }


    public long GetPlayerStat(Define.EPlayerStat type)
    {
        int templateId = playerStatLevel[(int)type];

        if (Managers.Data.PlayerUpgradeDict.TryGetValue(templateId, out var d))
            return d.CurrentValue;

        return 0;
    }

    public long GetForgeStat(Define.EPlayerForgeStat type)
    {
        int templateId = forgeStatLevel[(int)type];

        if (Managers.Data.ForgeUpgradeDict.TryGetValue(templateId, out var d))
            return d.CurrentValue;

        return 0;
    }

    public long GetTownStat(Define.EPlayerTownStat type)
    {
        int templateId = townStatLevel[(int)type];

        if (Managers.Data.TownUpgradeDict.TryGetValue(templateId, out var d))
            return d.CurrentValue;

        return 0;
    }

    public int[] GetPlayerAllStat() => playerStatLevel;
    public int[] GetForgeAllStat() => forgeStatLevel;
    public int[] GetTownAllStat() => townStatLevel;
    public int[] GetShopAllStat() => shopProducts;

    public List<int> GetOwnedWeapons() => ownedWeapons;

    public void AddOwnedWeapon(int templateId)
    {
        if (!ownedWeapons.Contains(templateId))
        {
            ownedWeapons.Add(templateId);
            OnPlayerSave?.Invoke();
        }
    }

    public bool HasWeapon(int templateId) => ownedWeapons.Contains(templateId);

    #region UpgradeHelper
    private int GetTemplateIdFor(Define.EUpgradeType upgradeType, int type)
    {
        switch (upgradeType)
        {
            case Define.EUpgradeType.Player:
                return playerStatLevel[type];
            case Define.EUpgradeType.Forge:
                return forgeStatLevel[type];
            case Define.EUpgradeType.Town:
                return townStatLevel[type];
            case Define.EUpgradeType.Shop:
                return shopProducts[type];
            default:
                return 0;
        }
    }

    private Data.UpgradeData GetUpgradeDataFor(Define.EUpgradeType upgradeType, int templateId)
    {
        if (templateId <= 0) return null;

        switch (upgradeType)
        {
            case Define.EUpgradeType.Player:
                Managers.Data.PlayerUpgradeDict.TryGetValue(templateId, out var p);
                return p;
            case Define.EUpgradeType.Forge:
                Managers.Data.ForgeUpgradeDict.TryGetValue(templateId, out var f);
                return f;
            case Define.EUpgradeType.Town:
                Managers.Data.TownUpgradeDict.TryGetValue(templateId, out var t);
                return t;
            case Define.EUpgradeType.Shop:
                Managers.Data.ShopProductDict.TryGetValue(templateId, out var s);
                return s;
            default:
                return null;
        }
    }
    #endregion

    public void StatUpgrade(Define.EUpgradeType upgradeType, int slotIndex)
    {
        int templateId = GetTemplateIdFor(upgradeType, slotIndex);
        var data = GetUpgradeDataFor(upgradeType, templateId);

        if (data == null)
            return;

        // Gold 체크
        if (data.Price > GetCurrency(Define.ECurrency.Gold))
        {
            ShowLessMessage(Define.ECurrency.Gold);
            return;
        }

        // Shop이 아닌 경우 다음 레벨 가능 여부 체크
        if (upgradeType != Define.EUpgradeType.Shop && data.NextTempalteId == 0)
            return;

        // Upgrade하기
        if (upgradeType == Define.EUpgradeType.Shop)
        {
            ProcessShopPurchase(slotIndex);
        }
        else
        {
            // Gold 차감
            CurrencySubtract(Define.ECurrency.Gold, data.Price);

            // 공통 업그레이드 처리
            ApplyUpgradeLevel(upgradeType, slotIndex, data.NextTempalteId);

            Managers.Sound.Play(Define.ESound.Effect, "UpgradeEffect");
        }

        OnPlayerUpgradeChanged?.Invoke();
        OnPlayerSave?.Invoke();
    }

    private void ApplyUpgradeLevel(Define.EUpgradeType upgradeType, int slotIndex, int nextTemplateId)
    {
        switch (upgradeType)
        {
            case Define.EUpgradeType.Player:
                playerStatLevel[slotIndex] = nextTemplateId;
                break;

            case Define.EUpgradeType.Forge:
                forgeStatLevel[slotIndex] = nextTemplateId;
                break;

            case Define.EUpgradeType.Town:
                townStatLevel[slotIndex] = nextTemplateId;
                break;
        }
    }

    private void ProcessShopPurchase(int shopSlotIndex)
    {
        int shopTemplateId = shopProducts[shopSlotIndex];
        if (!Managers.Data.ShopProductDict.TryGetValue(shopTemplateId, out var dataValue))
            return;

        // 어떤 자원인지 판별
        Define.ECurrency currencyType = dataValue.StatType switch
        {
            Define.EShopProductType.BuyIron => Define.ECurrency.Iron,
            Define.EShopProductType.BuyCoal => Define.ECurrency.Coal,
            // TODO ILHAK 골드 추후에 구현 필요
            _ => Define.ECurrency.None,
        };

        if (currencyType == Define.ECurrency.None)
            return;

        // 최대값 체크
        var stock = GetCurrency(currencyType);
        var maxStock = GetCurrenyMax(currencyType); // 없으면 int 최대값

        if (stock >= maxStock)
        {
            ShowFullMessage(currencyType);
            return;
        }

        // 골드 지불
        CurrencySubtract(Define.ECurrency.Gold, dataValue.Price);
        Managers.Sound.Play(Define.ESound.Effect, "UpgradeEffect");

        long addValue = dataValue.CurrentValue;
        long bonusPercent = GetTownStat(Define.EPlayerTownStat.ShopBuyBonus);
        long bonusValue = (long)Mathf.Floor(addValue * (bonusPercent / 1000f));

        ChangeCurrency(currencyType, addValue + bonusValue);
    }

    #region  ShowMessage
    private void ShowLessMessage(Define.ECurrency currency)
    {
        string message = currency switch
        {
            Define.ECurrency.Gold => "골드가 부족합니다.",
            Define.ECurrency.Iron => "재료가 부족합니다.",
            Define.ECurrency.Coal => "연료가 부족합니다.",
            _ => "자원이 부족합니다."
        };

        Managers.UI.ShowToast(message, 1, Define.EToastColor.Red, Define.EToastPosition.MiddleCenter);
    }

    private void ShowFullMessage(Define.ECurrency currency)
    {
        string message = currency switch
        {
            Define.ECurrency.Gold => "골드가 최대치입니다.",
            Define.ECurrency.Iron => "재료가 최대치입니다.",
            Define.ECurrency.Coal => "연료가 최대치입니다.",
            _ => "자원이 가득합니다."
        };

        Managers.UI.ShowToast(message, 1, Define.EToastColor.Orange, Define.EToastPosition.MiddleCenter);
    }
    #endregion

    #region Action
    public event Action OnCurrenciesChanged;
    public event Action OnPlayerUpgradeChanged;
    public event Action OnPlayerSave;
    #endregion

    #region Save&Load
    public SaveData GetSaveData()
    {
        SaveData data = new SaveData();
        data.currency = (long[])currency.Clone();
        data.playerStatLevel = (int[])playerStatLevel.Clone();
        data.forgeStatLevel = (int[])forgeStatLevel.Clone();
        data.townStatLevel = (int[])townStatLevel.Clone();
        data.shopProducts = (int[])shopProducts.Clone();
        data.ownedWeapons = new List<int>(ownedWeapons);
        return data;
    }

    public void RestoreFromSaveData(SaveData data)
    {
        currency = (long[])data.currency.Clone();
        playerStatLevel = (int[])data.playerStatLevel.Clone();
        forgeStatLevel = (int[])data.forgeStatLevel.Clone();
        townStatLevel = (int[])data.townStatLevel.Clone();
        shopProducts = (int[])data.shopProducts.Clone();
        ownedWeapons = new List<int>(data.ownedWeapons);

        OnCurrenciesChanged?.Invoke();
        OnPlayerUpgradeChanged?.Invoke();
    }
    #endregion
}
