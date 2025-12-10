using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

namespace Data
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ExcludeFieldAttribute : Attribute
    {
    }
    #region CreatureData
    [Serializable]
    public class CreatureData
    {
        public int TemplateId;
        public string NameTextID;
        public float ColliderOffsetX;
        public float ColliderOffsetY;
        public float ColliderRadius;
        public float MaxHp;
        public float UpMaxHpBonus;
        public float Atk;
        public float MissChance;
        public float AtkBonus;
        public float MoveSpeed;
        public float CriRate;
        public float CriDamage;
        public string IconImage;
        public string SkeletonDataID;
        public int DefaultSkillId;
        public int EnvSkillId;
        public int SkillAId;
        public int SkillBId;
       
    }

    [Serializable]
    public class CreatureDataLoader : ILoader<int, CreatureData>
    {
        public List<CreatureData> creatures = new List<CreatureData>();
        public Dictionary<int, CreatureData> MakeDict()
        {
            Dictionary<int, CreatureData> dict = new Dictionary<int, CreatureData>();
            foreach (CreatureData creature in creatures)
                dict.Add(creature.TemplateId, creature);
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

    #region TextData
    public class TextData
    {
        public string DataId;
        public string KOR;
    }

    [Serializable]
    public class TextDataLoader : ILoader<string, TextData>
    {
        public List<TextData> texts = new List<TextData>();

        public Dictionary<string, TextData> MakeDict()
        {
            Dictionary<string, TextData> dict = new Dictionary<string, TextData>();
            foreach (TextData text in texts)
                dict.Add(text.DataId, text);

            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

    #region WeaponData
    [Serializable]
    public class WeaponData
    {
        public int TemplateId;
        public string WeaponName;
        public long HP;
        public long Iron;
        public long Coal;
        public long Price;
        public int NextTemplateId;
        public string WeaponImage;
    }

    [Serializable]
    public class WeaponDataLoader : ILoader<int, WeaponData>
    {
        public List<WeaponData> weapons = new List<WeaponData>();
        public Dictionary<int, WeaponData> MakeDict()
        {
            Dictionary<int, WeaponData> dict = new Dictionary<int, WeaponData>();
            foreach (WeaponData weapon in weapons)
                dict.Add(weapon.TemplateId, weapon);
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

    #region EnhancementData
    [Serializable]
    public class EnhancementData
    {
        public int EnhancementLevel;
        public int EnhancementSucess;
        public int BasicSucess;
        public float Price;
    }

    [Serializable]
    public class EnhancementDataLoader : ILoader<int, EnhancementData>
    {
        public List<EnhancementData> enhancements = new List<EnhancementData>();
        public Dictionary<int, EnhancementData> MakeDict()
        {
            Dictionary<int, EnhancementData> dict = new Dictionary<int, EnhancementData>();
            foreach (EnhancementData enhancement in enhancements)
                dict.Add(enhancement.EnhancementLevel, enhancement);
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

    [Serializable]
    public class UpgradeData
    {
        public int TemplateId;
        public string UpgradeName;
        public long Price;
        public long CurrentValue;
        public long NextValue;
        public int OriginalTemplateId;
        public int NextTempalteId;

        //[ExcludeFieldAttribute]
        //public int StatIndex;
    }

    #region PlayerUpgrade
    [Serializable]
    public class PlayerUpgradeData : UpgradeData
    {
        //public int TemplateId;
        //public string UpgradeName;
        public Define.EPlayerStat StatType;
        //public int Price;
        //public int CurrentValue;
        //public int NextValue;
        //public int OriginalTemplateId;
        //public int NextTempalteId;
    }

    [Serializable]
    public class PlayerUpgradeDataLoader : ILoader<int, PlayerUpgradeData>
    {
        public List<PlayerUpgradeData> upgrades = new List<PlayerUpgradeData>();
        public Dictionary<int, PlayerUpgradeData> MakeDict()
        {
            Dictionary<int, PlayerUpgradeData> dict = new Dictionary<int, PlayerUpgradeData>();
            foreach (PlayerUpgradeData upgrade in upgrades)
                dict.Add(upgrade.TemplateId, upgrade);
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

    #region ForgeUpgrade
    [Serializable]
    public class ForgeUpgradeData : UpgradeData
    {
        //public int TemplateId;
        //public string UpgradeName;
        public Define.EPlayerForgeStat StatType;
        //public int Price;
        //public int CurrentValue;
        //public int NextValue;
        //public int OriginalTemplateId;
        //public int NextTempalteId;
    }

    [Serializable]
    public class ForgeUpgradeDataLoader : ILoader<int, ForgeUpgradeData>
    {
        public List<ForgeUpgradeData> upgrades = new List<ForgeUpgradeData>();
        public Dictionary<int, ForgeUpgradeData> MakeDict()
        {
            Dictionary<int, ForgeUpgradeData> dict = new Dictionary<int, ForgeUpgradeData>();
            foreach (ForgeUpgradeData upgrade in upgrades)
                dict.Add(upgrade.TemplateId, upgrade);
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

    #region TownUpgrade
    [Serializable]
    public class TownUpgradeData : UpgradeData
    {
        public Define.EPlayerTownStat StatType;
    }

    [Serializable]
    public class TownUpgradeDataLoader : ILoader<int, TownUpgradeData>
    {
        public List<TownUpgradeData> upgrades = new List<TownUpgradeData>();
        public Dictionary<int, TownUpgradeData> MakeDict()
        {
            Dictionary<int, TownUpgradeData> dict = new Dictionary<int, TownUpgradeData>();
            foreach (TownUpgradeData upgrade in upgrades) 
                dict.Add(upgrade.TemplateId, upgrade);
            
            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

    #region ShopProduct
    [Serializable]
    public class ShopProductData : UpgradeData
    {
        public Define.EShopProductType StatType;
    }

    [Serializable]
    public class ShopProductDataLoader : ILoader<int, ShopProductData>
    {
        public List<ShopProductData> products = new List<ShopProductData>();
        public Dictionary<int, ShopProductData> MakeDict()
        {
            Dictionary<int, ShopProductData> dict = new Dictionary<int, ShopProductData>();
            foreach (ShopProductData product in products)
                dict.Add(product.TemplateId, product);

            return dict;
        }

        public bool Validate()
        {
            return true;
        }
    }
    #endregion

}