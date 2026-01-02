using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.LevelPlay;
using UnityEngine;


public class PlayerManager
{
    private long[] currency = new long[Enum.GetValues(typeof(Define.ECurrency)).Length];
    private long[] playerStatLevel = new long[Enum.GetValues(typeof(Define.EPlayerStat)).Length];
    private long[] forgeStatLevel = new long[Enum.GetValues(typeof(Define.EPlayerForgeStat)).Length];
    private long[] townStatLevel = new long[Enum.GetValues(typeof(Define.EPlayerTownStat)).Length];
    private List<long> shopProducts = new List<long>();
    private List<int> ownedWeapons = new List<int>();
    private int _clearedWeaponCount;
    private Define.ELanguage _language;
    private int _stage = 1;


    
    public Define.ELanguage language
    {
        get => _language;
        private set
        {
            if(_language != value)
            {
                _language = value;
                Managers.Language = _language;
                OnPlayerSave?.Invoke();
                OnLanguageChange?.Invoke();
            }
        }
        
    }

    public int ClearedWeaponCount
    {
        get => _clearedWeaponCount;
        private set
        {
            if (_clearedWeaponCount != value)
            {
                _clearedWeaponCount = value;
                OnPlayerSave?.Invoke();
            }
        }
    }

    
    public int Stage
    {
        get => _stage;
        private set
        {
            if (_stage != value)
            {
                _stage = value;
                OnPlayerSave?.Invoke();
            }
        }
    }

    public void SetStage(int stage)
    {
        if (stage < 1)
        {
            Debug.LogError($"[PlayerManager] Invalid Stage set: {stage}. Stage must be at least 1.");
        }
        Stage = stage;
    }

    public void SetClearedWeaponCount(int count)
    {
        ClearedWeaponCount = count;
    }

