using Unity.Services.LevelPlay;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_UpgradeSubItem : UI_SubItem
{
    enum Images
    {
        Image_Upgrade,
        Image_CostType,
    }

    enum Texts
    {
        Text_UpgradeDiscribe,
        Text_UpgradeStat,
        Text_UpgradePrice,
    }

    enum Buttons
    {
        Button_Upgrade,
    }

    private Define.EUpgradeType upgradeType = Define.EUpgradeType.None;
    private Data.PlayerUpgradeData playerUpgradeData = null;
    private Data.ForgeUpgradeData forgeUpgradeData = null;
    private Data.TownUpgradeData townUpgradeData = null;
    private Data.ShopProductData shopProductData = null;

    private void OnDisable()
    {
        Managers.Ad.OnRewardedLoaded -= AdReady;
        Managers.Ad.OnRewardedLoadFailed -= AdLoadFailed;
    }


    protected override void Awake()
    {
        base.Awake();

        BindImages(typeof(Images));
        BindTexts(typeof(Texts));
        BindButtons(typeof(Buttons));

        GetButton((int)Buttons.Button_Upgrade).gameObject.BindEvent(OnClickedUpgradeButton);

        RefreshUI();
    }

    public void SetInfo(Define.EUpgradeType type, long templateId)
    {
        upgradeType = type;

        if(type == Define.EUpgradeType.Player)
        {
            Managers.Data.PlayerUpgradeDict.TryGetValue(templateId, out playerUpgradeData);
        }
        else if(type == Define.EUpgradeType.Forge)
        {
            Managers.Data.ForgeUpgradeDict.TryGetValue(templateId, out forgeUpgradeData);
        }
        else if(type == Define.EUpgradeType.Town)
        {
            Managers.Data.TownUpgradeDict.TryGetValue(templateId, out townUpgradeData);
        }
        else if(type == Define.EUpgradeType.Shop)
        {
            Managers.Data.ShopProductDict.TryGetValue(templateId, out shopProductData);

            if (shopProductData != null && shopProductData.BuyType == Define.EShopBuyType.Ad)
            {
                Managers.Ad.OnRewardedLoaded -= AdReady;
                Managers.Ad.OnRewardedLoaded += AdReady;
                Managers.Ad.OnRewardedLoadFailed -= AdLoadFailed;
                Managers.Ad.OnRewardedLoadFailed += AdLoadFailed;
            }
        }
        

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (upgradeType == Define.EUpgradeType.None)
            return;

        if(upgradeType == Define.EUpgradeType.Player)
        {
            if (playerUpgradeData == null)
                return;

            // TODO Image based on type

            if (playerUpgradeData.StatType == Define.EPlayerStat.Str)
            {
                var increasesAttackPower = Managers.GetText("IncreasesAttackPower");
                GetText((int)Texts.Text_UpgradeDiscribe).text = $"{increasesAttackPower}.";
            }
            else if (playerUpgradeData.StatType == Define.EPlayerStat.Skill)
            {
                var increasesProductQuality = Managers.GetText("IncreasesProductQuality");
                GetText((int)Texts.Text_UpgradeDiscribe).text = $"{increasesProductQuality}.";
            }
            else if (playerUpgradeData.StatType == Define.EPlayerStat.Mastery)
            {
                var increasesEnhancementSuccessRate = Managers.GetText("IncreasesEnhancementSuccessRate");
                GetText((int)Texts.Text_UpgradeDiscribe).text = $"{increasesEnhancementSuccessRate}.";
            }

            GetText((int)Texts.Text_UpgradeStat).text = $"{playerUpgradeData.CurrentValue.ToAbbreviatedString()} > {playerUpgradeData.NextValue.ToAbbreviatedString()}";

            GetText((int)Texts.Text_UpgradePrice).text = playerUpgradeData.Price.ToAbbreviatedString();

            if (playerUpgradeData.NextTempalteId == 0)
            {
                GetButton((int)Buttons.Button_Upgrade).gameObject.SetActive(false);
            }
        }
        else if (upgradeType == Define.EUpgradeType.Forge)
        {
            if (forgeUpgradeData == null)
                return;

            // TODO Image based on type

            if (forgeUpgradeData.StatType == Define.EPlayerForgeStat.CoalTime)
            {
                var ExtendsFuelUsageTime = Managers.GetText("ExtendsFuelUsageTime");
                GetText((int)Texts.Text_UpgradeDiscribe).text = $"{ExtendsFuelUsageTime}.";
            }
            else if (forgeUpgradeData.StatType == Define.EPlayerForgeStat.Skill)
            {
                var increasesProductQuality = Managers.GetText("IncreasesProductQuality");
                GetText((int)Texts.Text_UpgradeDiscribe).text = $"{increasesProductQuality}.";
            }
            else if (forgeUpgradeData.StatType == Define.EPlayerForgeStat.Mastery)
            {
                var increasesEnhancementSuccessRate = Managers.GetText("IncreasesEnhancementSuccessRate");
                GetText((int)Texts.Text_UpgradeDiscribe).text = $"{increasesEnhancementSuccessRate}.";
            }

            GetText((int)Texts.Text_UpgradeStat).text = $"{forgeUpgradeData.CurrentValue.ToAbbreviatedString()} > {forgeUpgradeData.NextValue.ToAbbreviatedString()}";

            GetText((int)Texts.Text_UpgradePrice).text = forgeUpgradeData.Price.ToAbbreviatedString();

            if (forgeUpgradeData.NextTempalteId == 0)
            {
                GetButton((int)Buttons.Button_Upgrade).gameObject.SetActive(false);
            }
        }
        else if (upgradeType == Define.EUpgradeType.Town)
        {
            if (townUpgradeData == null)
                return;

            // TODO Image based on type

            if (townUpgradeData.StatType == Define.EPlayerTownStat.GoldMax)
            {
                var increasesMaxGoldCapacity = Managers.GetText("IncreasesMaxGoldCapacity");
                GetText((int)Texts.Text_UpgradeDiscribe).text = $"{increasesMaxGoldCapacity}.";
            }
            else if (townUpgradeData.StatType == Define.EPlayerTownStat.IronMax)
            {
                var increasesMaxMaterialCapacity = Managers.GetText("IncreasesMaxMaterialCapacity");
                GetText((int)Texts.Text_UpgradeDiscribe).text = $"{increasesMaxMaterialCapacity}.";
            }
            else if (townUpgradeData.StatType == Define.EPlayerTownStat.IronRegeneration)
            {
                var increasesMaterialProduction = Managers.GetText("IncreasesMaterialProduction");
                GetText((int)Texts.Text_UpgradeDiscribe).text = $"{increasesMaterialProduction}.";
            }
            else if (townUpgradeData.StatType == Define.EPlayerTownStat.CoalMax)
            {
                var increasesMaxFuelCapacity = Managers.GetText("IncreasesMaxFuelCapacity");
                GetText((int)Texts.Text_UpgradeDiscribe).text = $"{increasesMaxFuelCapacity}.";
            }
            else if (townUpgradeData.StatType == Define.EPlayerTownStat.CoalRegeneration)
            {
                var increasesFuelProduction = Managers.GetText("IncreasesFuelProduction");
                GetText((int)Texts.Text_UpgradeDiscribe).text = $"{increasesFuelProduction}.";
            }
            else if (townUpgradeData.StatType == Define.EPlayerTownStat.ShopSellBonus)
            {
                var increasesGoldBonuswhenSellingItems = Managers.GetText("IncreasesGoldBonuswhenSellingItems");
                GetText((int)Texts.Text_UpgradeDiscribe).text = $"{increasesGoldBonuswhenSellingItems}.";
            }
            else if (townUpgradeData.StatType == Define.EPlayerTownStat.ShopBuyBonus)
            {
                var increasesPurchaseAmountFromtheShop = Managers.GetText("IncreasesPurchaseAmountFromtheShop");
                GetText((int)Texts.Text_UpgradeDiscribe).text = $"{increasesPurchaseAmountFromtheShop}.";
            }

            GetText((int)Texts.Text_UpgradeStat).text = $"{townUpgradeData.CurrentValue.ToAbbreviatedString()} > {townUpgradeData.NextValue.ToAbbreviatedString()}";

            GetText((int)Texts.Text_UpgradePrice).text = townUpgradeData.Price.ToAbbreviatedString();

            if (townUpgradeData.NextTempalteId == 0)
            {
                GetButton((int)Buttons.Button_Upgrade).gameObject.SetActive(false);
            }
        }
        else if (upgradeType == Define.EUpgradeType.Shop)
        {
            if (shopProductData == null)
                return;

            // TODO Image based on type

            if(shopProductData.BuyType == Define.EShopBuyType.Gold)
            {
                var usingGold = Managers.GetText("UsingGold");
                GetText((int)Texts.Text_UpgradeDiscribe).text = $"{usingGold} ";
            }
            else if(shopProductData.BuyType == Define.EShopBuyType.Ad)
            {
                var watchingAds = Managers.GetText("WatchingAds");
                GetText((int)Texts.Text_UpgradeDiscribe).text = $"{watchingAds} ";
                GetImage((int)Images.Image_CostType).sprite = null;
            }

            long maxValue = 0;
            long buyValue = 0;
            long price = 0;

            if (shopProductData.StatType == Define.EShopProductType.BuyIron)
            {
                var toBuyMaterials = Managers.GetText("ToBuyMaterials");
                GetText((int)Texts.Text_UpgradeDiscribe).text += $"{toBuyMaterials}.";
                maxValue = Managers.Player.GetCurrenyMax(Define.ECurrency.Iron);
                
            }
            else if (shopProductData.StatType == Define.EShopProductType.BuyCoal)
            {
                var toBuyFuel = Managers.GetText("ToBuyFuel");
                GetText((int)Texts.Text_UpgradeDiscribe).text += $"{toBuyFuel}.";
                maxValue = Managers.Player.GetCurrenyMax(Define.ECurrency.Coal);
            }
            else if (shopProductData.StatType == Define.EShopProductType.BuyGold)
            {
                var toBuyGold = Managers.GetText("ToBuyGold");
                GetText((int)Texts.Text_UpgradeDiscribe).text += $"toBuyGold.";
                maxValue = Managers.Player.GetCurrenyMax(Define.ECurrency.Gold);
            }

            buyValue = maxValue * shopProductData.CurrentValue / 100;
            price = buyValue * shopProductData.Price;

            GetText((int)Texts.Text_UpgradeStat).text = buyValue.ToAbbreviatedString();

            if(shopProductData.BuyType == Define.EShopBuyType.Gold)
            {
                GetText((int)Texts.Text_UpgradePrice).text = price.ToAbbreviatedString();
            }
            else if(shopProductData.BuyType == Define.EShopBuyType.Ad)
            {
                GetText((int)Texts.Text_UpgradePrice).text = "AD";

                if(shopProductData.StatType == Define.EShopProductType.BuyGold)
                {
                    GetButton((int)Buttons.Button_Upgrade).gameObject.SetActive(Managers.Ad.IsRewardedReady(Define.ECurrency.Gold));
                }
                else if (shopProductData.StatType == Define.EShopProductType.BuyIron)
                {
                    GetButton((int)Buttons.Button_Upgrade).gameObject.SetActive(Managers.Ad.IsRewardedReady(Define.ECurrency.Iron));
                }
                else if (shopProductData.StatType == Define.EShopProductType.BuyCoal)
                {
                    GetButton((int)Buttons.Button_Upgrade).gameObject.SetActive(Managers.Ad.IsRewardedReady(Define.ECurrency.Coal));
                }
            }
        }
    }

    private void OnClickedUpgradeButton(PointerEventData eventData)
    {
        if (upgradeType == Define.EUpgradeType.None)
            return;

        if (upgradeType == Define.EUpgradeType.Player)
        {

            if (playerUpgradeData == null)
                return;

            Managers.Player.StatUpgrade(Define.EUpgradeType.Player ,(int)playerUpgradeData.StatType);
        }
        else if (upgradeType == Define.EUpgradeType.Forge)
        {

            if (forgeUpgradeData == null)
                return;

            Managers.Player.StatUpgrade(Define.EUpgradeType.Forge, (int)forgeUpgradeData.StatType);
        }
        else if (upgradeType == Define.EUpgradeType.Town)
        {
            if (townUpgradeData == null)
                return;

            Managers.Player.StatUpgrade(Define.EUpgradeType.Town, (int)townUpgradeData.StatType);
        }
        else if(upgradeType == Define.EUpgradeType.Shop)
        {
            if(shopProductData == null)
                return;

            if (shopProductData.BuyType == Define.EShopBuyType.Ad)
            {
                GetButton((int)Buttons.Button_Upgrade).gameObject.SetActive(false);
            }

            Managers.Player.ProcessShopPurchase(shopProductData.TemplateId);
        }
    }

    private void AdReady(Define.ECurrency currency, LevelPlayAdInfo info)
    {
        if (shopProductData == null) return;

        Define.ECurrency myCurrency = shopProductData.StatType switch
        {
            Define.EShopProductType.BuyIron => Define.ECurrency.Iron,
            Define.EShopProductType.BuyCoal => Define.ECurrency.Coal,
            Define.EShopProductType.BuyGold => Define.ECurrency.Gold,
            _ => Define.ECurrency.None,
        };

        if (myCurrency == currency)
        {
            GetButton((int)Buttons.Button_Upgrade).gameObject.SetActive(true);
        }
    }

    private void AdLoadFailed(Define.ECurrency currency, string error)
    {
        if (shopProductData == null) return;

        Define.ECurrency myCurrency = shopProductData.StatType switch
        {
            Define.EShopProductType.BuyIron => Define.ECurrency.Iron,
            Define.EShopProductType.BuyCoal => Define.ECurrency.Coal,
            Define.EShopProductType.BuyGold => Define.ECurrency.Gold,
            _ => Define.ECurrency.None,
        };

        if (myCurrency == currency)
        {
            GetButton((int)Buttons.Button_Upgrade).gameObject.SetActive(false);
        }
    }
}
