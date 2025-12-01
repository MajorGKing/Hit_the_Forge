using System;
using System.Diagnostics;


public class PlayerManager
{
    private int[] currency = new int[Enum.GetValues(typeof(Define.ECurrency)).Length];
    //public int[] maxCurrency = new int[Enum.GetValues(typeof(Define.ECurrency)).Length];
    //private int[] playerStat = new int[Enum.GetValues(typeof(Define.EPlayerStat)).Length];
    private int[] playerStatLevel = new int[Enum.GetValues(typeof(Define.EPlayerStat)).Length];
    private int[] forgeStatLevel = new int[Enum.GetValues(typeof(Define.EPlayerForgeStat)).Length];
    private int[] townStatLevel = new int[Enum.GetValues(typeof(Define.EPlayerTownStat)).Length];
    private int[] shopProducts = new int[Enum.GetValues(typeof(Define.EShopProductType)).Length];
    

    public void Clear()
    {

    }

    public void Init()
    {
        //for (int i = 0; i < maxCurrency.Length; i++)
        //{
        //    maxCurrency[i] = 10000;
        //}

        currency[(int)Define.ECurrency.Gold] = 0;
        currency[(int)Define.ECurrency.Iron] = 5000;
        currency[(int)Define.ECurrency.Coal] = 5000;

        //playerStat[(int)Define.EPlayerStat.Str] = 10;
        //playerStat[(int)Define.EPlayerStat.Skill] = 10;
        //playerStat[(int)Define.EPlayerStat.Dex] = 10;
        //playerStat[(int)Define.EPlayerStat.Mastery] = 10;

        playerStatLevel[(int)Define.EPlayerStat.Str] = 1;
        playerStatLevel[(int)Define.EPlayerStat.Skill] = 101;
        playerStatLevel[(int)Define.EPlayerStat.Dex] = 0;
        playerStatLevel[(int)Define.EPlayerStat.Mastery] = 201;

        forgeStatLevel[(int)Define.EPlayerForgeStat.CoalTime] = 1;
        forgeStatLevel[(int)Define.EPlayerForgeStat.Skill] = 101;
        forgeStatLevel[(int)Define.EPlayerForgeStat.Mastery] = 201;

        townStatLevel[(int)Define.EPlayerTownStat.GoldMax] = 1;
        townStatLevel[(int)Define.EPlayerTownStat.IronMax] = 101;
        townStatLevel[(int)Define.EPlayerTownStat.IronRegeneration] = 201;
        townStatLevel[(int)Define.EPlayerTownStat.CoalMax] = 301;
        townStatLevel[(int)Define.EPlayerTownStat.CoalRegeneration] = 401;
        townStatLevel[(int)Define.EPlayerTownStat.ShopSellBonus] = 501;
        townStatLevel[(int)Define.EPlayerTownStat.ShopBuyBonus] = 601;

        shopProducts[(int)Define.EShopProductType.BuyIron] = 1;
        shopProducts[(int)Define.EShopProductType.BuyCoal] = 11;
    }

    public void CurrencyAdd(Define.ECurrency type, int value)
    {
        if (value <= 0)
            return;
        int index = (int)type;

        int oldValue = currency[index];
        int newValue = oldValue + value;

        if (newValue > GetCurrenyMax(type))
        {
            newValue = GetCurrenyMax(type);
        }

        currency[index] = newValue;

        if (oldValue != newValue)
        {
            OnCurrenciesChagned?.Invoke();
        }
    }

    public void CurrencySubtract(Define.ECurrency type, int value)
    {
        if (value <= 0)
            return;

        int index = (int)type;

        int oldValue = currency[index];
        int newValue = oldValue - value;

        if (newValue < 0)
        {
            newValue = 0;
        }

        currency[index] = newValue;

        if (oldValue != newValue)
        {
            OnCurrenciesChagned?.Invoke();
        }
    }

    public int GetCurrency(Define.ECurrency type)
    {
        return currency[(int)type];
    }

    public int GetCurrenyMax(Define.ECurrency type)
    {
        int index = -1;

        if(type == Define.ECurrency.Gold)
        {
            index = townStatLevel[(int)Define.EPlayerTownStat.GoldMax];
        }
        else if (type == Define.ECurrency.Iron)
        {
            index = townStatLevel[(int)Define.EPlayerTownStat.IronMax];
        }
        else if (type == Define.ECurrency.Coal)
        {
            index = townStatLevel[(int)Define.EPlayerTownStat.CoalMax];
        }

        if (index != -1)
            return Managers.Data.TownUpgradeDict[index].CurrentValue;

        return 0;
    }

    public int GetPlayerStat(Define.EPlayerStat type)
    {
        var statData = Managers.Data.PlayerUpgradeDict[playerStatLevel[(int)type]];

        return statData.CurrentValue;
    }

    public int GetForgeStat(Define.EPlayerForgeStat type)
    {
        var statData = Managers.Data.ForgeUpgradeDict[forgeStatLevel[(int)type]];

        return statData.CurrentValue;
    }

    public int GetTownStat(Define.EPlayerTownStat type)
    {
        var statData = Managers.Data.TownUpgradeDict[townStatLevel[(int)type]];

        return statData.CurrentValue;
    }

    public int[] GetPlayerAllStat()
    {
        return playerStatLevel;
    }

