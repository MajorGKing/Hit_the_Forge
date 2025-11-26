using System.Linq;
using Data;
using UnityEngine;
using UnityEngine.EventSystems;


public class UI_GameScene : UI_Scene
{
    #region Enum
    enum GameObjects
    {
        WeaponContent
    }

    enum Images
    {
        Image_EnhancementCountDown,
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
    }

    enum Sliders
    {

    }

    #endregion

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));
        BindSliders(typeof(Sliders));

        RefreshUI();

        WeaponSelectReset();

        GetButton((int)Buttons.Button_ForgeEnhancement).gameObject.BindEvent(OnClickedForgeEnhancementButton);
        GetButton((int)Buttons.Button_ForgeSell).gameObject.BindEvent(OnClickedForgeSellButton);
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
        Managers.Player.OnCurrenciesChagned -= RefreshUI;
        Managers.Player.OnCurrenciesChagned += RefreshUI;
        Managers.Game.OnEnhancementCountChanged -= RefreshUI;
        Managers.Game.OnEnhancementCountChanged += RefreshUI;
        Managers.Game.OnEnhancementPercentChanged -= RefreshUI;
        Managers.Game.OnEnhancementPercentChanged += RefreshUI;
    }

    private void OnDisable()
    {
        Managers.Player.OnCurrenciesChagned -= RefreshUI;
        Managers.Game.OnEnhancementCountChanged -= RefreshUI;
        Managers.Game.OnEnhancementPercentChanged -= RefreshUI;
    }

    public void SetInfo()
    {

    }

    public void RefreshUI()
    {
        GetText((int)Texts.Text_Gold).text = Managers.Player.GetCurrency(Define.ECurrency.Gold).ToString();
        GetText((int)Texts.Text_Iron).text = Managers.Player.GetCurrency(Define.ECurrency.Iron).ToString();
        GetText((int)Texts.Text_Coal).text = Managers.Player.GetCurrency(Define.ECurrency.Coal).ToString();

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
            GetText((int)Texts.Text_EnhancementPercent).text = percent.ToString();

            var price = Managers.Game.GetSellPrice();
            GetText((int)Texts.Text_SellPrice).text = price.ToString();
        }
    }

    private void WeaponSelectReset()
    {
        GetGameObject((int)GameObjects.WeaponContent).DestroyChildren();

        foreach(var weapon in Managers.Data.WeaponDict.Values)
        {
            var item = Managers.UI.MakeSubItem<UI_WeaponSelectSubItem>(GetGameObject((int)GameObjects.WeaponContent).transform);
            item.SetInfo(weapon.TemplateId);
        }
    }

    private void OnClickedForgeEnhancementButton(PointerEventData eventData)
    {
        Managers.Game.DoEnhancemenet();
    }

    private void OnClickedForgeSellButton(PointerEventData eventData)
    {
        Managers.Game.SellWeapon();
    }
}