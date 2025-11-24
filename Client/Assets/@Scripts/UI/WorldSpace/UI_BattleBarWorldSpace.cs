using UnityEngine;

public class UI_BattleBarWorldSpace : UI_WorldSpace
{
    private enum Texts
    {
        Text_Hp,
    }

    private enum Sliders
    {
        Slider_HP,
        //Slider_Skill_Guage,
    }

    private int _hp;
    private int _maxHP;

    private float _coolTime;
    private float _maxCoolTime;

    private bool _showMaxHp;

    protected override void Awake()
    {
        base.Awake();

        // Bind
        BindTexts(typeof(Texts));
        BindSliders(typeof(Sliders));

        //GetSlider((int)Sliders.Slider_Skill_Guage).gameObject.SetActive(false);
        GetSlider((int)Sliders.Slider_HP).gameObject.SetActive(true);
    }

    public void SetInfo(int hp, int maxHP, bool showMaxHp = false)
    {
        //if (isInit == false)
        //    return;

        GetSlider((int)Sliders.Slider_HP).gameObject.SetActive(true);

        _hp = hp;
        _maxHP = maxHP;

        _showMaxHp = showMaxHp;

        RefreshUI();
    }

    public void SetInfo(float coolTime, float maxCoolTime)
    {
        //GetSlider((int)Sliders.Slider_Skill_Guage).gameObject.SetActive(true);

        _coolTime = coolTime;
        _maxCoolTime = maxCoolTime;

        RefreshUI();
    }

    public void RefreshUI()
    {
        // GetSlider((int)Sliders.Slider_HP).value = (float)_hp / _maxHP;
        // GetText((int)Texts.Text_Hp).text = $"{_hp:N0}";

        // if(_showMaxHp == true)
        // {
        _hp = Managers.Game.WeaponHp;
        _maxHP = Managers.Game.WeaponMaxHp;


            GetText((int)Texts.Text_Hp).text = $"{_hp:N0} / {_maxHP:N0}";

            GetSlider((int)Sliders.Slider_HP).value = (float)_hp / _maxHP;
        // }

        //GetSlider((int)Sliders.Slider_Skill_Guage).value = _coolTime / _maxCoolTime;
    }

    private void OnEnable() 
    {
        Managers.Game.OnWeaponHpChagned -= RefreshUI;
        Managers.Game.OnWeaponHpChagned += RefreshUI;
    }

    private void OnDisable() 
    {
        Managers.Game.OnWeaponHpChagned -= RefreshUI;
    }
}
