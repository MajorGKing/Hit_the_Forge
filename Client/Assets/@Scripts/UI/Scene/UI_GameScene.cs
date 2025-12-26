using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;


public class UI_GameScene : UI_Scene
{
    #region Enum
    enum GameObjects
    {
        WeaponContent,
        Object_PlayerUpgrade,
        Object_ForgeUpgrade,
        Object_TownUpgrade,
        Object_ShopUpgrade,
        Object_PlayerUpgradeRedDot,
        Object_ForgeUpgradeRedDot,
        Object_TownUpgradeRedDot,
        Object_ShopUpgradeRedDot,
        PlayerUpgradeContent,
        ForgeUpgradeContent,
        TownUpgradeContent,
        ShopUpgradeContent,
        WeaponSelect,
        BannerPosition,
    }

    enum Images
    {
        Image_EnhancementCountDown,
        Image_Help,
    }

    enum Buttons
    {
        Button_ForgeEnhancement,
        Button_ForgeSell,
    }

    enum Texts
    {
        Text_Gold,
        Text_Iron,
        Text_Coal,
        FpsText,
        Text_EnhancementCountDown,
        Text_EnhancementPercent,
        Text_SellPrice,
        Text_ClearWeaponCount, 
    }

    enum Sliders
    {

    }

    enum Toggles
    {
        Toggle_PlayerUpgrade,
        Toggle_ForgeUpgrade,
        Toggle_TownUpgrade,
        Toggle_ShopUpgrade,
    }
    #endregion

