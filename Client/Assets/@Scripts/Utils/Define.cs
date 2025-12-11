using System;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class Define
{
    public const char MAP_TOOL_WALL = '0';
    public const char MAP_TOOL_NONE = '1';

    public enum EScene
    {
        Unknown,
        TitleScene,
        LoadingScene,
        GameScene,
    }

    public enum ESound
    {
        Bgm,
        SubBgm,
        Effect,
        Max,
    }

    public enum ETouchEvent
    {
        PointerUp,
        PointerDown,
        Click,
        Pressed,
        BeginDrag,
        Drag,
        EndDrag,
    }

    public enum ELanguage
	{
        Korean,
        English,
        French,
        SimplifiedChinese,
        TraditionalChinese,
        Japanese,
	}

    public enum EEventType
	{
		None,

		OnClickAttackButton,
		OnClickAutoButton,

		InventoryChanged,
		CurrencyChanged,
		StatChanged,
		QuestUpdated,
		CollectionUpdated,
	}

	public enum ELayer
	{
		Default = 0,
		TransparentFX = 1,
		IgnoreRaycast = 2,
		Dummy1 = 3,
		Water = 4,
		UI = 5,
		Hero = 6,
		Monster = 7,
		Boss = 8,
		//
		Env = 11,
		Obstacle = 12,
		//
		Projectile = 20,
	}

    #region Toast
    public enum EToastColor
    {
        Black,
        Red,
        Purple,
        Magenta,
        Blue,
        Green,
        Yellow,
        Orange
    }

    public enum EToastPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }
    #endregion

    public enum ECurrency
    {
        None,
        Gold,
        Iron,
        Coal,
    }

    public enum EUpgradeType
    {
        None,
        Player,
        Forge,
        Town,
        Shop
    }

    public enum EPlayerStat
    {
        Str,
        Skill,
        Dex,
        Mastery,
    }

    public enum EPlayerForgeStat
    {
        CoalTime,
        Skill,
        Mastery,
    }

    public enum EPlayerTownStat
    {
        GoldMax,
        IronMax,
        IronRegeneration,
        CoalMax,
        CoalRegeneration,
        ShopSellBonus,
        ShopBuyBonus,
    }

    public enum EShopProductType
    {
        BuyIron,
        BuyCoal,
        BuyGold,
    }

    public enum EShopBuyType
    {
        Gold,
        Ad,
    }
}