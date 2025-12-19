using System;
using System.Collections.Generic;
using Unity.Services.LevelPlay;
using UnityEngine;


public class PlayerManager
{
    private long[] currency = new long[Enum.GetValues(typeof(Define.ECurrency)).Length];
    private int[] playerStatLevel = new int[Enum.GetValues(typeof(Define.EPlayerStat)).Length];
    private int[] forgeStatLevel = new int[Enum.GetValues(typeof(Define.EPlayerForgeStat)).Length];
    private int[] townStatLevel = new int[Enum.GetValues(typeof(Define.EPlayerTownStat)).Length];
    private List<int> shopProducts = new List<int>();
    private List<int> ownedWeapons = new List<int>();


    public void Clear()
    {
        Array.Clear(currency, 0, currency.Length);
        Array.Clear(playerStatLevel, 0, playerStatLevel.Length);
        Array.Clear(forgeStatLevel, 0, forgeStatLevel.Length);
        Array.Clear(townStatLevel, 0, townStatLevel.Length);
        shopProducts.Clear();
        Managers.Ad.OnRewardedEarned -= RewardAdEarned;
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

            {
                // shopProducts[(int)Define.EShopProductType.BuyIron] = 1;
                // shopProducts[(int)Define.EShopProductType.BuyCoal] = 11;
                foreach (var shopData in Managers.Data.ShopProductDict.Values)
                {
                    Debug.Log(shopData.TemplateId);
                    shopProducts.Add(shopData.TemplateId);
                }
            }


            ownedWeapons.Add(1);
            
            OnPlayerSave?.Invoke();
        }

        Managers.Ad.OnRewardedEarned -= RewardAdEarned;
        Managers.Ad.OnRewardedEarned += RewardAdEarned;
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
    public List<int> GetShopAllStat() => shopProducts;
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
        int templateId = 0;
        // Shop의 경우 TemplateId를 전달 받음
        if(upgradeType == Define.EUpgradeType.Shop)
        {
            templateId = slotIndex;
        }
        else
        {
            templateId = GetTemplateIdFor(upgradeType, slotIndex);
        }

        var data = GetUpgradeDataFor(upgradeType, templateId);

        if (data == null)
            return;

        // Shop이 아닌 경우 다음 레벨 가능 여부 체크
        if (upgradeType != Define.EUpgradeType.Shop && data.NextTempalteId == 0)
            return;

        // Upgrade하기
        if (upgradeType == Define.EUpgradeType.Shop)
        {
            // Shop인 경우 tempalteId를 활용하여 업데이트
            ProcessShopPurchase(templateId);
        }
        else
        {
            // Gold 체크
            if (data.Price > GetCurrency(Define.ECurrency.Gold))
            {
                ShowLessMessage(Define.ECurrency.Gold);
                return;
            }

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

    private void ProcessShopPurchase(int templateId)
    {
        int shopTemplateId = templateId;
        if (!Managers.Data.ShopProductDict.TryGetValue(shopTemplateId, out var dataValue))
            return;

        // 어떤 자원인지 판별
        Define.ECurrency currencyType = dataValue.StatType switch
        {
            Define.EShopProductType.BuyIron => Define.ECurrency.Iron,
            Define.EShopProductType.BuyCoal => Define.ECurrency.Coal,
            Define.EShopProductType.BuyGold => Define.ECurrency.Gold,
            _ => Define.ECurrency.None,
        };

        if (currencyType == Define.ECurrency.None)
            return;

        // 최대값 체크
        var stock = GetCurrency(currencyType);
        var maxStock = GetCurrenyMax(currencyType); // 없으면 long 최대값

        if (stock >= maxStock)
        {
            ShowFullMessage(currencyType);
            return;
        }

        Debug.Log($"get currencyType {currencyType}");

        // 기존 클래스를 그대로 쓰기 때문에 CurrentValue는 몇 % 획득인지를 의미함
        long addValue = maxStock * dataValue.CurrentValue / 100;

        // 구매 타입에 따른 분류
        if (dataValue.BuyType == Define.EShopBuyType.Gold)
        {
            // 골드 지불 계산 기존 클래스를 그대로 쓰기 때문에 Price는 1개다 가격을 의미함
            long price = addValue * dataValue.Price;

            if(price > Managers.Player.GetCurrency(Define.ECurrency.Gold))
            {
                ShowLessMessage(Define.ECurrency.Gold);
                return;
            }

            CurrencySubtract(Define.ECurrency.Gold, price);
            Managers.Sound.Play(Define.ESound.Effect, "UpgradeEffect");

            long bonusPercent = GetTownStat(Define.EPlayerTownStat.ShopBuyBonus);
            long bonusValue = (long)Mathf.Floor(addValue * (bonusPercent / 1000f));

            CurrencyAdd(currencyType, addValue + bonusValue);
        }
        else if (dataValue.BuyType == Define.EShopBuyType.Ad)
        {
            Managers.Ad.ShowRewardAd(currencyType);
        }
    }

    private void RewardAdEarned(Define.ECurrency currencyType, LevelPlayAdInfo info, LevelPlayReward reward)
    {
        // 광고 시청 보류 중인 상품 정보 찾기 (여기서는 통화별로 하나의 광고 상품만 있다고 가정)
        Data.ShopProductData dataValue = null;
        foreach (var productId in shopProducts)
        {
            if (Managers.Data.ShopProductDict.TryGetValue(productId, out var product))
            {
                if (product.BuyType == Define.EShopBuyType.Ad)
                {
                    Define.ECurrency pCurrency = product.StatType switch
                    {
                        Define.EShopProductType.BuyIron => Define.ECurrency.Iron,
                        Define.EShopProductType.BuyCoal => Define.ECurrency.Coal,
                        Define.EShopProductType.BuyGold => Define.ECurrency.Gold,
                        _ => Define.ECurrency.None,
                    };

                    if (pCurrency == currencyType)
                    {
                        dataValue = product;
                        break;
                    }
                }
            }
        }

        if (dataValue == null)
            return;

        long maxStock = GetCurrenyMax(currencyType);
        long addValue = maxStock * reward.Amount / 100;
        

        Managers.Sound.Play(Define.ESound.Effect, "UpgradeEffect");
        long bonusPercent = GetTownStat(Define.EPlayerTownStat.ShopBuyBonus);
        long bonusValue = (long)Mathf.Floor(addValue * (bonusPercent / 1000f));

        Debug.Log($"Currency : {currencyType} Value : {addValue + bonusValue}");

        CurrencyAdd(currencyType, addValue + bonusValue);
        
        OnPlayerUpgradeChanged?.Invoke();
        OnPlayerSave?.Invoke();
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
        data.shopProducts = new List<int>(shopProducts);
        data.ownedWeapons = new List<int>(ownedWeapons);
        return data;
    }

    public void RestoreFromSaveData(SaveData data)
    {
        currency = (long[])data.currency.Clone();
        playerStatLevel = (int[])data.playerStatLevel.Clone();
        forgeStatLevel = (int[])data.forgeStatLevel.Clone();
        townStatLevel = (int[])data.townStatLevel.Clone();
        shopProducts = new List<int>(data.shopProducts);
        ownedWeapons = new List<int>(data.ownedWeapons);

        OnCurrenciesChanged?.Invoke();
        OnPlayerUpgradeChanged?.Invoke();
    }
    #endregion
}
