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
        Text_Touch,
    }

    enum Buttons
    {
        Button_Weapon
    }

    enum Images
    {
        Image_Case,
    }

    private Data.WeaponData weaponInfo;

    protected override void Awake()
    {
        base.Awake();

        BindTexts(typeof(Texts));
        BindButtons(typeof(Buttons));
        BindImages(typeof(Images));

        GetButton((int)Buttons.Button_Weapon).gameObject.BindEvent(OnClickedWeaponButton);

        GetText((int)Texts.Text_Touch).gameObject.SetActive(false);

        GetImage((int)Images.Image_Case).gameObject.SetActive(false);
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

        if(Managers.Game.CurrentWeaponInfo != null && Managers.Game.CurrentWeaponInfo.WeaponNumber == weaponInfo.WeaponNumber)
        {
            GetImage((int)Images.Image_Case).gameObject.SetActive(true);
        }
    }

    private void OnClickedWeaponButton(PointerEventData eventData)
    {
        if (weaponInfo == null) return;

        if(Managers.Game.isTutorial == false)
        {
            Managers.Game.StartWeaponMake(weaponInfo.Stage, weaponInfo.WeaponNumber);
        }
        else if(Managers.Game.isTutorial == true && Managers.Game.tutorialStep == 1)
        {
            Managers.Game.StartWeaponMake(weaponInfo.Stage, weaponInfo.WeaponNumber);
        }
    }

    public GameObject ButtonWeapon()
    {
        return GetButton((int)Buttons.Button_Weapon).gameObject;
    }

    public void ShowTouchText(bool isShow)
    {
        GetText((int)Texts.Text_Touch).gameObject.SetActive(isShow);
    }
}