    public void ChangeLanguage()
    {
        language = language == Define.ELanguage.Korean ? Define.ELanguage.English : Define.ELanguage.Korean;
    }


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
            InitByStage(1);
            language = Managers.Language;
            OnPlayerSave?.Invoke();
        }

        Managers.Ad.OnRewardedEarned -= RewardAdEarned;
        Managers.Ad.OnRewardedEarned += RewardAdEarned;
    }

    public void InitByStage(int stageNum)
    {
        SetStage(stageNum);

        // 기본 자원 초기화
        currency[(int)Define.ECurrency.Gold] = 0;
        currency[(int)Define.ECurrency.Iron] = 5000;
        currency[(int)Define.ECurrency.Coal] = 5000;

        // Player Stats 초기화 (OriginalTemplateId가 자신인 것이 1레벨)
        foreach (Define.EPlayerStat stat in Enum.GetValues(typeof(Define.EPlayerStat)))
        {
            var data = Managers.Data.PlayerUpgradeDict.Values
                .FirstOrDefault(d => d.Stage == stageNum && d.StatType == stat && d.TemplateId == d.OriginalTemplateId);
            
            playerStatLevel[(int)stat] = (data != null) ? data.TemplateId : 0;
        }

        // Forge Stats 초기화
        foreach (Define.EPlayerForgeStat stat in Enum.GetValues(typeof(Define.EPlayerForgeStat)))
        {
            var data = Managers.Data.ForgeUpgradeDict.Values
                .FirstOrDefault(d => d.Stage == stageNum && d.StatType == stat && d.TemplateId == d.OriginalTemplateId);
            
            forgeStatLevel[(int)stat] = (data != null) ? data.TemplateId : 0;
        }

        // Town Stats 초기화
        foreach (Define.EPlayerTownStat stat in Enum.GetValues(typeof(Define.EPlayerTownStat)))
        {
            var data = Managers.Data.TownUpgradeDict.Values
                .FirstOrDefault(d => d.Stage == stageNum && d.StatType == stat && d.TemplateId == d.OriginalTemplateId);
            
            townStatLevel[(int)stat] = (data != null) ? data.TemplateId : 0;
        }

        // Shop Products 초기화 (해당 스테이지의 상품만 등록)
        shopProducts.Clear();
        foreach (var shopData in Managers.Data.ShopProductDict.Values)
        {
            if (shopData.Stage == stageNum)
                shopProducts.Add(shopData.TemplateId);
        }

        // 무기 초기화 (해당 스테이지의 첫 번째 무기)
        ownedWeapons.Clear();
        if (Managers.Data.WeaponDict.TryGetValue(stageNum, out var stageWeapons))
        {
            var firstWeapon = stageWeapons.Values
                .OrderBy(w => w.WeaponNumber)
                .FirstOrDefault();

            if (firstWeapon != null)
                ownedWeapons.Add(firstWeapon.WeaponNumber);
        }

        SetClearedWeaponCount(0);
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
        var templateId = townStatLevel[(int)stat];
        if (Managers.Data.TownUpgradeDict.TryGetValue(templateId, out var data))
            return data.CurrentValue;

        return long.MaxValue;
    }


    public long GetPlayerStat(Define.EPlayerStat type)
    {
        var templateId = playerStatLevel[(int)type];

        if (Managers.Data.PlayerUpgradeDict.TryGetValue(templateId, out var d))
            return d.CurrentValue;

        return 0;
    }

    public long GetForgeStat(Define.EPlayerForgeStat type)
    {
        var templateId = forgeStatLevel[(int)type];

        if (Managers.Data.ForgeUpgradeDict.TryGetValue(templateId, out var d))
            return d.CurrentValue;

        return 0;
    }

    public long GetTownStat(Define.EPlayerTownStat type)
    {
        var templateId = townStatLevel[(int)type];

        if (Managers.Data.TownUpgradeDict.TryGetValue(templateId, out var d))
            return d.CurrentValue;

        return 0;
    }

    public long[] GetPlayerAllStat() => playerStatLevel;
    public long[] GetForgeAllStat() => forgeStatLevel;
    public long[] GetTownAllStat() => townStatLevel;
    public List<long> GetShopAllStat() => shopProducts;
    public List<int> GetOwnedWeapons() => ownedWeapons;

    public void AddOwnedWeapon(int weaponNumber)
    {
        if (!ownedWeapons.Contains(weaponNumber))
        {
            ownedWeapons.Add(weaponNumber);
            OnPlayerSave?.Invoke();
        }
    }

    public bool HasWeapon(int weaponNumber) => ownedWeapons.Contains(weaponNumber);

    #region UpgradeHelper
    private long GetTemplateIdFor(Define.EUpgradeType upgradeType, int type)
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

    private Data.UpgradeData GetUpgradeDataFor(Define.EUpgradeType upgradeType, long templateId)
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
        long templateId = GetTemplateIdFor(upgradeType, slotIndex);;

        var data = GetUpgradeDataFor(upgradeType, templateId);

        if (data == null)
            return;

        // 다음 레벨 가능 여부 체크
        if (data.NextTempalteId == 0)
            return;

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


        OnPlayerUpgradeChanged?.Invoke();
        OnPlayerSave?.Invoke();
    }

    private void ApplyUpgradeLevel(Define.EUpgradeType upgradeType, int slotIndex, long nextTemplateId)
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

    public void ProcessShopPurchase(long templateId)
    {
        long shopTemplateId = templateId;
        if (!Managers.Data.ShopProductDict.TryGetValue(shopTemplateId, out var dataValue))
            return;

        if(dataValue.Stage != Stage)
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
            Define.ECurrency.Gold => Managers.GetText("NotEnoughGold"),
            Define.ECurrency.Iron => Managers.GetText("NotEnoughMaterial"),
            Define.ECurrency.Coal => Managers.GetText("NotEnoughFuel"),
            _ => Managers.GetText("NotEnoughResources")
        };

        Managers.UI.ShowToast(message, 1, Define.EToastColor.Red, Define.EToastPosition.MiddleCenter);
    }

    private void ShowFullMessage(Define.ECurrency currency)
    {
        string message = currency switch
        {
            Define.ECurrency.Gold => Managers.GetText("GoldMax"),
            Define.ECurrency.Iron => Managers.GetText("MaterialMax"),
            Define.ECurrency.Coal => Managers.GetText("CoalMax"),
            _ => Managers.GetText("ResourcesMax")
        };

        Managers.UI.ShowToast(message, 1, Define.EToastColor.Orange, Define.EToastPosition.MiddleCenter);
    }
    #endregion

    #region Action
    public event Action OnCurrenciesChanged;
    public event Action OnPlayerUpgradeChanged;
    public event Action OnPlayerSave;
    public event Action OnLanguageChange;
    #endregion

    #region Save&Load
    public SaveData GetSaveData()
    {
        SaveData data = new SaveData();
        data.currency = (long[])currency.Clone();
        data.playerStatLevel = (long[])playerStatLevel.Clone();
        data.forgeStatLevel = (long[])forgeStatLevel.Clone();
        data.townStatLevel = (long[])townStatLevel.Clone();
        data.shopProducts = new List<long>(shopProducts);
        data.ownedWeapons = new List<int>(ownedWeapons);
        data.clearedWeaponCount = ClearedWeaponCount;
        data.stage = Stage;
        data.language = language;
        return data;
    }

    public void RestoreFromSaveData(SaveData data)
    {
        if (data.stage < 1)
        {
            Debug.LogError($"[PlayerManager] RestoreFromSaveData failed: Invalid stage {data.stage} in save data.");
            return;
        }

        language = data.language;
        Managers.Language = language;

        int savedStage = data.stage;

        // 1. 해당 스테이지의 기본값으로 전체 초기화 (Define 추가에 대비)
        InitByStage(savedStage);

        // 2. 통화 복사 (있는 데이터만)
        if (data.currency != null)
        {
            for (int i = 0; i < currency.Length && i < data.currency.Length; i++)
                currency[i] = data.currency[i];
        }

        // 3. 업그레이드 레벨 복사 (있는 데이터만 적용, 데이터 유효성 검사 수행)
        if (data.playerStatLevel != null)
        {
            for (int i = 0; i < playerStatLevel.Length && i < data.playerStatLevel.Length; i++)
            {
                long savedId = data.playerStatLevel[i];
                if (Managers.Data.PlayerUpgradeDict.ContainsKey(savedId))
                    playerStatLevel[i] = savedId;
            }
        }

        if (data.forgeStatLevel != null)
        {
            for (int i = 0; i < forgeStatLevel.Length && i < data.forgeStatLevel.Length; i++)
            {
                long savedId = data.forgeStatLevel[i];
                if (Managers.Data.ForgeUpgradeDict.ContainsKey(savedId))
                    forgeStatLevel[i] = savedId;
            }
        }

        if (data.townStatLevel != null)
        {
            for (int i = 0; i < townStatLevel.Length && i < data.townStatLevel.Length; i++)
            {
                long savedId = data.townStatLevel[i];
                if (Managers.Data.TownUpgradeDict.ContainsKey(savedId))
                    townStatLevel[i] = savedId;
            }
        }

        // 4. 리스트 복구 (유효한 데이터만 필터링)
        shopProducts.Clear();
        if (data.shopProducts != null)
        {
            foreach (var id in data.shopProducts)
            {
                if (Managers.Data.ShopProductDict.TryGetValue(id, out var shopData))
                {
                    // 해당 스테이지 상품이 맞는 경우에만 복구
                    if (shopData.Stage == savedStage)
                        shopProducts.Add(id);
                }
            }
        }

        ownedWeapons.Clear();
        if (data.ownedWeapons != null)
        {
            if (Managers.Data.WeaponDict.TryGetValue(savedStage, out var stageWeapons))
            {
                foreach (var weaponNum in data.ownedWeapons)
                {
                    if (stageWeapons.ContainsKey(weaponNum))
                        ownedWeapons.Add(weaponNum);
                }
            }
            
            // 만약 유효한 무기가 하나도 남지 않았다면 첫 무기 강제 추가 (InitByStage에서 이미 처리됨)
            if (ownedWeapons.Count == 0 && Managers.Data.WeaponDict.TryGetValue(savedStage, out var sw))
            {
                var first = sw.Values.OrderBy(w => w.WeaponNumber).FirstOrDefault();
                if (first != null) ownedWeapons.Add(first.WeaponNumber);
            }
        }

        SetClearedWeaponCount(data.clearedWeaponCount);

        OnCurrenciesChanged?.Invoke();
        OnPlayerUpgradeChanged?.Invoke();
    }
    #endregion
}