    public void StatUpgrade(Define.EUpgradeType upgradeType, int type)
    {
        Data.UpgradeData data = null;

        if (upgradeType == Define.EUpgradeType.Player)
        {
            Managers.Data.PlayerUpgradeDict.TryGetValue(playerStatLevel[type], out var dataValue);
            
            if (dataValue != null)
            {
                data = new Data.UpgradeData() { 
                    TemplateId = dataValue.TemplateId, 
                    UpgradeName = dataValue.UpgradeName,
                    //StatIndex = (int)dataValue.StatType,
                    Price = dataValue.Price,
                    CurrentValue = dataValue.CurrentValue,
                    NextValue = dataValue.NextValue,
                    OriginalTemplateId = dataValue.OriginalTemplateId,
                    NextTempalteId = dataValue.NextTempalteId,
                };
            }
        }
        else if(upgradeType == Define.EUpgradeType.Forge)
        {
            Managers.Data.ForgeUpgradeDict.TryGetValue(forgeStatLevel[type], out var dataValue);

            if (dataValue != null)
            {
                data = new Data.UpgradeData()
                {
                    TemplateId = dataValue.TemplateId,
                    UpgradeName = dataValue.UpgradeName,
                    //StatIndex = (int)dataValue.StatType,
                    Price = dataValue.Price,
                    CurrentValue = dataValue.CurrentValue,
                    NextValue = dataValue.NextValue,
                    OriginalTemplateId = dataValue.OriginalTemplateId,
                    NextTempalteId = dataValue.NextTempalteId,
                };
            }
        }
        else if (upgradeType == Define.EUpgradeType.Town)
        {
            Managers.Data.TownUpgradeDict.TryGetValue(townStatLevel[type], out var dataValue);

            if (dataValue != null)
            {
                data = new Data.UpgradeData()
                {
                    TemplateId = dataValue.TemplateId,
                    UpgradeName = dataValue.UpgradeName,
                    //StatIndex = (int)dataValue.StatType,
                    Price = dataValue.Price,
                    CurrentValue = dataValue.CurrentValue,
                    NextValue = dataValue.NextValue,
                    OriginalTemplateId = dataValue.OriginalTemplateId,
                    NextTempalteId = dataValue.NextTempalteId,
                };
            }
        }
        else if (upgradeType == Define.EUpgradeType.Shop)
        {
            Managers.Data.ShopProductDict.TryGetValue(shopProducts[type], out var dataValue);

            data = new Data.UpgradeData()
            {
                TemplateId = dataValue.TemplateId,
                UpgradeName = dataValue.UpgradeName,
                //StatIndex = (int)dataValue.StatType,
                Price = dataValue.Price,
                CurrentValue = dataValue.CurrentValue,
                NextValue = dataValue.NextValue,
                OriginalTemplateId = dataValue.OriginalTemplateId,
                NextTempalteId = dataValue.NextTempalteId,
            };

            // TODO Type을 None을 할 수 없어서 임시로
            Define.ECurrency currencyType = Define.ECurrency.Gold;

            // 가지고 있는 자원 종류 파악
            if(dataValue.StatType == Define.EShopProductType.BuyIron)
            {
                currencyType = Define.ECurrency.Iron;
            }
            else if(dataValue.StatType == Define.EShopProductType.BuyCoal)
            {
                currencyType = Define.ECurrency.Coal;
            }
            else
            {
                return;
            }

            // 최대 값 비교
            var stock = GetCurrency(currencyType);
            var maxStock = GetCurrenyMax(currencyType);

            if (stock == maxStock)
            {
                // TODO 최대치 관련 메세지
                return;
            }
        }


        if (data == null)
            return;

        // Gold 체크
        if(data.Price > GetCurrency(Define.ECurrency.Gold))
        {
            // TODO 골드 부족 알림
            return;
        }

        // 다음 레벨 가능 여부 체크
        if (upgradeType != Define.EUpgradeType.Shop && data.NextTempalteId == 0)
            return;

        // Gold 깍고 레벨업
        CurrencySubtract(Define.ECurrency.Gold, data.Price);
        // TODO 나중에 골드 아닌 방식 있으면 csv수정 필요

        if (upgradeType == Define.EUpgradeType.Player)
        {
            playerStatLevel[type] = data.NextTempalteId;
        }
        else if(upgradeType == Define.EUpgradeType.Forge)
        {
            forgeStatLevel[type] = data.NextTempalteId;
        }
        else if(upgradeType == Define.EUpgradeType.Town)
        {
            townStatLevel[type] = data.NextTempalteId;
        }
        else if(upgradeType == Define.EUpgradeType.Shop)
        {
            Managers.Data.ShopProductDict.TryGetValue(shopProducts[type], out var dataValue);

            // 가지고 있는 자원 종류 파악
            if (dataValue.StatType == Define.EShopProductType.BuyIron)
            {
                var addValue = dataValue.CurrentValue;
                var bonusValue = addValue * (GetTownStat(Define.EPlayerTownStat.ShopBuyBonus)/100f);
                CurrencyAdd(Define.ECurrency.Iron, addValue + (int)bonusValue);
            }
            else if (dataValue.StatType == Define.EShopProductType.BuyCoal)
            {
                var addValue = dataValue.CurrentValue;
                var bonusValue = addValue * (GetTownStat(Define.EPlayerTownStat.ShopBuyBonus) / 100f);
                CurrencyAdd(Define.ECurrency.Coal, dataValue.CurrentValue);
            }

            OnCurrenciesChagned?.Invoke();
        }

        // invoke
        OnPlayerUpgradeChanged?.Invoke();
    }

    public int[] GetForgeAllStat()
    {
        return forgeStatLevel;
    }

    public int[] GetTownAllStat()
    {
        return townStatLevel;
    }

    public int[] GetShopAllStat()
    {
        return shopProducts;
    }

    #region Action
    public event Action OnCurrenciesChagned;
    public event Action OnPlayerUpgradeChanged;
    #endregion
}
