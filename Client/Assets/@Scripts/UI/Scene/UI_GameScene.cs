using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;


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
        Image_TutorialFingerEquipment,
        Image_TutorialFingerAnvil,
        Image_TutorialFingerUpgrade
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
        Button_Language,
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
        Text_ButtonLanguage,
        Text_PlayerUpgradeBackToggle,
        Text_ForgeUpgradeBackToggle,
        Text_TownUpgradeBackToggle,
        Text_ShopUpgradeBackToggle,
        Text_PlayerUpgradeToggle,
        Text_ForgeUpgradeToggle,
        Text_TownUpgradeToggle,
        Text_ShopUpgradeToggle,
        Text_Touch,
        Text_Goal,
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

        WeaponSelectReset();

        GetButton((int)Buttons.Button_ForgeEnhancement).gameObject.BindEvent(OnClickedForgeEnhancementButton);
        GetButton((int)Buttons.Button_ForgeSell).gameObject.BindEvent(OnClickedForgeSellButton);
        GetButton((int)Buttons.Button_Language).gameObject.BindEvent(OnClickedLanguageButton);

        GetToggle((int)Toggles.Toggle_PlayerUpgrade).gameObject.BindEvent(OnClickPlayerToogle);
        GetToggle((int)Toggles.Toggle_ForgeUpgrade).gameObject.BindEvent(OnClickForgeToogle);
        GetToggle((int)Toggles.Toggle_TownUpgrade).gameObject.BindEvent(OnClickTownToogle);
        GetToggle((int)Toggles.Toggle_ShopUpgrade).gameObject.BindEvent(OnClickShopToogle);

        GetImage((int)Images.Image_Help).gameObject.BindEvent(OnClickedHelp);

        TogglesInit();

        GetGameObject((int)GameObjects.Object_PlayerUpgrade).SetActive(true);
        _isSelectedPlayer = true;

        GetText((int)Texts.Text_Touch).gameObject.SetActive(false);
        GetText((int)Texts.Text_Goal).gameObject.SetActive(false);

        RefreshUI();
        RefreshUpgradeUI();

        GetGameObject((int)GameObjects.Image_TutorialFingerEquipment).gameObject.SetActive(false);
        GetGameObject((int)GameObjects.Image_TutorialFingerAnvil).gameObject.SetActive(false);
        GetGameObject((int)GameObjects.Image_TutorialFingerUpgrade).gameObject.SetActive(false);

        //Managers.Ad.SetBannerPosition(new Vector2(0, 1850));
        //Managers.Ad.SetBannerPosition(GetGameObject((int)GameObjects.BannerPosition).transform.position);

        Managers.Ad.LoadBanner();

        // SceneUI
        Managers.UI.SceneUI = this;
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
        Managers.Player.OnPlayerUpgradeChanged -= RefreshUpgradeUI;
        Managers.Player.OnPlayerUpgradeChanged += RefreshUpgradeUI;
        Managers.Game.OnNewWeaponAdded -= WeaponSelectReset;
        Managers.Game.OnNewWeaponAdded += WeaponSelectReset;
        Managers.Game.OnNewWeaponAdded -= RefreshUI;
        Managers.Game.OnNewWeaponAdded += RefreshUI;
        Managers.Player.OnLanguageChange -= RefreshUI;
        Managers.Player.OnLanguageChange += RefreshUI;
        Managers.Player.OnLanguageChange -= RefreshUpgradeUI;
        Managers.Player.OnLanguageChange += RefreshUpgradeUI;
    }

    private void OnDisable()
    {
        Managers.Player.OnCurrenciesChanged -= RefreshUI;
        Managers.Game.OnEnhancementCountChanged -= RefreshUI;
        Managers.Game.OnEnhancementPercentChanged -= RefreshUI;
        Managers.Player.OnPlayerUpgradeChanged -= RefreshUI;
        Managers.Player.OnPlayerUpgradeChanged -= RefreshUpgradeUI;
        Managers.Game.OnNewWeaponAdded -= WeaponSelectReset;
        Managers.Game.OnNewWeaponAdded -= RefreshUI;
        Managers.Player.OnLanguageChange -= RefreshUI;
        Managers.Player.OnLanguageChange -= RefreshUpgradeUI;
    }

    public void SetInfo()
    {

    }

    public void RefreshUI()
    {
        GetText((int)Texts.Text_Gold).text = Managers.Player.GetCurrency(Define.ECurrency.Gold).ToAbbreviatedString();
        GetText((int)Texts.Text_Iron).text = Managers.Player.GetCurrency(Define.ECurrency.Iron).ToAbbreviatedString();
        GetText((int)Texts.Text_Coal).text = Managers.Player.GetCurrency(Define.ECurrency.Coal).ToAbbreviatedString();
 
        int totalWeapons = 0;
        if (Managers.Data.WeaponDict.TryGetValue(Managers.Player.Stage, out var stageWeapons))
            totalWeapons = stageWeapons.Count;

        GetText((int)Texts.Text_ClearWeaponCount).text = $"{Managers.Player.ClearedWeaponCount} / {totalWeapons}";
        GetText((int)Texts.Text_ButtonLanguage).text = Managers.GetText("GameLanguage");
        
        GetText((int)Texts.Text_PlayerUpgradeBackToggle).text = Managers.GetText("PlayerUpgradeToggle");
        GetText((int)Texts.Text_PlayerUpgradeToggle).text = Managers.GetText("PlayerUpgradeToggle");

        GetText((int)Texts.Text_ForgeUpgradeBackToggle).text = Managers.GetText("ForgeUpgradeToggle");
        GetText((int)Texts.Text_ForgeUpgradeToggle).text = Managers.GetText("ForgeUpgradeToggle");

        GetText((int)Texts.Text_TownUpgradeBackToggle).text = Managers.GetText("TownUpgradeToggle");
        GetText((int)Texts.Text_TownUpgradeToggle).text = Managers.GetText("TownUpgradeToggle");

        GetText((int)Texts.Text_ShopUpgradeBackToggle).text = Managers.GetText("ShopUpgradeToggle");
        GetText((int)Texts.Text_ShopUpgradeToggle).text = Managers.GetText("ShopUpgradeToggle");


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
    }

    private void RefreshUpgradeUI()
    {
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
        if(Managers.Game.isTutorial == false)
        {
            Managers.Game.DoEnhancemenet();
        }
        
    }

    private void OnClickedForgeSellButton(PointerEventData eventData)
    {
        Managers.Game.SellWeapon();
    }

    private void OnClickedLanguageButton(PointerEventData eventData)
    {
        Managers.Player.ChangeLanguage();
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
        if(Managers.Game.isTutorial)
            return;

        TogglesInit();

        GetGameObject((int)GameObjects.Object_PlayerUpgrade).SetActive(true);
        _isSelectedPlayer = true;

        RefreshUI();
        RefreshUpgradeUI();
    }

    private void OnClickForgeToogle(PointerEventData eventData)
    {
        if(Managers.Game.isTutorial)
            return;

        TogglesInit();

        GetGameObject((int)GameObjects.Object_ForgeUpgrade).SetActive(true);
        _isSelectedForge = true;

        RefreshUI();
        RefreshUpgradeUI();
    }

    private void OnClickTownToogle(PointerEventData eventData)
    {
        if(Managers.Game.isTutorial)
            return;

        TogglesInit();

        GetGameObject((int)GameObjects.Object_TownUpgrade).SetActive(true);
        _isSelectedTown = true;

        RefreshUI();
        RefreshUpgradeUI();
    }

    private void OnClickShopToogle(PointerEventData eventData)
    {
        if(Managers.Game.isTutorial)
            return;

        TogglesInit();

        GetGameObject((int)GameObjects.Object_ShopUpgrade).SetActive(true);
        _isSelectedShop = true;

        RefreshUI();
        RefreshUpgradeUI();
    }

    private void OnClickedHelp(PointerEventData eventData)
    {
        Managers.UI.ShowPopupUI<UI_HelpPopup>();
    }
    #endregion

    #region Tutorial
    public void DoTutorial(int step)
    {
        if(step == 1)
        {
            // 무기만 활성화
            GameObject weaponContent = GetGameObject((int)GameObjects.WeaponContent);
            List<GameObject> children = new List<GameObject>();
            foreach (Transform child in weaponContent.transform)
            {
                var weapon = child.GetComponent<UI_WeaponSelectSubItem>();
                var button = weapon.ButtonWeapon();
                weapon.ShowTouchText(true);
                children.Add(button);
            }

            Managers.Touch.AllowOnly(children.ToArray());

            GetText((int)Texts.Text_Goal).gameObject.SetActive(true);
            GetText((int)Texts.Text_Goal).text = Managers.GetText("Tutorial1");

            GetGameObject((int)GameObjects.Image_TutorialFingerEquipment).gameObject.SetActive(true);
        }
        else if(step == 2)
        {
            GetGameObject((int)GameObjects.Image_TutorialFingerEquipment).gameObject.SetActive(false);
            GetGameObject((int)GameObjects.Image_TutorialFingerAnvil).gameObject.SetActive(true);

            GameObject weaponContent = GetGameObject((int)GameObjects.WeaponContent);
            foreach (Transform child in weaponContent.transform)
            {
                var weapon = child.GetComponent<UI_WeaponSelectSubItem>();
                weapon.ShowTouchText(false);
            }

            GetText((int)Texts.Text_Goal).gameObject.SetActive(true);
            GetText((int)Texts.Text_Goal).text = Managers.GetText("Tutorial2");

            GetText((int)Texts.Text_Touch).gameObject.SetActive(true);
        }
        else if(step == 3)
        {
            GetText((int)Texts.Text_Goal).gameObject.SetActive(true);
            GetText((int)Texts.Text_Goal).text = Managers.GetText("Tutorial3");

            GetText((int)Texts.Text_Touch).gameObject.SetActive(false);

            // 공격력 업그레이드 에만 터치 텍스트 뜨도록
            GameObject playerUpgradeContent = GetGameObject((int)GameObjects.PlayerUpgradeContent);
            List<GameObject> children = new List<GameObject>();
            foreach (Transform child in playerUpgradeContent.transform)
            {
                var upgradeItem = child.GetComponent<UI_UpgradeSubItem>();
                if (upgradeItem == null) continue;

                if (upgradeItem.UpgradeType == Define.EUpgradeType.Player && 
                    upgradeItem.PlayerUpgradeData != null && 
                    upgradeItem.PlayerUpgradeData.StatType == Define.EPlayerStat.Str)
                {
                    upgradeItem.ShowTouchText(true);
                    children.Add(upgradeItem.GetUpgradeButton());
                }
            }

            Managers.Touch.AllowOnly(children.ToArray());

            GetGameObject((int)GameObjects.Image_TutorialFingerAnvil).gameObject.SetActive(false);
            GetGameObject((int)GameObjects.Image_TutorialFingerUpgrade).gameObject.SetActive(true);
        }
        else if(step == 4)
        {
            GetGameObject((int)GameObjects.Image_TutorialFingerAnvil).gameObject.SetActive(true);
            GetGameObject((int)GameObjects.Image_TutorialFingerUpgrade).gameObject.SetActive(false);

            GetText((int)Texts.Text_Goal).gameObject.SetActive(true);
            GetText((int)Texts.Text_Goal).text = Managers.GetText("Tutorial4");

            GetText((int)Texts.Text_Touch).gameObject.SetActive(true);
        }
        else if(step == 5)
        {
            GetText((int)Texts.Text_Goal).gameObject.SetActive(false);
            GetText((int)Texts.Text_Touch).gameObject.SetActive(false);

            GetGameObject((int)GameObjects.Image_TutorialFingerEquipment).gameObject.SetActive(false);
            GetGameObject((int)GameObjects.Image_TutorialFingerAnvil).gameObject.SetActive(false);
            GetGameObject((int)GameObjects.Image_TutorialFingerUpgrade).gameObject.SetActive(false);
        }

    }
    #endregion
}