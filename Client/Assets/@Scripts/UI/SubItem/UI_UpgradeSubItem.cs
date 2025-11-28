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

            Managers.Player.PlayerStatUpgrade(playerUpgradeData.StatType);
        }
    }
}
