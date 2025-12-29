using UnityEngine;
using UnityEngine.EventSystems;

public class UI_WeaponSelectSubItem : UI_SubItem
{
    enum Texts
    {
        Text_NeedIron,
        Text_NeedCoal,
        Text_GetPrice,
        Text_Name,
    }

    enum Buttons
    {
        Button_Weapon
    }

    private Data.WeaponData weaponInfo;

    protected override void Awake()
    {
        base.Awake();

        BindTexts(typeof(Texts));
        BindButtons(typeof(Buttons));

        GetButton((int)Buttons.Button_Weapon).gameObject.BindEvent(OnClickedWeaponButton);
    }

    public void SetInfo(int weaponNumber)
    {
        if (Managers.Data.WeaponDict.TryGetValue(Managers.Player.Stage, out var stageWeapons))
        {
            stageWeapons.TryGetValue(weaponNumber, out weaponInfo);
        }
         RefreshUI();
    }

    private void RefreshUI()
    {
        if (weaponInfo == null)
            return;

        GetText((int)Texts.Text_NeedIron).text = weaponInfo.Iron.ToAbbreviatedString();
        GetText((int)Texts.Text_NeedCoal).text = weaponInfo.Coal.ToAbbreviatedString();
        GetText((int)Texts.Text_GetPrice).text = weaponInfo.Price.ToAbbreviatedString();
        GetText((int)Texts.Text_Name).text = weaponInfo.WeaponName;

        GetButton((int)Buttons.Button_Weapon).image.sprite = Managers.Resource.Load<Sprite>(weaponInfo.WeaponImage);
    }

    private void OnClickedWeaponButton(PointerEventData eventData)
    {
        if (weaponInfo == null) return;
        Managers.Game.StartWeaponMake(weaponInfo.Stage, weaponInfo.WeaponNumber);
    }
}
