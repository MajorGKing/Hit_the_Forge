using UnityEngine;
using UnityEngine.EventSystems;

public class UI_UpgradeSubItem : UI_SubItem
{
    enum Images
    {
        Image_Upgrade,
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


    protected override void Awake()
    {
        base.Awake();

        BindImages(typeof(Images));
        BindTexts(typeof(Texts));
        BindButtons(typeof(Buttons));

        GetButton((int)Buttons.Button_Upgrade).gameObject.BindEvent(OnClickedUpgradeButton);

        RefreshUI();
    }

    public void SetInfo(Define.EUpgradeType type, int templateId)
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
                GetText((int)Texts.Text_UpgradeDiscribe).text = "터치시 공격력을 올립니다.";
            }
            else if (playerUpgradeData.StatType == Define.EPlayerStat.Skill)
            {
                GetText((int)Texts.Text_UpgradeDiscribe).text = "제품의 품질을 높입니다.";
            }
            else if (playerUpgradeData.StatType == Define.EPlayerStat.Mastery)
            {
                GetText((int)Texts.Text_UpgradeDiscribe).text = "강화 성공확률을 높입니다.";
            }

            GetText((int)Texts.Text_UpgradeStat).text = $"{playerUpgradeData.CurrentValue} > {playerUpgradeData.NextValue}";

            GetText((int)Texts.Text_UpgradePrice).text = playerUpgradeData.Price.ToString();

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
                GetText((int)Texts.Text_UpgradeDiscribe).text = "연료 사용시간을 늘립니다.";
            }
            else if (forgeUpgradeData.StatType == Define.EPlayerForgeStat.Skill)
            {
                GetText((int)Texts.Text_UpgradeDiscribe).text = "제품의 품질을 높입니다.";
            }
            else if (forgeUpgradeData.StatType == Define.EPlayerForgeStat.Mastery)
            {
                GetText((int)Texts.Text_UpgradeDiscribe).text = "강화 성공확률을 높입니다.";
            }

            GetText((int)Texts.Text_UpgradeStat).text = $"{forgeUpgradeData.CurrentValue} > {forgeUpgradeData.NextValue}";

            GetText((int)Texts.Text_UpgradePrice).text = forgeUpgradeData.Price.ToString();

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
                GetText((int)Texts.Text_UpgradeDiscribe).text = "골드 최대 소유량을 늘립니다.";
            }
            else if (townUpgradeData.StatType == Define.EPlayerTownStat.IronMax)
            {
                GetText((int)Texts.Text_UpgradeDiscribe).text = "자원 최대 소유량을 늘립니다.";
            }
            else if (townUpgradeData.StatType == Define.EPlayerTownStat.IronRegeneration)
            {
                GetText((int)Texts.Text_UpgradeDiscribe).text = "자원 생산량을 늘립니다.";
            }
            else if (townUpgradeData.StatType == Define.EPlayerTownStat.CoalMax)
            {
                GetText((int)Texts.Text_UpgradeDiscribe).text = "연로 최대 소유량을 늘립니다.";
            }
            else if (townUpgradeData.StatType == Define.EPlayerTownStat.CoalRegeneration)
            {
                GetText((int)Texts.Text_UpgradeDiscribe).text = "연로 생산량을 늘립니다.";
            }
            else if (townUpgradeData.StatType == Define.EPlayerTownStat.ShopSellBonus)
            {
                GetText((int)Texts.Text_UpgradeDiscribe).text = "판매시 받는 골드 보너스를 늘립니다.";
            }
            else if (townUpgradeData.StatType == Define.EPlayerTownStat.ShopBuyBonus)
            {
                GetText((int)Texts.Text_UpgradeDiscribe).text = "상점 구매시 받는 구매량을 늘립니다.";
            }

            GetText((int)Texts.Text_UpgradeStat).text = $"{townUpgradeData.CurrentValue} > {townUpgradeData.NextValue}";

            GetText((int)Texts.Text_UpgradePrice).text = townUpgradeData.Price.ToString();

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

            if (shopProductData.StatType == Define.EShopProductType.BuyIron)
            {
                GetText((int)Texts.Text_UpgradeDiscribe).text = "재료를 구매 합니다.";
            }
            else if (shopProductData.StatType == Define.EShopProductType.BuyCoal)
            {
                GetText((int)Texts.Text_UpgradeDiscribe).text = "연료를 구매 합니다.";
            }

            GetText((int)Texts.Text_UpgradeStat).text = shopProductData.CurrentValue.ToString();

            GetText((int)Texts.Text_UpgradePrice).text = shopProductData.Price.ToString();
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

            Managers.Player.StatUpgrade(Define.EUpgradeType.Shop, (int)shopProductData.StatType);
        }
    }
}
