using System;


public class PlayerManager
{
    public int[] currency = new int[Enum.GetValues(typeof(Define.ECurrency)).Length];
    public int[] maxCurrency = new int[Enum.GetValues(typeof(Define.ECurrency)).Length];
    public int[] playerStat = new int[Enum.GetValues(typeof(Define.EPlayerStat)).Length];
    public int[] forgeStat = new int[Enum.GetValues(typeof(Define.EPlayerForgeStat)).Length];

    public void Clear()
    {

    }

    public void Init()
    {
        for (int i = 0; i < maxCurrency.Length; i++)
        {
            maxCurrency[i] = 10000;
        }

        currency[(int)Define.ECurrency.Gold] = 0;
        currency[(int)Define.ECurrency.Iron] = 5000;
        currency[(int)Define.ECurrency.Coal] = 5000;

        playerStat[(int)Define.EPlayerStat.Str] = 10;
        playerStat[(int)Define.EPlayerStat.Skill] = 10;
        playerStat[(int)Define.EPlayerStat.Dex] = 10;

        forgeStat[(int)Define.EPlayerForgeStat.CoalTime] = 1000;
    }

    public void CurrencyAdd(Define.ECurrency type, int value)
    {
        if (value <= 0)
            return;
        int index = (int)type;

        int oldValue = currency[index];
        int newValue = oldValue + value;

        if (newValue > maxCurrency[index])
        {
            newValue = maxCurrency[index];
        }

        currency[index] = newValue;

        if (oldValue != newValue)
        {
            OnCurrenciesChagned?.Invoke();
        }
    }

    #region Action
    public event Action OnCurrenciesChagned;
    #endregion
}