    private bool _isSelectedPlayer = false;
    private bool _isSelectedForge = false;
    private bool _isSelectedTown = false;
    private bool _isSelectedShop = false;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));
        BindSliders(typeof(Sliders));
        BindToggles(typeof(Toggles));

        RefreshUI();

        WeaponSelectReset();

        GetButton((int)Buttons.Button_ForgeEnhancement).gameObject.BindEvent(OnClickedForgeEnhancementButton);
        GetButton((int)Buttons.Button_ForgeSell).gameObject.BindEvent(OnClickedForgeSellButton);

        GetToggle((int)Toggles.Toggle_PlayerUpgrade).gameObject.BindEvent(OnClickPlayerToogle);
        GetToggle((int)Toggles.Toggle_ForgeUpgrade).gameObject.BindEvent(OnClickForgeToogle);
        GetToggle((int)Toggles.Toggle_TownUpgrade).gameObject.BindEvent(OnClickTownToogle);
        GetToggle((int)Toggles.Toggle_ShopUpgrade).gameObject.BindEvent(OnClickShopToogle);

        GetImage((int)Images.Image_Help).gameObject.BindEvent(OnClickedHelp);

        TogglesInit();

        //Managers.Ad.SetBannerPosition(new Vector2(0, 1850));
        //Managers.Ad.SetBannerPosition(GetGameObject((int)GameObjects.BannerPosition).transform.position);

        Managers.Ad.LoadBanner();
    }

    private float elapsedTime;
    private float updateInterval = 0.3f;

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= updateInterval)
        {
            float fps = 1.0f / Time.deltaTime;
            float ms = Time.deltaTime * 1000.0f;
            string text = string.Format("{0:N1} FPS ({1:N1}ms)", fps, ms);
            // GetText((int)Texts.FpsText).text = text;

            elapsedTime = 0;
        }
    }

    private void OnEnable()
    {
        Managers.Player.OnCurrenciesChanged -= RefreshUI;
        Managers.Player.OnCurrenciesChanged += RefreshUI;
        Managers.Game.OnEnhancementCountChanged -= RefreshUI;
        Managers.Game.OnEnhancementCountChanged += RefreshUI;
        Managers.Game.OnEnhancementPercentChanged -= RefreshUI;
        Managers.Game.OnEnhancementPercentChanged += RefreshUI;
        Managers.Player.OnPlayerUpgradeChanged -= RefreshUI;
        Managers.Player.OnPlayerUpgradeChanged += RefreshUI;
        Managers.Game.OnNewWeaponAdded -= WeaponSelectReset;
        Managers.Game.OnNewWeaponAdded += WeaponSelectReset;
        Managers.Game.OnNewWeaponAdded -= RefreshUI;
        Managers.Game.OnNewWeaponAdded += RefreshUI;
    }

    private void OnDisable()
    {
        Managers.Player.OnCurrenciesChanged -= RefreshUI;
        Managers.Game.OnEnhancementCountChanged -= RefreshUI;
        Managers.Game.OnEnhancementPercentChanged -= RefreshUI;
        Managers.Player.OnPlayerUpgradeChanged -= RefreshUI;
        Managers.Game.OnNewWeaponAdded -= WeaponSelectReset;
        Managers.Game.OnNewWeaponAdded -= RefreshUI;
    }

    public void SetInfo()
    {

    }

    public void RefreshUI()
    {
        GetText((int)Texts.Text_Gold).text = Managers.Player.GetCurrency(Define.ECurrency.Gold).ToAbbreviatedString();
        GetText((int)Texts.Text_Iron).text = Managers.Player.GetCurrency(Define.ECurrency.Iron).ToAbbreviatedString();
        GetText((int)Texts.Text_Coal).text = Managers.Player.GetCurrency(Define.ECurrency.Coal).ToAbbreviatedString();
 
        GetText((int)Texts.Text_ClearWeaponCount).text = $"{Managers.Player.ClearedWeaponCount} / {Managers.Data.WeaponDict.Count}";

        var countTime = Managers.Game.GetEnhancementCount();
        if(countTime < 0)
        {
            GetImage((int)Images.Image_EnhancementCountDown).gameObject.SetActive(false);
            GetText((int)Texts.Text_EnhancementCountDown).gameObject.SetActive(false);
        }
        else
        {
            GetImage((int)Images.Image_EnhancementCountDown).gameObject.SetActive(true);
            GetText((int)Texts.Text_EnhancementCountDown).gameObject.SetActive(true);

            GetText((int)Texts.Text_EnhancementCountDown).text = ((int)countTime).ToString();
            GetImage((int)Images.Image_EnhancementCountDown).fillAmount = countTime - (int)countTime;
        }

        var enhancementLevel = Managers.Game.GetEnhancementLevel();
        if(enhancementLevel == 0)
        {
            GetText((int)Texts.Text_EnhancementPercent).gameObject.SetActive(false);
            GetText((int)Texts.Text_SellPrice).gameObject.SetActive(false);
        }
        else
        {
            GetText((int)Texts.Text_EnhancementPercent).gameObject.SetActive(true);
            GetText((int)Texts.Text_SellPrice).gameObject.SetActive(true);

            var percent = Managers.Game.GetEnhancementPercent();
            GetText((int)Texts.Text_EnhancementPercent).text = percent.ToString() + "%";

            var price = Managers.Game.GetSellPrice();
            GetText((int)Texts.Text_SellPrice).text = price.ToAbbreviatedString();
        }

        // �÷��̾� ���׷��̵� ������ ����
        if (_isSelectedPlayer == true)
        {
            GetGameObject((int)GameObjects.PlayerUpgradeContent).DestroyChildren();
            var playerLevels = Managers.Player.GetPlayerAllStat();

            foreach (var level in playerLevels)
            {
                Managers.Data.PlayerUpgradeDict.TryGetValue(level, out var data);

                if (data != null)
                {
                    var item = Managers.UI.MakeSubItem<UI_UpgradeSubItem>(GetGameObject((int)GameObjects.PlayerUpgradeContent).transform);

                    item.SetInfo(Define.EUpgradeType.Player, data.TemplateId);
                }
            }
        }
        else if(_isSelectedForge == true)
        {
            GetGameObject((int)GameObjects.ForgeUpgradeContent).DestroyChildren();
            var forgeLevels = Managers.Player.GetForgeAllStat();

            foreach (var level in forgeLevels)
            {
                Managers.Data.ForgeUpgradeDict.TryGetValue(level, out var data);

                if (data != null)
                {
                    var item = Managers.UI.MakeSubItem<UI_UpgradeSubItem>(GetGameObject((int)GameObjects.ForgeUpgradeContent).transform);

                    item.SetInfo(Define.EUpgradeType.Forge, data.TemplateId);
                }
            }
        }
        else if (_isSelectedTown == true)
        {
            GetGameObject((int)GameObjects.TownUpgradeContent).DestroyChildren();
            var townLevels = Managers.Player.GetTownAllStat();

            foreach (var level in townLevels)
            {
                Managers.Data.TownUpgradeDict.TryGetValue(level, out var data);

                if (data != null)
                {
                    var item = Managers.UI.MakeSubItem<UI_UpgradeSubItem>(GetGameObject((int)GameObjects.TownUpgradeContent).transform);

                    item.SetInfo(Define.EUpgradeType.Town, data.TemplateId);
                }
            }
        }
        else if(_isSelectedShop == true)
        {
            GetGameObject((int)GameObjects.ShopUpgradeContent).DestroyChildren();
            var shopProducts = Managers.Player.GetShopAllStat();


            foreach (var product in shopProducts)
            {
                Managers.Data.ShopProductDict.TryGetValue(product, out var data);

                if (data != null)
                {
                    var item = Managers.UI.MakeSubItem<UI_UpgradeSubItem>(GetGameObject((int)GameObjects.ShopUpgradeContent).transform);

                    item.SetInfo(Define.EUpgradeType.Shop, data.TemplateId);
                }
            }

        }
    }

    private void WeaponSelectReset()
    {
        GetGameObject((int)GameObjects.WeaponContent).DestroyChildren();

        foreach(var weaponId in Managers.Player.GetOwnedWeapons().TakeLast(20))
        {
            var item = Managers.UI.MakeSubItem<UI_WeaponSelectSubItem>(GetGameObject((int)GameObjects.WeaponContent).transform);
            item.SetInfo(weaponId);
        }

        GetGameObject((int)GameObjects.WeaponSelect).GetComponent<ScrollRect>().horizontalNormalizedPosition = 1f; 
    }

    private void OnClickedForgeEnhancementButton(PointerEventData eventData)
    {
        Managers.Game.DoEnhancemenet();
    }

    private void OnClickedForgeSellButton(PointerEventData eventData)
    {
        Managers.Game.SellWeapon();
    }

    #region Toggle

    private void TogglesInit()
    {
        GetGameObject((int)GameObjects.Object_PlayerUpgrade).SetActive(false);
        GetGameObject((int)GameObjects.Object_ForgeUpgrade).SetActive(false);
        GetGameObject((int)GameObjects.Object_TownUpgrade).SetActive(false);
        GetGameObject((int)GameObjects.Object_ShopUpgrade).SetActive(false);

        _isSelectedPlayer = false;
        _isSelectedForge = false;
        _isSelectedTown = false;
        _isSelectedShop = false;

        GetGameObject((int)GameObjects.Object_PlayerUpgradeRedDot).SetActive(false);
        GetGameObject((int)GameObjects.Object_ForgeUpgradeRedDot).SetActive(false);
        GetGameObject((int)GameObjects.Object_TownUpgradeRedDot).SetActive(false);
        GetGameObject((int)GameObjects.Object_ShopUpgradeRedDot).SetActive(false);
    }

    private void OnClickPlayerToogle(PointerEventData eventData)
    {
        TogglesInit();

        GetGameObject((int)GameObjects.Object_PlayerUpgrade).SetActive(true);
        _isSelectedPlayer = true;

        RefreshUI();
    }

    private void OnClickForgeToogle(PointerEventData eventData)
    {
        TogglesInit();

        GetGameObject((int)GameObjects.Object_ForgeUpgrade).SetActive(true);
        _isSelectedForge = true;

        RefreshUI();
    }

    private void OnClickTownToogle(PointerEventData eventData)
    {
        TogglesInit();

        GetGameObject((int)GameObjects.Object_TownUpgrade).SetActive(true);
        _isSelectedTown = true;

        RefreshUI();
    }

    private void OnClickShopToogle(PointerEventData eventData)
    {
        TogglesInit();

        GetGameObject((int)GameObjects.Object_ShopUpgrade).SetActive(true);
        _isSelectedShop = true;

        RefreshUI();
    }

    private void OnClickedHelp(PointerEventData eventData)
    {
        Managers.UI.ShowPopupUI<UI_HelpPopup>();
    }
    #endregion
}